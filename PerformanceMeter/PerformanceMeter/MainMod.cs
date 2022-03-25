using System;
using System.Collections;
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
        private static string SCENE_NAME_GAME_END = "3.GameEnd";
        private static int LIFE_CHECK_FREQUENCY_MS = 100;

        public static MelonPreferences_Category prefs;

        private static MelonLogger.Instance _logger;
        private static Dictionary<int, float> lifePctFrames;
        private static EndGameDisplay endGameDisplay;
        private static bool checkingLife = false;
        private static int timeSinceLastCheckMs = 0;
        private static int accumulatedTimeMs = 0;

        public override void OnApplicationStart()
        {
            prefs = MelonPreferences.CreateCategory("MainPreferences", "Preferences");
            prefs.SetFilePath("UserData/PerformanceMeter.cfg");

            _logger = LoggerInstance;
            lifePctFrames = new Dictionary<int, float>();
            endGameDisplay = new EndGameDisplay();
        }

        private void Reset()
        {
            checkingLife = false;
            timeSinceLastCheckMs = 0;
            accumulatedTimeMs = 0;
            lifePctFrames.Clear();
            lifePctFrames.Add(0, 1.0f);
        }

        private bool IsSceneStage(string sceneName)
        {
            if (sceneName == null || !sceneName.Contains("."))
            {
                return false;
            }

            string subName = sceneName.Split('.')[1];
            bool isNormalStage = subName.StartsWith("Stage");
            bool isSpinStage = subName.StartsWith("Static Stage");
            bool isSpiralStage = subName.StartsWith("Spiral Stage");

            return isNormalStage || isSpinStage || isSpiralStage;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            LoggerInstance.Msg("Scene loaded: " + sceneName);

            if (IsSceneStage(sceneName))
            {
                LoggerInstance.Msg("Starting to track life");
                Reset();
                checkingLife = true;
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
                    endGameDisplay.Inject(LoggerInstance, lifePctFrames);
                }
            }
        }

        public override void OnUpdate()
        {
            if (!checkingLife) return;
            if (GameControlManager.IsOnGameOver)
            {
                checkingLife = false;
                return;
            }
            if (!GameControlManager.ImPlaying()) return;

            int deltaMs = (int)(Time.deltaTime * 1000);
            accumulatedTimeMs += deltaMs;
            timeSinceLastCheckMs += deltaMs;
            if (timeSinceLastCheckMs > LIFE_CHECK_FREQUENCY_MS)
            {
                float lifePct = Synth.Utils.LifeBarHelper.GetScalePercent(0);
                lifePctFrames.Add(accumulatedTimeMs, lifePct);
                timeSinceLastCheckMs = 0;
            }
        }
        public static void Log(string message)
        {
            _logger.Msg(message);
        }
    }
}
