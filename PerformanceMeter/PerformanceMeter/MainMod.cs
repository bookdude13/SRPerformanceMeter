using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace PerformanceMeter
{
    public class MainMod : MelonMod
    {
        private static string SCENE_NAME_STAGE = "03.Stage";
        private static string SCENE_NAME_GAME_END = "3.GameEnd";

        public static MelonPreferences_Category prefs;

        private static MelonLogger.Instance _logger;
        private static Dictionary<float, float> lifePctFrames;
        private static EndGameDisplay endGameDisplay;

        public override void OnApplicationStart()
        {
            prefs = MelonPreferences.CreateCategory("MainPreferences", "Preferences");
            prefs.SetFilePath("UserData/PerformanceMeter.cfg");

            _logger = LoggerInstance;
            lifePctFrames = new Dictionary<float, float>();
            endGameDisplay = new EndGameDisplay();
        }

        private void Reset()
        {
            lifePctFrames.Clear();
            lifePctFrames.Add(0, 1.0f);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            LoggerInstance.Msg("Scene loaded: " + sceneName);

            if (sceneName == SCENE_NAME_STAGE)
            {

            }
            else if (sceneName == SCENE_NAME_GAME_END)
            {
                if (lifePctFrames.Count <= 0)
                {
                    LoggerInstance.Msg("lifePctFrames empty, ignoring");
                }
                else
                {
                    LoggerInstance.Msg(lifePctFrames.Count + " frames recorded.");
                    float avgLifePct = Utils.CalculateAverageLifePercent(lifePctFrames);
                    LoggerInstance.Msg("Average life pct: " + avgLifePct);

                    endGameDisplay.Inject(LoggerInstance, avgLifePct);
                }
            }

            Reset();
        }

       /* public override void OnUpdate()
        {
            base.OnUpdate();
            accumTimeSec += Time.deltaTime;
            if (timeSec >= pollingDelaySec)
            {
                lifePctFrames.Add(lastFrameSec + accumTimeSec, Synth.Utils.LifeBarHelper.GetScalePercent());
                lastFrameSec += accumTimeSec;
            }
        }*/

        public static void OnUpdateLifesBar(float songTimeMS, float lifePct)
        {
            lifePctFrames.Add(songTimeMS, lifePct);
        }

        public static void Log(string message)
        {
            _logger.Msg(message);
        }
    }
}
