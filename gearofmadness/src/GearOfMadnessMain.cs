using ProtoBuf;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace gearofmadness.src
{
    public class GearOfMadnessMain : ModSystem
    {
        internal static readonly bool debugMode = false;

        const string CONFIG_FILE_NAME = "gearofmadnessconfig.json";
        internal static GMConfig config;

        internal static ICoreServerAPI sapi;

        internal static IServerNetworkChannel sSoundChannel;

        ICoreClientAPI capi;
        ILoadedSound gearSound;

        public override void StartPre(ICoreAPI api)
        {
            base.StartPre(api);

            try
            {
                config = api.LoadModConfig<GMConfig>(CONFIG_FILE_NAME);
            }
            catch (Exception) { }

            if (config == null)
                config = new();

            api.StoreModConfig(config, CONFIG_FILE_NAME);
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterItemClass("itemgearofmadness", typeof(ItemGearOfMadness));
            api.RegisterEntityBehaviorClass("gearsoundaffected", typeof(BehaviorGearSound));


            api.Event.OnEntitySpawn += AddEntityBehaviors;
            api.Event.OnEntityLoaded += AddEntityBehaviors;
        }


        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            sapi = api;
            sSoundChannel =
                sapi.Network.RegisterChannel("gearSoundChannel")
                .RegisterMessageType<GearSoundMessage>();

            if (debugMode)
                RegisterCommands(sapi);
        }


        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(capi);

            capi = api;

            capi.Network.RegisterChannel("gearSoundChannel")
                .RegisterMessageType<GearSoundMessage>()
                .SetMessageHandler<GearSoundMessage>(OnPlaySound);
        }


        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsFinalize(api);

            api.World.Config.SetBool("GearOfMadness_ItemGearOfMadness_IsEnabled", config.enableGearItem);

            if (api.Side == EnumAppSide.Client)
                InitSounds();
        }


        public void AddEntityBehaviors(Entity targetEntity)
        {
            if (targetEntity == null || targetEntity is not EntityPlayer)
                return;

            if (config.enableGearSound)
                targetEntity.AddBehavior(new BehaviorGearSound(targetEntity));
        }


        void RegisterCommands(ICoreServerAPI sapi)
        {
            CommandArgumentParsers parsers = sapi.ChatCommands.Parsers;

            sapi.ChatCommands
                .GetOrCreate("gom")
                .IgnoreAdditionalArgs()
                .RequiresPrivilege("worldedit")
                .WithDescription("Gear of Madness Mod debug commands.")

                .BeginSubCommand("setStability")
                    .WithDescription("Sets the temporal stability of the calling player.")
                    .WithArgs(parsers.DoubleRange("newStability", 0, 1))
                    .HandleWith(OnCmdSetStability)
                .EndSubCommand()
            ;
        }


        TextCommandResult OnCmdSetStability(TextCommandCallingArgs args)
        {
            double oldStability = args.Caller.Player.Entity.WatchedAttributes.GetDouble("temporalStability");
            double newStability = Convert.ToDouble(args[0]);
            
            args.Caller.Player.Entity.WatchedAttributes.SetDouble("temporalStability", newStability);

            Mod.Logger.Notification("Stability set from " + oldStability + " -> " + newStability);

            return TextCommandResult.Success();
        }


        void OnPlaySound(GearSoundMessage message)
        {
            float fadeSpeed = 3f;

            if (gearSound == null)
                return;

            if (message.shouldPlay) {
                if (debugMode)
                    Mod.Logger.Notification("playing gear sound");

                if (!gearSound.IsPlaying)
                    gearSound.Start();

                float volume = (0.4f - message.stability) * 1 / 0.4f; // volume scaling with how low stability is
                gearSound.FadeTo(Math.Min(1, volume), 0.95f * fadeSpeed, (s) => { });
            }
            else
            {
                if (debugMode)
                    Mod.Logger.Notification("stopping gear sound");

                gearSound.FadeTo(0, 0.95f * fadeSpeed, (s) => { gearSound.Stop(); });
            }
        }


        void InitSounds()
        {
            if (debugMode)
                Mod.Logger.Notification("initializing gear sound");

            gearSound = capi.World.LoadSound(new SoundParams()
            {
                Location = new AssetLocation("gearofmadness", "sounds/bs2_gear_loop.ogg"),
                ShouldLoop = true,
                RelativePosition = true,
                DisposeOnFinish = false,
                SoundType = EnumSoundType.SoundGlitchunaffected,
                Volume = 0f
            });
        }
    }


    // tells the client to play a sound
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GearSoundMessage
    {
        public bool shouldPlay;
        public float stability;
    }
}
