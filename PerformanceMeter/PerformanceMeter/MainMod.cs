using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using SynthRidersWebsockets.Events;
using UnityEngine;

namespace PerformanceMeter
{
    public class LifePercentFrame
    {
        public readonly float timeMs;
        public readonly float lifePercent;

        public LifePercentFrame(float timeMs, float lifePercent)
        {
            this.timeMs = timeMs;
            this.lifePercent = lifePercent;
        }
    }

    public class MainMod : MelonMod, ISynthRidersEventHandler
    {
        private static string SCENE_NAME_GAME_END = "3.GameEnd";
        private static int MARKER_PERIOD_MIN_MS = 1000;
        private static int MARKER_PERIOD_MAX_MS = 5 * 60 * 1000;

        public static MelonPreferences_Category prefs;
        private static bool showAverageLine = true;
        private static int markerPeriodMs = 30000;

        private static MelonLogger.Instance _logger;
        private static List<LifePercentFrame> lifePctFrames;
        private static EndGameDisplay endGameDisplay;
        private static SynthRidersEventsManager websocketManager;
        
        private static bool inSong = false;

        public override void OnApplicationStart()
        {
            _logger = LoggerInstance;

            SetupConfig();

            lifePctFrames = new List<LifePercentFrame>();
            endGameDisplay = new EndGameDisplay(showAverageLine, markerPeriodMs);

            websocketManager = new SynthRidersEventsManager(_logger, "ws://localhost:9000", this);
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

                _logger.Msg("Config Loaded");
                _logger.Msg("  Show average line? " + showAverageLine);
                _logger.Msg("  markerPeriodMs: " + markerPeriodMs);
            }
            catch (Exception e)
            {
                _logger.Msg("Failed to setup config values. Msg: " + e.Message);
            }
        }

        private void Reset()
        {
            lifePctFrames.Clear();
            lifePctFrames.Add(new LifePercentFrame(0, 1.0f));
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

            if (sceneName == "0.AWarning")
            {
                // Start websocket client after the server is likely started
                websocketManager.StartAsync();
            }
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            websocketManager.Shutdown();
        }

        /* Handle events */

        void ISynthRidersEventHandler.OnSongStart(EventDataSongStart data)
        {
            _logger.Msg("Song started!");
            Reset();
            inSong = true;
        }

        void ISynthRidersEventHandler.OnSongEnd(EventDataSongEnd data)
        {
            _logger.Msg("Song ended!");
            inSong = false;
        }

        void ISynthRidersEventHandler.OnPlayTime(EventDataPlayTime data)
        {
            _logger.Msg("Play time " + data.playTimeMS);
        }

        void ISynthRidersEventHandler.OnNoteHit(EventDataNoteHit data)
        {
            if (inSong)
            {
                lifePctFrames.Add(new LifePercentFrame(data.playTimeMS, data.lifeBarPercent));
            }
        }

        void ISynthRidersEventHandler.OnNoteMiss(EventDataNoteMiss data)
        {
            if (inSong)
            {
                lifePctFrames.Add(new LifePercentFrame(data.playTimeMS, data.lifeBarPercent));
            }
        }

        void ISynthRidersEventHandler.OnSceneChange(EventDataSceneChange data)
        {
            _logger.Msg("Scene change to " + data.sceneName);

            if (data.sceneName == SCENE_NAME_GAME_END)
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

        void ISynthRidersEventHandler.OnReturnToMenu()
        {
            _logger.Msg("Return to menu");
            inSong = false;
        }
    }
}
