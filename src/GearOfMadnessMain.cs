using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace gearofmadness.src
{
    public class GearOfMadnessMain : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterItemClass("itemgearofmadness", typeof(ItemGearOfMadness));
            api.RegisterEntityBehaviorClass("gearsoundaffected", typeof(BehaviorGearSound));
        }

        //public override void StartServerSide(ICoreServerAPI api)
        //{
        //    base.StartServerSide(api);
        //    api.RegisterCommand("setStability", "Set Temporal Stability", "", SetStability);
        //}

        //private void SetStability(IServerPlayer player, int groupId, CmdArgs args)
        //{
        //    double newStab = GameMath.Clamp(Convert.ToDouble(args.PeekWord()), 0, 1);
        //    player.Entity.WatchedAttributes.SetDouble("temporalStability", newStab);
        //}
    }
}
