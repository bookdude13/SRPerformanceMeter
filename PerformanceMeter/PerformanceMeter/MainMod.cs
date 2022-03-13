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
        public static Util util;

        private bool inStage = false;
        private bool failed = false;
        private List<float> lifePctFrames;

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

            util = new Util(LoggerInstance);
            lifePctFrames = new List<float>();
        }

        private void reset()
        {
            failed = false;
            inStage = false;
            lifePctFrames.Clear();
        }

        private void injectAverageLifePctText(Game_ScoreSceneController scoreSceneController, float avgLifePct)
        {
            TMPro.TMP_Text totalScoreTextObject = scoreSceneController.totalScore;

            // TODO clean up finding
            /*
             * [13:19:51.469] [Performance_Meter] Reference game object: Value (TMPro.TextMeshPro)
[13:19:51.470] [Performance_Meter] Parent exists: TotalScore (UnityEngine.Transform)
[13:19:51.471] [Performance_Meter]     Name: TotalScore
[13:19:51.471] [Performance_Meter]     GO: TotalScore (UnityEngine.GameObject)
[13:19:51.472] [Performance_Meter] Parent exists: ScoreWrap (UnityEngine.Transform)
[13:19:51.473] [Performance_Meter]     Name: ScoreWrap
[13:19:51.474] [Performance_Meter]     GO: ScoreWrap (UnityEngine.GameObject)
[13:19:51.475] [Performance_Meter] Parent exists: No Multiplayer (UnityEngine.Transform)
[13:19:51.477] [Performance_Meter]     Name: No Multiplayer
[13:19:51.478] [Performance_Meter]     GO: No Multiplayer (UnityEngine.GameObject)
[13:19:51.479] [Performance_Meter] Parent exists: DisplayWrap (UnityEngine.Transform)
[13:19:51.480] [Performance_Meter]     Name: DisplayWrap
[13:19:51.481] [Performance_Meter]     GO: DisplayWrap (UnityEngine.GameObject)
[13:19:51.482] [Performance_Meter] Parent exists: [Game_Scripts] (UnityEngine.Transform)
[13:19:51.482] [Performance_Meter]     Name: [Game_Scripts]
[13:19:51.483] [Performance_Meter]     GO: [Game_Scripts] (UnityEngine.GameObject)
[13:19:51.484] [Performance_Meter] Top parent: [Game_Scripts] . [Game_Scripts] (UnityEngine.GameObject)
             */

            // Works but overlaps
            /*
            Transform totalScoreTransform = totalScoreTextObject.transform.parent;
            Transform scoreWrapTransform = totalScoreTransform.parent;
            LoggerInstance.Msg("scorewrap children: " + scoreWrapTransform.childCount);

            GameObject duplicateText = GameObject.Instantiate(totalScoreTransform.gameObject, scoreWrapTransform);
            duplicateText.name = "pmAvgLifePct";
            duplicateText.GetComponentInChildren<TMPro.TMP_Text>().text = string.Format("Average life percentage: {0:0.##}", avgLifePct);
            */

            Transform totalScoreTransform = totalScoreTextObject.transform.parent;
            LoggerInstance.Msg("total score children: " + totalScoreTransform.childCount);

            GameObject duplicateText = GameObject.Instantiate(totalScoreTransform.gameObject, totalScoreTransform);

            string labelChildName = "Label";
            string valueChildName = "Value";

            int numChildren = duplicateText.transform.childCount;
            for (int i = numChildren - 1; i >= 0; i--)
            {
                Transform child = duplicateText.transform.GetChild(i);
                if (child.name == labelChildName)
                {
                    util.SetTMProText(child, "Avg life pct: ");
                }
                else if (child.name == valueChildName)
                {
                    util.SetTMProText(child, string.Format("{0:0.##}", avgLifePct));
                }
                else
                {
                    LoggerInstance.Msg("Removing child " + child);
                    GameObject.Destroy(child.gameObject);
                }
            }

            duplicateText.name = "pmAvgLifePct";
            RectTransform[] rects = duplicateText.GetComponentsInChildren<RectTransform>();
            LoggerInstance.Msg("Rects: " + rects?.Length);
            LoggerInstance.Msg("Transform: " + duplicateText.transform.localPosition);

            util.LogGameObjectHierarchy(util.GetRootTransform(totalScoreTransform));

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
                    float avgLifePct = lifePctFrames.Average();
                    LoggerInstance.Msg("Average life pct: " + avgLifePct);

                    Game_ScoreSceneController scoreSceneController = Game_ScoreSceneController.s_instance;
                    if (scoreSceneController != null)
                    {
                        injectAverageLifePctText(scoreSceneController, avgLifePct);
                    }
                }
            }

            reset();

            inStage = sceneName == SCENE_NAME_STAGE;

            LoggerInstance.Msg("Scene: " + sceneName + ", stage? " + inStage);
        }

        private bool shouldCheckLifePct()
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

        public override void OnUpdate()
        {
            if (inStage && !failed)
            {
                if (shouldCheckLifePct())
                {
                    float lifePct = Synth.Utils.LifeBarHelper.GetScalePercent();

                    lifePctFrames.Add(lifePct);
                    failed = lifePct <= 0;
                }
            }
        }
    }
}
