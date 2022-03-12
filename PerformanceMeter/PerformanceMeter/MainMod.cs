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
        private static String STAGE_SCENE_NAME = "03.Stage";

        public static MelonPreferences_Category prefs;

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

            lifePctFrames = new List<float>();
        }

        private void reset()
        {
            LoggerInstance.Msg("Reset");
            failed = false;
            lifePctFrames.Clear();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            if (lifePctFrames.Count > 0)
            {
                LoggerInstance.Msg(lifePctFrames.Count + " frames recorded.");
            }

            reset();
            inStage = (sceneName == STAGE_SCENE_NAME);

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
            if (inStage && !failed && shouldCheckLifePct())
            {
                float lifePct = Synth.Utils.LifeBarHelper.GetScalePercent();
                
                lifePctFrames.Add(lifePct);
                failed = lifePct <= 0;
            }
        }
    }
}
