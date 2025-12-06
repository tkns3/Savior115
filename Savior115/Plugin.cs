using IPA;
using HarmonyLib;
using IPALogger = IPA.Logging.Logger;

namespace Savior115
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }
        private Harmony _harmony;

        [Init]
        public Plugin(IPALogger logger)
        {
            Log = logger;
        }

        [OnStart]
        public void OnApplicationStart()
        {
            try
            {
                _harmony = new Harmony("net.tas3.Savior115");
                _harmony.PatchAll();
            }
            catch (System.Exception ex)
            {
                Log.Critical("Failed to apply Harmony patches.");
                Log.Critical(ex.ToString());
            }   
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
