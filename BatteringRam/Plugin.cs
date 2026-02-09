using System;
using Exiled.API.Features;
using Exiled.CustomItems.API;

namespace BatteringRam
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => nameof(BatteringRam);
        public override string Prefix => Name;
        public override string Author => "Morkamo";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 12, 1);

        public static Plugin Instance { get; private set; }
        
        public override void OnEnabled()
        {
            Instance = this;
            Config.BatteringRamCom18.Register();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;
            Config.BatteringRamCom18.Unregister();
            base.OnDisabled();
        }
    }
}