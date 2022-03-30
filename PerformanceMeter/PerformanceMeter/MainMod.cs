using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using PerformanceMeter.Events;
using PerformanceMeter.Messages;
using UnityEngine;
using Newtonsoft.Json;

namespace PerformanceMeter
{
    public class MainMod : MelonMod, ISynthRidersEventHandler
    {
        private static string SCENE_NAME_GAME_END = "3.GameEnd";
        private static int MARKER_PERIOD_MIN_MS = 1000;
        private static int MARKER_PERIOD_MAX_MS = 5 * 60 * 1000;
        private static int LIFE_CHECK_MIN_PERIOD_MS = 50;
        private static int LIFE_CHECK_MAX_PERIOD_MS = 5 * 1000;

        public static MelonPreferences_Category prefs;
        private static bool showAverageLine = true;
        private static int markerPeriodMs = 30000;
        private static int lifeCheckPeriodMs = 100;

        private static MelonLogger.Instance _logger;
        private static Dictionary<int, float> lifePctFrames;
        private static Dictionary<int, float> lifePctFramesWebSocket;
        private static EndGameDisplay endGameDisplay;
        private static WebsocketManager websocketManager;
        private static bool checkingLife = false;
        private static int timeSinceLastCheckMs = 0;
        private static int accumulatedTimeMs = 0;

        public override void OnApplicationStart()
        {
            _logger = LoggerInstance;

            SetupConfig();

            lifePctFrames = new Dictionary<int, float>();
            lifePctFramesWebSocket = new Dictionary<int, float>();
            endGameDisplay = new EndGameDisplay(showAverageLine, markerPeriodMs);

            websocketManager = new WebsocketManager(_logger, "ws://localhost:9000", this);
        }

        private void SetupConfig()
        {
            prefs = MelonPreferences.CreateCategory("MainPreferences", "Preferences");

            try
            {
                // Remove old empty config file from 1.0.0/1.1.0
                File.Delete("UserData/PerformanceMeter.cfg");
            }
            catch (Exception e)
            {
                _logger.Msg("Failed to remove old config file: " + e.Message);
            }

            try
            {
                Directory.CreateDirectory("UserData/PerformanceMeter");

                prefs.SetFilePath("UserData/PerformanceMeter/PerformanceMeter.cfg");

                var showAverageLineEntry = prefs.CreateEntry("showAverageLine", true, "Show Average Line");
                showAverageLine = showAverageLineEntry.Value;

                var markerPeriodMsEntry = prefs.CreateEntry("markerPeriodMs", 30000, "Marker Period (ms)");
                markerPeriodMs = markerPeriodMsEntry.Value;
                if (markerPeriodMs < MARKER_PERIOD_MIN_MS)
                {
                    _logger.Msg("markerPeroidMs is less than minimum; did you put in seconds instead of milliseconds? Using min of " + MARKER_PERIOD_MIN_MS);
                    markerPeriodMs = MARKER_PERIOD_MIN_MS;
                }
                markerPeriodMs = Math.Min(MARKER_PERIOD_MAX_MS, markerPeriodMs);

                var lifeCheckPeriodMsEntry = prefs.CreateEntry("lifeCheckPeriodMs", 100, "Life Check Period (ms)");
                lifeCheckPeriodMs = Math.Max(LIFE_CHECK_MIN_PERIOD_MS, lifeCheckPeriodMsEntry.Value);
                lifeCheckPeriodMs = Math.Min(LIFE_CHECK_MAX_PERIOD_MS, lifeCheckPeriodMs);

                _logger.Msg("Config Loaded");
                _logger.Msg("  Show average line? " + showAverageLine);
                _logger.Msg("  markerPeriodMs: " + markerPeriodMs);
                _logger.Msg("  lifeCheckPeriodMs: " + lifeCheckPeriodMs);
            }
            catch (Exception e)
            {
                _logger.Msg("Failed to setup config values. Msg: " + e.Message);
            }
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

            if (sceneName == "0.AWarning")
            {
                websocketManager.Start();
            }
            else if (IsSceneStage(sceneName))
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
            if (timeSinceLastCheckMs > lifeCheckPeriodMs)
            {
                float lifePct = Synth.Utils.LifeBarHelper.GetScalePercent(0);
                lifePctFrames.Add(accumulatedTimeMs, lifePct);
                timeSinceLastCheckMs = 0;
            }
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            websocketManager.Shutdown();
        }

        public static void Log(string message)
        {
            _logger.Msg(message);
        }

        public void OnSongStart(EventDataSongStart data)
        {
            _logger.Msg("Song started! " + JsonConvert.SerializeObject(data));
        }
    }
}
