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
            //Sprite bgSprite = parent.Find("AccuracyWrap/Force/Bg").GetComponent<SpriteRenderer>().sprite;

            // Remove unused pieces
            UnityUtil.DeleteChildren(logger, parent, new string[] { "pm_avgLifePct", "title" });

            // Container
            GameObject graphContainer = new GameObject("pm_lifePctGraphContainer", typeof(Canvas));
            graphContainer.transform.SetParent(parent);
            graphContainer.AddComponent<CanvasRenderer>();

            var containerRect = graphContainer.GetComponent<RectTransform>();
            containerRect.localPosition = Vector3.zero;
            containerRect.localEulerAngles = Vector3.zero;

            // Size
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(20.0f, 15.0f);

            // Background
            GameObject graphBg = GameObject.Instantiate(new GameObject("pm_lifePctGraphBg", typeof(Image)), graphContainer.transform);
            var bgImage = graphBg.GetComponent<Image>();
            //bgImage.sprite = bgSprite;
            bgImage.color = Color.white;

            // Fill parent
            var bgRect = graphBg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;


            // Nodes
            float lastTimeMs = lifePctFrames.Last().Key;
            RectTransform previousDot = null;
            foreach (KeyValuePair<int, float> frameData in lifePctFrames)
            {
                float pctTime = frameData.Key / lastTimeMs;
                float lifePct = frameData.Value;
                GameObject dot = CreatePoint(containerRect, pctTime, lifePct);
                RectTransform newRect = dot.GetComponent<RectTransform>();
                if (previousDot != null)
                {
                    CreateLineSegment(containerRect, previousDot.anchoredPosition, newRect.anchoredPosition);
                }
                previousDot = newRect;
            }
        }

        private GameObject CreatePoint(RectTransform graphContainer, float pctTime, float lifePct)
        {
            float graphWidth = graphContainer.sizeDelta.x;
            float graphHeight = graphContainer.sizeDelta.y;

            var dot = new GameObject("pm_graphCircle", typeof(Image));
            dot.transform.SetParent(graphContainer, false);

            var image = dot.GetComponent<Image>();
            //image.sprite = sprite;
            image.color = Color.red;
            image.enabled = true;

            float margin = 0.1f;
            var rectTransform = dot.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(pctTime * (graphWidth - margin), lifePct * (graphHeight - margin));
            rectTransform.sizeDelta = new Vector2(0.04f, 0.04f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            return dot;
        }

        private GameObject CreateLineSegment(RectTransform graphContainer, Vector2 from, Vector2 to)
        {
            var segment = new GameObject("pm_graphLineSegment", typeof(Image));
            segment.transform.SetParent(graphContainer, false);

            var image = segment.GetComponent<Image>();
            image.color = Color.blue;
            image.enabled = true;

            var rectTransform = segment.GetComponent<RectTransform>();
            var direction = (to - from).normalized;
            var distance = Vector2.Distance(from, to);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(distance, 0.02f);
            rectTransform.anchoredPosition = from + direction * distance * .5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            return segment;
        }
    }
}
