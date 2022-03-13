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
        private static MelonLogger.Instance logger;

        public static MelonPreferences_Category prefs;

        private static bool inStage = false;
        private static bool failed = false;
        private static Dictionary<float, float> lifePctFrames;
        private static int numNotesAdded = 0;
        private static int numNotesRemoved = 0;

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

            logger = LoggerInstance;
            lifePctFrames = new Dictionary<float, float>();
        }

        private void Reset()
        {
            failed = false;
            inStage = false;
            numNotesAdded = 0;
            numNotesRemoved = 0;
            lifePctFrames.Clear();
        }

        private void InjectAverageLifePctText(Game_ScoreSceneController scoreSceneController, float avgLifePct)
        {
            TMPro.TMP_Text totalScoreTextObject = scoreSceneController.totalScore;

            Transform totalScoreTransform = totalScoreTextObject.transform.parent;
            LoggerInstance.Msg("total score children: " + totalScoreTransform.childCount);

            GameObject duplicateText = GameObject.Instantiate(
                totalScoreTransform.gameObject,
                totalScoreTransform
            ); ;

            string labelChildName = "Label";
            string valueChildName = "Value";

            int numChildren = duplicateText.transform.childCount;
            for (int i = numChildren - 1; i >= 0; i--)
            {
                Transform child = duplicateText.transform.GetChild(i);
                if (child.name == labelChildName)
                {
                    Util.SetTMProText(child, "Avg life pct: ");
                }
                else if (child.name == valueChildName)
                {
                    Util.SetTMProText(child, string.Format("{0:0.##}%", avgLifePct * 100));
                }
                else
                {
                    LoggerInstance.Msg("Removing child " + child);
                    GameObject.Destroy(child.gameObject);
                }
            }

            duplicateText.name = "pmAvgLifePct";

            //Util.LogGameObjectHierarchy(Util.GetRootTransform(totalScoreTransform));

            /*
            Transform topParent = totalScoreTextObject.transform;
            while (topParent.parent != null)
            {
                LoggerInstance.Msg("Parent exists: " + topParent.parent);
                LoggerInstance.Msg("    Name: " + topParent.parent.name);
                LoggerInstance.Msg("    GO: " + topParent.parent.gameObject);
                topParent = topParent.parent;
            }
            LoggerInstance.Msg("Top parent: " + topParent.name + " . " + topParent.gameObject);
            */

            // avgLifePctText.SetText(string.Format("{0:0.##}", avgLifePct));
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            if (sceneName == SCENE_NAME_GAME_END)
            {
                if (lifePctFrames.Count > 0)
                {
                    LoggerInstance.Msg(lifePctFrames.Count + " frames recorded.");
                    
                    // TODO implement real average function using frames as time reference points
                    float avgLifePct = lifePctFrames.Values.Average();
                    LoggerInstance.Msg("Average life pct: " + avgLifePct);

                    Game_ScoreSceneController scoreSceneController = Game_ScoreSceneController.s_instance;
                    if (scoreSceneController != null)
                    {
                        InjectAverageLifePctText(scoreSceneController, avgLifePct);
                    }
                }
            }

            Log("Diff in notes ended and started: " + (numNotesRemoved - numNotesAdded));
            Reset();

            inStage = sceneName == SCENE_NAME_STAGE;

            LoggerInstance.Msg("Scene: " + sceneName + ", stage? " + inStage);
        }

        private bool ShouldCheckLifePct()
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

        /*public override void OnUpdate()
        {
            if (inStage && !failed)
            {
                if (ShouldCheckLifePct())
                {
                    float lifePct = Synth.Utils.LifeBarHelper.GetScalePercent();

                    lifePctFrames.Add(lifePct);
                    failed = lifePct <= 0;
                }
            }
        }*/

        public static void OnAddNoteToActiveList(Game_Note note)
        {
            numNotesAdded++;
            Log(string.Format("{0} added", note.name));
        }

        public static void OnRemoveNoteFromActiveList(Game_Note note, float delayMS = 0f)
        {
            numNotesRemoved++;
            Log(string.Format("{0} removed", note.name));
            float time = note.NoteTimeOnMS - delayMS;

            float lifePct = Synth.Utils.LifeBarHelper.GetScalePercent();
            lifePctFrames.Add(time, lifePct);
            failed = lifePct <= 0;
        }

        public static void OnConsumeLinePoint(Game_Note note)
        {
            Log(string.Format("{0} removed (line)", note.name));
        }

        public static void Log(string message)
        {
            logger.Msg(message);
        }
    }
}
