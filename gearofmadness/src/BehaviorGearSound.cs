using System;

using ProtoBuf;

using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Client;
using Vintagestory.API.Server;

namespace gearofmadness.src
{
    public class BehaviorGearSound : EntityBehavior
    {       
        bool enabled = true; // if temporal stability isn't enabled, this behavior does nothing
        int tickInterval = 20000;


        public override string PropertyName() { return "gearsoundaffected"; }


        public BehaviorGearSound(Entity entity) : base(entity) { }


        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);

            // check if the world is using temporal stability
            enabled = entity.Api.World.Config.GetBool("temporalStability", true);

            if (!enabled)
                return;

            if (!entity.WatchedAttributes.HasAttribute("temporalStability"))
                OwnStability = 1;

            if (entity.Api.Side == EnumAppSide.Server)
                GearOfMadnessMain.sapi.Event.RegisterGameTickListener(new Action<float>(OnGameTick), tickInterval);
        }


        public double OwnStability
        {
            get { return entity.WatchedAttributes.GetDouble("temporalStability"); }
            set { entity.WatchedAttributes.SetDouble("temporalStability", value); }
        }


        public override void OnGameTick(float deltaTime)
        {
            if (!enabled)
                return;

            if (entity.Api.Side == EnumAppSide.Client)
                return;

            GearSoundMessage message = 
                new() { 
                    shouldPlay = entity.WatchedAttributes.HasAttribute("temporalStability") && OwnStability <= 0.75,
                    stability = (float)OwnStability 
                };

            GearOfMadnessMain.sSoundChannel.SendPacket(message, (IServerPlayer)((EntityPlayer)entity).Player);
        }
    }
}
