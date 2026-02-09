using System.Collections;
using BatteringRam.Components;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.ServerEvents;
using MEC;
using RueI.API;
using RueI.API.Elements;
using UnityEngine;
using events = Exiled.Events.Handlers;
using Light = Exiled.API.Features.Toys.Light;

namespace BatteringRam.Variants
{
    public class BatteringRamCom18 : BatteringRamComponent
    {
        public override uint Id { get; set; } = 60;
        public override string Name { get; set; } = "Дверной таран";
        public override string Description { get; set; }
        public override ItemType Type { get; set; } = ItemType.GunCOM18;
        public override float Damage { get; set; } = 15;
        
        public override bool EnableHighlight { get; set; } = true;
        public override string HighlightColor { get; set; } = "#eb1700";
        public string HighlightSecondColor { get; set; } = "#8100eb";
        public override float HighlightRange { get; set; } = 0.7f;
        public override float HighlightIntensity { get; set; } = 4f;
    
        public override bool EnableParticles { get; set; } = true;
        public override Vector3 SpawnRange { get; set; } = new(0.7f, 0.7f, 0.7f);
        public override float ParticleSize { get; set; } = 0.1f;
        public override ushort Intensity { get; set; } = 5;

        protected override void SubscribeEvents()
        {
            events.Player.Shot += OnShot;
            events.Player.ChangedItem += OnChangedItem;
            events.Player.DroppedItem += OnDroppedItem;
            LabApi.Events.Handlers.ServerEvents.PickupCreated += OnPickupCreated;
        }

        protected override void UnsubscribeEvents()
        {
            events.Player.Shot -= OnShot;
            events.Player.ChangedItem -= OnChangedItem;
            events.Player.DroppedItem -= OnDroppedItem;
            LabApi.Events.Handlers.ServerEvents.PickupCreated -= OnPickupCreated;
        }
        
        private void OnPickupCreated(PickupCreatedEventArgs ev) => HighlightItemDouble(Pickup.Get(ev.Pickup.GameObject));
        private void OnDroppedItem(DroppedItemEventArgs ev) => HighlightItemDouble(ev.Pickup);
        
        private void HighlightItemDouble(Pickup pickup)
        {
            if (Check(pickup))
            {
                if (ColorUtility.TryParseHtmlString(HighlightColor, out var color))
                {
                    var anchor = HighlightManager.MakeLight(pickup.Position, color,
                        LightShadows.None, HighlightRange, HighlightIntensity - 1.5f);

                    Light anchor2 = null;
                    
                    if (ColorUtility.TryParseHtmlString(HighlightSecondColor, out var lightSecondColor))
                    {
                        anchor2 = HighlightManager.MakeLight(pickup.Position, lightSecondColor,
                            LightShadows.None, HighlightRange, HighlightIntensity);
                    }
                    
                    if (EnableParticles)
                    {
                        HighlightManager.ProceduralParticles(anchor.GameObject, color, 0, 0.05f,
                            SpawnRange, ParticleSize, Intensity);
                        
                        if (ColorUtility.TryParseHtmlString(HighlightSecondColor, out var secondColor))
                            HighlightManager.ProceduralParticles(anchor.GameObject, secondColor, 0, 0.05f,
                                SpawnRange, ParticleSize, Intensity);
                    }
                    
                    anchor.Transform.SetParent(pickup.Transform);
                    anchor.Spawn();
                    
                    anchor2?.Transform.SetParent(pickup.Transform);
                    anchor2?.Spawn();
                }
                else
                {
                    var anchor = HighlightManager.MakeLight(pickup.Position, Color.white,
                        LightShadows.None, HighlightRange, HighlightIntensity);
                    
                    if (EnableParticles)
                    {
                        HighlightManager.ProceduralParticles(anchor.GameObject, Color.white, 0, 0.05f,
                            SpawnRange, ParticleSize, Intensity);
                        
                        if (ColorUtility.TryParseHtmlString(HighlightSecondColor, out _))
                            HighlightManager.ProceduralParticles(anchor.GameObject, Color.white, 0, 0.05f,
                                SpawnRange, ParticleSize, Intensity);
                    }
                    
                    anchor.Transform.SetParent(pickup.Transform);
                    anchor.Spawn();
                        
                    Log.Warn("Установлен некорректный цвет подсветки, выбор значения по умолчанию..."); 
                }
            }
        }
        
        private new void OnShot(ShotEventArgs ev)
        {
            if (!Check(ev.Firearm))
                return;

            var doorVariant = GetDoorVariant(ev.RaycastHit);
            if (doorVariant == null)
                return;
            
            Door door = Door.Get(doorVariant);
            if (LabApi.Features.Wrappers.Door.Get(doorVariant).IsDestroyed || door.IsGate || door.IsElevator ||
                 door.Type == DoorType.HczLoadingBay ||
                 door.Type == DoorType.HeavyBulkDoor)
            {
                return;
            }

            (doorVariant as IDamageableDoor)?.ServerDamage(ushort.MaxValue, DoorDamageType.Weapon);
        }
        
        private void OnChangedItem(ChangedItemEventArgs ev)
        {
            if (Check(ev.Item))
                CoroutineRunner.Run(HintsHandler(ev.Player));
        }
        
        public static DoorVariant GetDoorVariant(RaycastHit hit)
        {
            var collider = hit.collider;

            return
                collider.GetComponent<DoorVariant>() ??
                collider.GetComponentInParent<DoorVariant>() ??
                collider.GetComponentInChildren<DoorVariant>();
        }

        public IEnumerator HintsHandler(Player player)
        {
            while (!Round.IsEnded && Round.IsStarted && player.IsAlive && Check(player.CurrentItem))
            {
                RueDisplay.Get(player).Show(
                    new Tag(),
                    new BasicElement(110, "<size=25><b><color=#F79100>Вы используете Дверной Таран!</color></b></size>"), 1.1f);

                foreach (var spec in player.CurrentSpectatingPlayers)
                {
                    RueDisplay.Get(spec).Show(
                        new Tag(),
                        new BasicElement(110, "<size=25><b><color=#F79100>Игрок использует Дверной Таран</color></b></size>"), 1.1f);
                    
                    Timing.CallDelayed(1.2f, () => RueDisplay.Get(spec).Update());
                }
                Timing.CallDelayed(1.2f, () => RueDisplay.Get(player).Update());

                yield return new WaitForSeconds(1f);
            }
        }
    }
}