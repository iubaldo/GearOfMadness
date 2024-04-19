using System;

using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace gearofmadness.src
{
    public class GearOfMadnessMain : ModSystem
    {
        const string CONFIG_FILE_NAME = "gearofmadnessconfig.json";
        internal static GMConfig config;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterItemClass("itemgearofmadness", typeof(ItemGearOfMadness));
            api.RegisterEntityBehaviorClass("gearsoundaffected", typeof(BehaviorGearSound));
        }

        public override void StartServerSide(ICoreServerAPI sapi)
        {
            base.StartServerSide(sapi);

            try
            {
                config = sapi.LoadModConfig<GMConfig>(CONFIG_FILE_NAME);
            } catch (Exception) { }

            if (config == null)
                config = new();

            sapi.StoreModConfig(config, CONFIG_FILE_NAME);

            
        }
    }
}
