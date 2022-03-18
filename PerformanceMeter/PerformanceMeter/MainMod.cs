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
        private static bool inStage = false;
        private static bool failed = false;
        private static Dictionary<float, float> lifePctFrames;
        private static int numNotesAdded = 0;
        private static int numNotesRemoved = 0;
        private static EndGameDisplay endGameDisplay;

        // Game_ScoreManager.UpdateLifesbar(float shrinkPercent)
        // called from GameControlManager.UpdateLifesBar()

        // Count these?
        // Collect, then present 30s window with the largest concentration?
        // Game_ScoreManager.BreakCombo(bool wrongHand, bool wasNotHandsClose)

        // Show in Game_ScoreSceneController

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
            failed = false;
            inStage = false;
            numNotesAdded = 0;
            numNotesRemoved = 0;
            lifePctFrames.Clear();
            lifePctFrames.Add(0, 1.0f);
        }

        

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            LoggerInstance.Msg("Scene loaded: " + sceneName);

            if (sceneName == SCENE_NAME_GAME_END)
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

                    endGameDisplay.Inject(LoggerInstance, Game_ScoreSceneController.s_instance, avgLifePct);
                }
            }

            Log("Diff in notes added and removed: " + (numNotesAdded - numNotesRemoved));
            Reset();

            inStage = sceneName == SCENE_NAME_STAGE;
        }

        /*private bool ShouldCheckLifePct()
        {
            if (GameControlManager.s_instance == null)
            {
                return false;
            }

            // TODO test
            if (GameControlManager.tutorialMode)
            {
                LoggerInstance.Msg("Tuturial mode");
                return false;
            }

            if (GameControlManager.IsPaused)
            {
                return false;
            }

            if (Synth.Utils.LifeBarHelper.s_instance == null)
            {
                return false;
            }

            return true;
        }
*/
        /*public override void OnUpdate()
        {
            if (inStage && !failed)
            {
                if (ShouldCheckLifePct())
                {
                    float lifePct = Synth.Utils.LifeBarHelper.GetScalePercent();
                    float timeSec = GameControlManager.CurrentTrackStatic.SongProgressOnSeconds;
                    lifePctFrames.Add(timeSec, lifePct);
                    failed = lifePct <= 0;
                }
            }
        }
*/

        public static void OnAddNoteToActiveList(Game_Note note)
        {
            Log(string.Format("{0} added", note.name));
        }

        public static void OnUpdateLifesBar(float songTimeMS, float lifePct)
        {
            lifePctFrames.Add(songTimeMS, lifePct);
            failed = lifePct <= 0;
        }

        public static void OnConsumeLinePoint(Game_Note note)
        {
            Log(string.Format("{0} removed (line)", note.name));
        }

        public static void Add()
        {
            numNotesAdded++;
        }

        public static void Remove()
        {
            numNotesRemoved++;
        }

        public static void Log(string message)
        {
            _logger.Msg(message);
        }
    }
}
