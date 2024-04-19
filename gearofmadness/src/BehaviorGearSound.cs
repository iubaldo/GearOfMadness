using System;

using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Client;
using Vintagestory.API.Server;

namespace gearofmadness.src
{
    public class BehaviorGearSound : EntityBehavior
    {
        ICoreClientAPI capi;
        ILoadedSound gearSound;
        bool requireInitSounds;
        bool enabled = true; // if temporal stability isn't enabled, this behavior does nothing
        bool isSelf;
        float oneSecAccum = 0f;

        public override string PropertyName() { return "gearsoundaffected"; }

        public BehaviorGearSound(Entity entity) : base(entity) { }

        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);

            if (entity.Api.Side == EnumAppSide.Client)
            {
                capi = entity.Api as ICoreClientAPI;
                requireInitSounds = true;
            }

            // check if the world is using temporal stability
            enabled = entity.Api.World.Config.GetBool("temporalStability", true);

            if (!entity.WatchedAttributes.HasAttribute("temporalStability"))
                OwnStability = 1;
        }

        public double OwnStability
        {
            get { return entity.WatchedAttributes.GetDouble("temporalStability"); }
            set { entity.WatchedAttributes.SetDouble("temporalStability", value); }
        }

        public override void OnGameTick(float deltaTime)
        {
            // sanity checks, don't want to do stuff on tick if we don't have to
            if (!enabled)
                return;

            if (requireInitSounds)
            {
                capi = entity.Api as ICoreClientAPI;

                if (capi == null)
                    return;

                isSelf = capi.World.Player.Entity.EntityId == entity.EntityId;
                if (!isSelf)
                    return;

                gearSound = capi.World.LoadSound(new SoundParams()
                {
                    Location = new AssetLocation("gearofmadness", "sounds/bs2_gear_loop.ogg"),
                    ShouldLoop = true,
                    RelativePosition = true,
                    DisposeOnFinish = false,
                    SoundType = EnumSoundType.SoundGlitchunaffected,
                    Volume = 0f
                });

                requireInitSounds = false;
            }

            EntityPlayer entityPlayer = entity as EntityPlayer;
            IPlayer player = (entityPlayer != null) ? entityPlayer.Player : null;
            IServerPlayer serverPlayer = player as IServerPlayer;
            if (entity.World.Side == EnumAppSide.Client)
            {
                if (!(entity.World.Api as ICoreClientAPI).PlayerReadyFired)
                    return;
            }
            else
            {
                serverPlayer = entity.World.PlayerByUid(((EntityPlayer)entity).PlayerUID) as IServerPlayer;
                if (serverPlayer != null && serverPlayer.ConnectionState != EnumClientState.Playing)
                    return;
            }

            deltaTime = GameMath.Min(0.5f, deltaTime);
            oneSecAccum += deltaTime; // do every second
            if (oneSecAccum > 1)
            {
                oneSecAccum = 0;

                if (!isSelf)
                    return;

                float fadeSpeed = 3f;
                if (gearSound != null)
                {
                    if (entity.WatchedAttributes.HasAttribute("temporalStability"))
                    {
                        if (OwnStability <= 0.75) // start playing sound, volume scaling with how low stability is
                        {
                            if (!gearSound.IsPlaying)
                                gearSound.Start();

                            float volume = (0.4f - (float)OwnStability) * 1 / 0.4f;
                            gearSound.FadeTo(Math.Min(1, volume), 0.95f * fadeSpeed, (s) => { });
                        }
                    }
                    else
                        gearSound.FadeTo(0, 0.95f * fadeSpeed, (s) => { gearSound.Stop(); });
                }
            }
        }
    }
}
