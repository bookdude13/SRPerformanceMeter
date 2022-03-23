using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace PerformanceMeter
{
    class EndGameDisplay
    {
        public void Inject(
            MelonLogger.Instance logger,
            Dictionary<int, float> lifePctFrames
        ) {
            GameObject leftScreen = InjectLeftScreen(logger);

            float avgLifePct = Utils.CalculateAverageLifePercent(lifePctFrames);
            logger.Msg("Average life pct: " + avgLifePct);
            InjectAverageLifePercentText(logger, leftScreen, avgLifePct);

            InjectLifePercentGraph(logger, leftScreen, lifePctFrames);
            UnityUtil.LogGameObjectHierarchy(logger, leftScreen.transform);
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
            leftScreen.name = "pm_gameEndLeftScreen";

            // Move to left side
            Vector3 platformPosition = new Vector3(0, 0, 0);
            leftScreen.transform.RotateAround(platformPosition, Vector3.up, -75.0f);

            // Delete unwanted children
            UnityUtil.DeleteChildren(logger, leftScreen.transform, new string[] { "ScoreWrap" });

            // Hide everything by default in ScoreWrap
            UnityUtil.SetChildrenActive(leftScreen.transform.Find("ScoreWrap"), false);

            return leftScreen;
        }

        private void InjectAverageLifePercentText(
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

        private void InjectLifePercentGraph(
            MelonLogger.Instance logger,
            GameObject leftScreen,
            Dictionary<int, float> lifePctFrames
        ) {
            Transform parent = leftScreen.transform.Find("ScoreWrap");
            if (parent == null)
            {
                logger.Msg("Failed to find root transform for graph");
                return;
            }

            // Copy background sprite for later
            Sprite bgSprite = parent.Find("AccuracyWrap/Force/Bg").GetComponent<SpriteRenderer>().sprite;

            // Remove unused pieces
            UnityUtil.DeleteChildren(logger, parent, new string[] { "pm_avgLifePct", "title" });

            // Container
            GameObject graphContainer = new GameObject("pm_lifePctGraphContainer", typeof(RectTransform));
            graphContainer.transform.SetParent(parent);
            var containerRect = graphContainer.GetComponent<RectTransform>();
            containerRect.localPosition = Vector3.zero;
            containerRect.localEulerAngles = Vector3.zero;
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.zero;
            containerRect.sizeDelta = new Vector2(10f, 20f);

            //parent.transform.localScale = new Vector3(1.5f, 4.5f, 1.0f);

            /*            // Set title
                        Transform title = root.Find("title");
                        UnityUtil.SetTMProText(title, "Life Over Time");
                        title.localPosition += new Vector3(0, yOffset, 0);

                        // TODO maybe use this later, but for now hide it
                        title.gameObject.SetActive(false);
            */
            // Add graph section
            GameObject graphBg = GameObject.Instantiate(new GameObject(), graphContainer.transform);
            graphBg.name = "pm_lifePctGraphBg";
            graphBg.AddComponent<RectTransform>();
            var bgRect = graphBg.GetComponent<RectTransform>();

            // Fill parent
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // Background
            graphBg.AddComponent<SpriteRenderer>();
            SpriteRenderer bgSpriteRenderer = graphBg.GetComponent<SpriteRenderer>();
            bgSpriteRenderer.sprite = bgSprite;
            bgSpriteRenderer.color = Color.white;

            // Nodes
            /*            foreach (KeyValuePair<int, float> frameData in lifePctFrames)
                        {
                            int timeMs = frameData.Key;
                            float lifePct = frameData.Value;
                        }
            */

            GameObject dot = CreatePoint(containerRect, 0.0f, 1.0f);

            parent.gameObject.SetActive(true);

            //UnityUtil.LogComponentsRecursive(logger, leftScreen.transform);
        }

        private GameObject CreatePoint(RectTransform graphContainer, float pctTime, float lifePct)
        {
            float graphWidth = graphContainer.sizeDelta.x;
            float graphHeight = graphContainer.sizeDelta.y;

            var dot = new GameObject("pm_graphCircle", typeof(Image));
            dot.transform.SetParent(graphContainer.transform, false);
            var image = dot.GetComponent<Image>();
            image.color = Color.red;
            image.enabled = true;

            var rectTransform = image.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(pctTime * graphWidth, lifePct * graphHeight);
            rectTransform.sizeDelta = new Vector2(2f, 2f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            return dot;
        }
    }
}
