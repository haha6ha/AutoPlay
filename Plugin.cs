using System.Reflection;
using HarmonyLib;
using IPA;
using IPALogger = IPA.Logging.Logger;

namespace AutoReplay
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static Plugin Instance { get; private set; }
        public static IPALogger Log { get; private set; }
        public const string HarmonyId = "com.autoreplay.beatsaber";
        private Harmony _harmony;

        [Init]
        public void Init(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            Log.Info("AutoReplay initialized.");
        }

        [OnStart]
        public void OnApplicationStart()
        {
            try
            {
                _harmony = new Harmony(HarmonyId);
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log.Info("AutoReplay Harmony patches applied successfully.");
            }
            catch (System.Exception ex)
            {
                Log.Error($"AutoReplay Harmony patching failed: {ex}");
            }
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            try
            {
                _harmony?.UnpatchSelf();
            }
            catch (System.Exception ex)
            {
                Log.Error($"AutoReplay Harmony unpatching failed: {ex}");
            }
        }
    }
}
