using BatteringRam.Variants;
using Exiled.API.Interfaces;

namespace BatteringRam
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public BatteringRamCom18 BatteringRamCom18 { get; set; } = new();
    }
}