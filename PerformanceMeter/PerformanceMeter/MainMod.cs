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
using SRModCore;
using Il2Cpp;
using Il2CppSynth.Data;
using SynthRidersWebsockets.Events;
using UnityEngine;
using System.Threading;

namespace PerformanceMeter
{
    public class MainMod : MelonMod, ISynthRidersEventHandler
    {
        private const bool VERBOSE_LOGS = false;

        private static readonly string SCENE_NAME_GAME_END = "3.GameEnd";
        private static readonly string modDirectory = "UserData/PerformanceMeter";

        private static SRLogger _logger;
        private static ConfigManager config;
        private static BestRunService bestRunService;
        private static PlayConfigurationService playConfigurationService;
        private static EndGameDisplay endGameDisplay;
        private static SREventsWebSocketClient webSocketClient;
        private static CancellationToken webSocketCancellation = new();

        private static List<PercentFrame> lifePctFrames;
        private static List<CumulativeFrame> totalScoreFrames;
        private static List<CumulativeFrame> totalPerfectFrames;
        private static PlayConfiguration currentPlayConfig;
        private static bool inSong = false;

        private bool shouldInject = false;
        private static TotalScoreRun highScoreRun = null;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            _logger = new MelonLoggerWrapper(LoggerInstance);
            
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

            try
            {
                _logger.Msg("Setting up database...");
                var dbPath = Path.Combine(modDirectory, "PerformanceMeter.db");
                var db = new LiteDB.LiteDatabase(string.Format("Filename={0}", dbPath));

                _logger.Msg("Setting up repos....");
                var playConfigurationRepo = new PlayConfigurationRepository(_logger, db);
                playConfigurationService = new PlayConfigurationService(_logger, playConfigurationRepo);

                var bestRunRepo = new BestRunRepository(_logger, db);
                bestRunService = new BestRunService(_logger, bestRunRepo);
            }
            catch (Exception e)
            {
                _logger.Msg("Failed to set up database! Disabling to be safe. Message: " + e.Message);
                config.isEnabled = false;
            }

            _logger.Msg("Setting up websocket manager...");
            webSocketClient = new SREventsWebSocketClient(LoggerInstance, "localhost", 9000, this);

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
                _ = webSocketClient.StartAsync(webSocketCancellation);
            }
            else if (sceneName == SCENE_NAME_GAME_END)
            {
                _logger.Msg(lifePctFrames.Count + " life pct frames recorded.");
                _logger.Msg(totalScoreFrames.Count + " score frames recorded.");
                _logger.Msg(totalPerfectFrames.Count + " accuracy frames recorded.");

                // Database updates
                LifePercentRun bestLifePercentRun = null;
                
                try
                {
                    bestRunService.UpdateBestLifePercent(currentPlayConfig, lifePctFrames);
                    bestLifePercentRun = bestRunService.GetBestLifePercent(currentPlayConfig);

                    // Get high score run before updating, so we can compare if we beat it
                    highScoreRun = bestRunService.GetBestTotalScore(currentPlayConfig);
                    bestRunService.UpdateBestTotalScore(currentPlayConfig, totalScoreFrames);

                    // If we didn't have a high score run set yet (first score), then show both the same
                    if (highScoreRun == null)
                    {
                        highScoreRun = bestRunService.GetBestTotalScore(currentPlayConfig);
                    }
                }
                catch (Exception e)
                {
                    _logger.Msg("Error while updating best runs: " + e.Message);
                    return;
                }

                if (lifePctFrames.Count > 0 && totalScoreFrames.Count > 0 && highScoreRun != null)
                {
                    shouldInject = true;
                }
            }
        }

        public override void OnUpdate()
        {
            if (!shouldInject)
                return;

            // Wait for the center screen to load
            var center = GameObject.Find("[Score Summary]/DisplayWrap");
            if (center == null)
                return;

            // Once center screen is loaded show the graph and reset
            shouldInject = false;
            endGameDisplay.Inject(_logger, lifePctFrames, highScoreRun.TotalScoreFrames, totalScoreFrames);
            highScoreRun = null;
        }

        public override async void OnApplicationQuit()
        {
            base.OnApplicationQuit();

            if (webSocketClient != null && config.isEnabled)
            {
                try
                {
                    await webSocketClient.StopAsync(webSocketCancellation);
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
                _logger.Error("GameControlManager or Game_InfoProvider null! Cannot retrieve play configuration");
                return null;
            }

            // Pieces taken from Game_InfoProvider.DoScoresUpload() and Util_LeaderboardManager.SubmitMatchScoresGlobal()

            // For Remastered, all user values are null/0. Ignoring them in the db query/comparison for now.
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
            var leaderboardModifiers = infoProvider.GetCurrentModifiersListSelected(false);
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

        void LogVerbose(string message)
        {
            if (VERBOSE_LOGS)
            {
                _logger.Msg(message);
            }
        }

        /* Handle websocket events */

        void ISynthRidersEventHandler.OnSongStart(EventDataSongStart data)
        {
            LogVerbose("Song started!");
            Reset();
            inSong = true;
            currentPlayConfig = GetCurrentPlayConfiguration(GameControlManager.s_instance, Game_InfoProvider.s_instance);
        }

        void ISynthRidersEventHandler.OnSongEnd(EventDataSongEnd data)
        {
            LogVerbose("Song ended!");
            inSong = false;

            // Once we don't risk introducing any lag into the run, ensure that the play config exists while wrapping up the map
            currentPlayConfig.Id = playConfigurationService.EnsurePlayConfiguration(currentPlayConfig) ?? Guid.NewGuid();
        }

        void ISynthRidersEventHandler.OnPlayTime(EventDataPlayTime data)
        {
            LogVerbose("Play time " + data.playTimeMS);
        }

        void ISynthRidersEventHandler.OnNoteHit(EventDataNoteHit data)
        {
            LogVerbose($"Note miss. Health: {data.lifeBarPercent}");
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

        void ISynthRidersEventHandler.OnEnterSpecial() { }
        void ISynthRidersEventHandler.OnCompleteSpecial() { }
        void ISynthRidersEventHandler.OnFailSpecial() { }

        void ISynthRidersEventHandler.OnReturnToMenu()
        {
            _logger.Msg("Return to menu");
            inSong = false;
        }
    }
}
