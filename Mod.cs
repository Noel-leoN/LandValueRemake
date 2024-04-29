using System;
using System.Linq;
using System.Reflection;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Buildings;
using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Settings;
using Game.Simulation;
using HarmonyLib;
using Unity.Entities;


namespace LandValueRemake
{
    public sealed class Mod : IMod
    {
        public const string ModName = "LandValueRemake";

        public static Mod Instance { get; private set; }

        public static ExecutableAsset ModAsset { get; private set; }

        public static readonly string harmonyID = ModName;

        // log init;
        public static ILog log = LogManager.GetLogger($"{ModName}").SetShowsErrorsInUI(false);

        public static void Log(string text) => log.Info(text);

        public static void LogIf(string text)
        {
            if (m_Setting.Logging) log.Info(text);
        }

        public static Setting m_Setting { get; private set; }

        public void OnLoad(UpdateSystem updateSystem)
        {
            Instance = this;

            // Log;
            log.Info(nameof(OnLoad));
#if DEBUG
            log.Info("setting logging level to Debug");
            log.effectivenessLevel = Level.Debug;
#endif
            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                log.Info($"{asset.name} mod asset at {asset.path}");
                ModAsset = asset;
            }

            // harmony patches.
            var harmony = new Harmony(harmonyID);
            harmony.PatchAll(typeof(Mod).Assembly);
            var patchedMethods = harmony.GetPatchedMethods().ToArray();
            log.Info($"Plugin {harmonyID} made patches! Patched methods: " + patchedMethods.Length);
            foreach (var patchedMethod in patchedMethods)
            {
                log.Info($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.DeclaringType.Name}.{patchedMethod.Name}");
            }

            // UI settings; 
            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();
            // UI locale;
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));
            GameManager.instance.localizationManager.AddSource("zh-HANS", new LocaleCN(m_Setting));

            m_Setting._Hidden = false;

            // Load UI saved setting;
            AssetDatabase.global.LoadSettings(nameof(ModName), m_Setting, new Setting(this));

            //Disable vanilla systmes & enable custom systems；
            if (m_Setting.EnableRealisticLVSimulation == true)
            {
                World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Simulation.LandValueSystem>().Enabled = false;
                updateSystem.UpdateAt<LandValueRemake.Systems.LandValueSystemRe>(SystemUpdatePhase.GameSimulation);
            }
            
            if (m_Setting.EraseLowResHighRentWarning == true)
            {
                World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Simulation.RentAdjustSystem>().Enabled = false;
                updateSystem.UpdateAt<LandValueRemake.Systems.RentAdjustSystemRe>(SystemUpdatePhase.GameSimulation);
            }
            if(m_Setting.EnableLowResBuildEverywhere == true)
            {
                World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Simulation.ZoneSpawnSystem>().Enabled = false;
                updateSystem.UpdateAt<LandValueRemake.Systems.ZoneSpawnSystemRe>(SystemUpdatePhase.GameSimulation);
            }

            
        }

       
        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            Instance = null;

            // un-Harmony;
            var harmony = new Harmony(harmonyID);
            harmony.UnpatchAll(harmonyID);

            // un-Setting;
            if (m_Setting != null)
            {
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null;
            }
        }
    }
}
