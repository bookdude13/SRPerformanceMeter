using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace PerformanceMeter
{
    class EndGameDisplay
    {
        public void Inject(
            MelonLogger.Instance logger,
            float avgLifePct
        ) {
            GameObject leftScreen = InjectLeftScreen(logger);
            InjectAverageLifePctText(logger, leftScreen, avgLifePct);
        }

        /// <summary>
        /// Clones the center screen and moves it to the left to put additional statistics.
        /// Returns the left screen parent GameObject.
        /// </summary>
        /// <param name="logger">Main MelonLogger.Instance</param>
        /// <returns>Root GameObject for created left screen</returns>
        private GameObject InjectLeftScreen(MelonLogger.Instance logger)
        {
            // "Leaderboards" object from main scene used as reference for rotation and position
            // Leaderboards at local position (17.2, 0.0, -7.8) (global position (2.8, 2.0, -0.1)) with rotation (0.0, 0.4, 0.0, 0.9) (local rotation (0.0, 0.4, 0.0, 0.9))
            // "Modifiers" in main menu also helpful
            // Modifiers at local position (-17.2, 0.0, -8.3) (global position (-2.8, 2.0, -0.1)) with rotation (0.0, -0.4, 0.0, 0.9) (local rotation (0.0, -0.4, 0.0, 0.9))

            // But, the end game screen is further back in z, so adjust by "No Multiplayer/ScoreWrap"

            GameObject displayWrap = GameObject.Find("DisplayWrap");

            // Center screen
            GameObject centerScreen = displayWrap.transform.Find("No Multiplayer").gameObject;

            // Clone
            GameObject leftScreen = GameObject.Instantiate(
                centerScreen.gameObject,
                centerScreen.transform.position,
                centerScreen.transform.rotation,
                displayWrap.transform
            );
            leftScreen.name = "pmGameEndLeftScreen";

            // Move to left side
            Vector3 platformPosition = new Vector3(0, 0, 0);
            leftScreen.transform.RotateAround(platformPosition, Vector3.up, -75.0f);

            // Delete unwanted children
            UnityUtil.DeleteChildren(logger, leftScreen.transform, new string[] { "ScoreWrap" });

            // Hide everything by default in ScoreWrap
            UnityUtil.SetChildrenActive(leftScreen.transform.Find("ScoreWrap"), false);

            return leftScreen;
        }

        private void InjectAverageLifePctText(
            MelonLogger.Instance logger,
            GameObject leftScreen,
            float avgLifePct
        ) {
            Transform root = leftScreen.transform.Find("ScoreWrap/TotalScore");
            if (root == null)
            {
                logger.Msg("Failed to find root transform for average life percent text");
                return;
            }

            root.name = "pm_avgLifePct";
            UnityUtil.SetTMProText(root.Find("Label"), "Average Life Percentage");
            UnityUtil.SetTMProText(root.Find("Value"), string.Format("{0:0.###}%", avgLifePct * 100));
            UnityUtil.DeleteChildren(logger, root, new string[] { "Label", "Value", "Bg" });

            root.gameObject.SetActive(true);
        }
    }
}
