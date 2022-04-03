using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using PerformanceMeter.Frames;
using PerformanceMeter.Repositories;
using PerformanceMeter.Services;
using SynthRidersWebsockets.Events;
using UnityEngine;

namespace PerformanceMeter
{
    public class MainMod : MelonMod, ISynthRidersEventHandler
    {
        private static readonly string SCENE_NAME_GAME_END = "3.GameEnd";
        private static readonly string modDirectory = "UserData/PerformanceMeter";

        private static MelonLogger.Instance _logger;
        private static ConfigManager config;
        private static LocalScoreService localScoreService;

        private static List<PercentFrame> lifePctFrames;
        private static List<CumulativeFrame> totalScoreFrames;
        private static List<CumulativeFrame> totalPerfectFrames;
        private static EndGameDisplay endGameDisplay;
        private static SynthRidersEventsManager websocketManager;
        
        private static bool inSong = false;

        public override void OnApplicationStart()
        {
            _logger = LoggerInstance;
            
            config = new ConfigManager(modDirectory);
            config.Initialize(_logger);

            if (!config.isEnabled)
            {
                _logger.Msg("Disabled in config");
                return;
            }

            lifePctFrames = new List<PercentFrame>();
            totalScoreFrames = new List<CumulativeFrame>();
            totalPerfectFrames = new List<CumulativeFrame>();

            endGameDisplay = new EndGameDisplay(config);

            var wrappedLogger = new MelonLoggerWrapper(_logger);

            try
            {
                _logger.Msg("Setting up database...");
                var scoresDbPath = Path.Combine(modDirectory, "scores.db");
                var scoresDb = new LiteDB.LiteDatabase(scoresDbPath);
                var localScoreRepo = new LocalScoreRepository(wrappedLogger, scoresDb);
                localScoreService = new LocalScoreService(wrappedLogger, localScoreRepo);
            }
            catch (Exception e)
            {
                _logger.Msg("Failed to set up database! Disabling to be safe. Message: " + e.Message);
                config.isEnabled = false;
            }

            _logger.Msg("Setting up websocket manager...");
            websocketManager = new SynthRidersEventsManager(_logger, "ws://localhost:9000", this);

            _logger.Msg("Initialized.");
        }

        
        private void Reset()
        {
            lifePctFrames.Clear();
            lifePctFrames.Add(new PercentFrame(0, 1.0f));

            totalScoreFrames.Clear();
            totalScoreFrames.Add(new CumulativeFrame(0, 0));

            totalPerfectFrames.Clear();
            totalPerfectFrames.Add(new CumulativeFrame(0, 0));
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            if (!config.isEnabled)
            {
                return;
            }

            if (sceneName == "0.AWarning")
            {
                // Start websocket client after the server is likely started
                _logger.Msg("Starting websocket client after startup...");
                websocketManager.StartAsync();
            }
            else if (sceneName == SCENE_NAME_GAME_END)
            {
                if (lifePctFrames.Count > 0)
                {
                    _logger.Msg(lifePctFrames.Count + " life pct frames recorded.");
                    _logger.Msg(totalScoreFrames.Count + " score frames recorded.");
                    _logger.Msg(totalPerfectFrames.Count + " accuracy frames recorded.");

                    float targetScore = totalScoreFrames.Last().Amount;
                    var scorePctFrames = totalScoreFrames.Select(scoreFrame => scoreFrame.ToPercentFrame(targetScore)).ToList();

                    endGameDisplay.Inject(LoggerInstance, lifePctFrames, scorePctFrames);
                }
            }
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();

            if (websocketManager != null && config.isEnabled)
            {
                try
                {
                    websocketManager.Shutdown();
                }
                catch (Exception e)
                {
                    _logger.Msg("Failed to shutdown websocket manager: " + e.ToString());
                }
            }
        }

        /* Handle websocket events */

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
                lifePctFrames.Add(new PercentFrame(data.playTimeMS, data.lifeBarPercent));
                totalScoreFrames.Add(new CumulativeFrame(data.playTimeMS, data.score));
            }
        }

        void ISynthRidersEventHandler.OnNoteMiss(EventDataNoteMiss data)
        {
            if (inSong)
            {
                lifePctFrames.Add(new PercentFrame(data.playTimeMS, data.lifeBarPercent));
            }
        }

        void ISynthRidersEventHandler.OnSceneChange(EventDataSceneChange data)
        {            
        }

        void ISynthRidersEventHandler.OnReturnToMenu()
        {
            _logger.Msg("Return to menu");
            inSong = false;
        }
    }
}
