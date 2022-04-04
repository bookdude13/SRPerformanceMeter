using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using PerformanceMeter.Frames;
using PerformanceMeter.Models;
using PerformanceMeter.Repositories;
using PerformanceMeter.Services;
using Synth.Data;
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
        private static BestRunService bestRunService;
        private static PlayConfigurationService playConfigurationService;
        private static EndGameDisplay endGameDisplay;
        private static SynthRidersEventsManager websocketManager;

        private static List<PercentFrame> lifePctFrames;
        private static List<CumulativeFrame> totalScoreFrames;
        private static List<CumulativeFrame> totalPerfectFrames;
        private static PlayConfiguration currentPlayConfig;        
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
                var dbPath = Path.Combine(modDirectory, "PerformanceMeter.db");
                var db = new LiteDB.LiteDatabase(string.Format("Filename={0}", dbPath));

                _logger.Msg("Setting up repos....");
                var playConfigurationRepo = new PlayConfigurationRepository(wrappedLogger, db);
                playConfigurationService = new PlayConfigurationService(wrappedLogger, playConfigurationRepo);

                var bestRunRepo = new BestRunRepository(wrappedLogger, db);
                bestRunService = new BestRunService(wrappedLogger, bestRunRepo);
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
                _logger.Msg(lifePctFrames.Count + " life pct frames recorded.");
                _logger.Msg(totalScoreFrames.Count + " score frames recorded.");
                _logger.Msg(totalPerfectFrames.Count + " accuracy frames recorded.");

                var averageLifePercent = Utils.CalculateAveragePercent(lifePctFrames);

                // Database updates
                LifePercentRun bestLifePercentRun = null;
                TotalScoreRun bestTotalScoreRun = null;
                try
                {
                    bestRunService.UpdateBestLifePercent(currentPlayConfig, averageLifePercent, lifePctFrames);
                    bestLifePercentRun = bestRunService.GetBestLifePercent(currentPlayConfig);

                    bestRunService.UpdateBestTotalScore(currentPlayConfig, totalScoreFrames);
                    bestTotalScoreRun = bestRunService.GetBestTotalScore(currentPlayConfig);
                }
                catch (Exception e)
                {
                    _logger.Msg("Error while updating best runs: " + e.Message);
                    return;
                }

                if (lifePctFrames.Count > 0 && totalScoreFrames.Count > 0 && bestTotalScoreRun.TotalScoreFrames.Count > 0)
                {
                    endGameDisplay.Inject(LoggerInstance, lifePctFrames, bestTotalScoreRun.TotalScoreFrames, totalScoreFrames);
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

        private PlayConfiguration GetCurrentPlayConfiguration(GameControlManager gameControlManager, Game_InfoProvider infoProvider)
        {
            if (gameControlManager == null || infoProvider == null)
            {
                return null;
            }

            // Pieces taken from Game_InfoProvider.DoScoresUpload() and Util_LeaderboardManager.SubmitMatchScoresGlobal()

            string username = Util_PlatformManager.MyUserName;
            string difficulty = Game_InfoProvider.CurrentDifficultyS.ToString();
            
            LeaderboardInfo.PlayMode gameMode = LeaderboardInfo.PlayMode.Rhythm;
            if (Game_InfoProvider.ModifiersMode == SynthSettings.ModifiersMode.Boxing || Game_InfoProvider.ModifiersMode == SynthSettings.ModifiersMode.BoxingNormal)
            {
                gameMode = LeaderboardInfo.PlayMode.Force;
            }

            string mapHash = infoProvider.LeaderboardName;
            if (!string.IsNullOrEmpty(infoProvider.LeaderboardHash))
            {
                mapHash = infoProvider.LeaderboardHash;
            }

            var modifiers = new List<string>();
            List<LeaderboardModifier> leaderboardModifiers = infoProvider.GetCurrentModifiersListSelected(false);
            foreach (var modif in leaderboardModifiers)
            {
                modifiers.Add(modif.ToString());
            }

            return new PlayConfiguration()
            {
                Username = username,
                Difficulty = difficulty,
                MapHash = mapHash,
                GameMode = gameMode.ToString(),
                Modifiers = modifiers
            };
        }

        /* Handle websocket events */

        void ISynthRidersEventHandler.OnSongStart(EventDataSongStart data)
        {
            _logger.Msg("Song started!");
            Reset();
            inSong = true;
            currentPlayConfig = GetCurrentPlayConfiguration(GameControlManager.s_instance, Game_InfoProvider.s_instance);
        }

        void ISynthRidersEventHandler.OnSongEnd(EventDataSongEnd data)
        {
            _logger.Msg("Song ended!");
            inSong = false;

            // Once we don't risk introducing any lag into the run, ensure that the play config exists while wrapping up the map
            currentPlayConfig.Id = playConfigurationService.EnsurePlayConfiguration(currentPlayConfig) ?? Guid.NewGuid();
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
