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
        private static Color red = Color.red;
        private static Color orange = new Color(1.0f, 0.65f, 0.0f);
        private static Color yellowGreen = new Color(0.81f, 0.98f, 0.2f);
        private static Color green = Color.green;
        private static Color colorMarker = new Color(0.6f, 0.6f, 0.6f, 0.8f);
        private static Color colorAverageLine = new Color(0.9f, 0.9f, 0.9f, 0.5f);

        private bool showAverageLine;
        private int markerPeriodMs;

        public EndGameDisplay(bool showAverageLine, int markerPeriodMs)
        {
            this.showAverageLine = showAverageLine;
            this.markerPeriodMs = markerPeriodMs;
        }

        public void Inject(
            MelonLogger.Instance logger,
            List<PercentFrame> lifePctFrames
        ) {
            GameObject leftScreen = InjectLeftScreen(logger);

            InjectTitle(logger, leftScreen);

            float avgLifePct = Utils.CalculateAveragePercent(lifePctFrames);
            logger.Msg("Average life pct: " + avgLifePct);
            InjectAveragePercentText(logger, leftScreen, "Average Life Percent: ", avgLifePct);

            InjectPercentGraph(logger, leftScreen, lifePctFrames, avgLifePct);
        }

        /// <summary>
        /// Clones the center screen and moves it to the left to put additional statistics.
        /// Returns the left screen parent GameObject.
        /// </summary>
        /// <param name="logger">Main MelonLogger.Instance</param>
        /// <returns>Root GameObject for created left screen</returns>
        private GameObject InjectLeftScreen(MelonLogger.Instance logger)
        {
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

            // Delete unwanted children
            UnityUtil.DeleteChildren(logger, leftScreen.transform.Find("ScoreWrap"), new string[] { "TotalScore", "Streak" });

            return leftScreen;
        }

        private void InjectTitle(MelonLogger.Instance logger, GameObject leftScreen)
        {
            Transform root = leftScreen.transform.Find("ScoreWrap/TotalScore");
            if (root == null)
            {
                logger.Msg("Failed to find root transform");
                return;
            }

            root.name = "pm_title";

            var labelText = root.Find("Label").GetComponent<TMPro.TMP_Text>();
            labelText.SetText("");

            var valueText = root.Find("Value").GetComponent<TMPro.TMP_Text>();
            valueText.SetText("Performance");
            valueText.color = Color.white;

            UnityUtil.DeleteChildren(logger, root, new string[] { "Label", "Value" });

            root.gameObject.SetActive(true);
        }

        private void InjectAveragePercentText(
            MelonLogger.Instance logger,
            GameObject leftScreen,
            string labelText,
            float averagePercent
        ) {
            Transform root = leftScreen.transform.Find("ScoreWrap/Streak");
            if (root == null)
            {
                logger.Msg("Failed to find root transform for average percent text");
                return;
            }

            root.name = "pm_avgPct";

            var labelTMP = root.Find("Label").GetComponent<TMPro.TMP_Text>();
            labelTMP.SetText(labelText);

            var valueText = root.Find("Value").GetComponent<TMPro.TMP_Text>();
            valueText.SetText(string.Format("{0:0.###}%", averagePercent * 100));

            var transforms = root.GetComponentsInChildren<Transform>();
            foreach (var transform in transforms) {
                transform.localPosition += new Vector3(2.0f, 0.0f, 0.0f);
            }

            UnityUtil.DeleteChildren(logger, root, new string[] { "Label", "Value", "Bg" });

            root.gameObject.SetActive(true);
        }

        private void InjectPercentGraph(
            MelonLogger.Instance logger,
            GameObject leftScreen,
            List<PercentFrame> percentFrames,
            float averagePercent
        ) {
            Transform parent = leftScreen.transform.Find("ScoreWrap");
            if (parent == null)
            {
                logger.Msg("Failed to find root transform for graph");
                return;
            }

            // Remove unused pieces
            UnityUtil.DeleteChildren(logger, parent, new string[] { "pm_avgPct", "title" });

            // Container
            GameObject graphContainer = new GameObject("pm_graphContainer", typeof(Canvas));
            graphContainer.transform.SetParent(parent, false);

            var containerRect = graphContainer.GetComponent<RectTransform>();
            containerRect.localPosition = Vector3.zero;
            containerRect.localEulerAngles = Vector3.zero;
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(20.0f, 15.0f);

            // Border
            var borderSprite = UnityUtil.CreateSpriteFromAssemblyResource(logger, "PerformanceMeter.Resources.Sprites.bg.png");

            GameObject graphBorder = GameObject.Instantiate(new GameObject("pm_graphBg", typeof(Image)), graphContainer.transform);
            var borderImage = graphBorder.GetComponent<Image>();
            borderImage.sprite = borderSprite;
            borderImage.color = Color.black;

            FillParent(graphBorder.GetComponent<RectTransform>());

            // Side indicators 0 / 100
            var label100 = CreateLabel(graphBorder.transform, new Vector2(-0.6f, 0.0f), "100");
            label100.anchorMin = new Vector2(0f, 1f);
            label100.anchorMax = new Vector2(0f, 1f);
            label100.GetComponent<TMPro.TextMeshPro>().alignment = TMPro.TextAlignmentOptions.MidlineRight;

            var label0 = CreateLabel(graphBorder.transform, new Vector2(-0.25f, 0.0f), "0");
            label0.anchorMin = new Vector2(0f, 0f);
            label0.anchorMax = new Vector2(0f, 0f);

            // Graphable Region
            var padding = new Vector2(0.4f, 0.4f);
            var graphableRegionSize = containerRect.sizeDelta - padding;
            var graphableRect = CreateGraphableRegion(graphContainer.transform, graphableRegionSize);

            // Nodes
            var pointSprite = UnityUtil.CreateSpriteFromAssemblyResource(logger, "PerformanceMeter.Resources.Sprites.circle.png");
            AddPointsToGraph(graphableRect, pointSprite, percentFrames);

            // Time markers
            // Treat last recorded event as end of song (ignoring outros etc)
            float songDurationMs = percentFrames.Last().timeMs;
            logger.Msg("Duration: " + songDurationMs);
            for (var markerMs = markerPeriodMs; markerMs < songDurationMs; markerMs += markerPeriodMs)
            {
                float pctX = markerMs / songDurationMs;
                CreateTimeMarker(graphableRect, pctX);
            }

            if (showAverageLine)
            {
                CreateAverageLine(graphableRect, averagePercent);
            }
        }

        /// <summary>
        /// Creates label with text within parent, anchored at (0.5, 0.5)
        /// </summary>
        /// <param name="parent">Parent of new label</param>
        /// <param name="anchoredPosition">Position of text relative to anchor point</param>
        /// <param name="text">Text to set</param>
        /// <returns>RectTransform of the new label</returns>
        private RectTransform CreateLabel(Transform parent, Vector2 anchoredPosition, string text)
        {
            var label = new GameObject("pm_graphLabel", typeof(TMPro.TextMeshPro));
            label.transform.SetParent(parent, false);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = anchoredPosition;

            var labelTMP = label.GetComponent<TMPro.TextMeshPro>();
            labelTMP.fontSize = 6f;
            labelTMP.SetText(text);
            labelTMP.autoSizeTextContainer = true;

            return labelRect;
        }

        private RectTransform CreateGraphableRegion(Transform graphContainer, Vector2 sizeDelta)
        {
            var graphableRegion = new GameObject("pm_graphArea", typeof(Canvas));
            graphableRegion.transform.SetParent(graphContainer.transform, false);
            graphableRegion.AddComponent<CanvasRenderer>();

            var graphableRect = graphableRegion.GetComponent<RectTransform>();
            graphableRect.localPosition = Vector3.zero;
            graphableRect.localEulerAngles = Vector3.zero;
            graphableRect.anchorMin = new Vector2(0.5f, 0.5f);
            graphableRect.anchorMax = new Vector2(0.5f, 0.5f);
            graphableRect.sizeDelta = sizeDelta;

            return graphableRect;
        }

        private void AddPointsToGraph(RectTransform graphableRect, Sprite pointSprite, List<PercentFrame> pctFrames, bool changeColors = true)
        {
            float lastTimeMs = pctFrames.Last().timeMs;
            RectTransform previousDot = null;
            float previousPct = 0f;
            foreach (PercentFrame frameData in pctFrames)
            {
                float percentTime = frameData.timeMs / lastTimeMs;
                float percentOfTotal = frameData.percentOfTotal;
                Color color = changeColors ? GetColorForPercent(percentOfTotal) : Color.white;
                GameObject dot = CreatePoint(graphableRect, pointSprite, percentTime, percentOfTotal, color);
                RectTransform newRect = dot.GetComponent<RectTransform>();
                if (previousDot != null)
                {
                    color = changeColors ? GetColorForPercent((previousPct + percentOfTotal) / 2.0f) : Color.white;
                    CreateLineSegment(
                        graphableRect,
                        previousDot.anchoredPosition,
                        newRect.anchoredPosition,
                        color
                    );
                }
                previousDot = newRect;
                previousPct = percentOfTotal;
            }
        }

        private void FillParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        private GameObject CreatePoint(RectTransform graphContainer, Sprite sprite, float pctTime, float pctOfTotal, Color color)
        {
            float graphWidth = graphContainer.sizeDelta.x;
            float graphHeight = graphContainer.sizeDelta.y;

            var dot = new GameObject("pm_graphCircle", typeof(Image));
            dot.transform.SetParent(graphContainer, false);

            var image = dot.GetComponent<Image>();
            if (sprite != null)
            {
                image.sprite = sprite;
            }
            image.color = color;
            image.enabled = true;

            var rectTransform = dot.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(pctTime * graphWidth, pctOfTotal * graphHeight);
            rectTransform.sizeDelta = new Vector2(0.04f, 0.06f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            return dot;
        }

        private GameObject CreateTimeMarker(RectTransform graphContainer, float pctTime)
        {
            float graphWidth = graphContainer.sizeDelta.x;
            float graphHeight = graphContainer.sizeDelta.y;

            var marker = new GameObject("pm_graphTimeMark", typeof(Image));
            marker.transform.SetParent(graphContainer, false);

            var image = marker.GetComponent<Image>();
            image.color = colorMarker;
            image.enabled = true;

            var rectTransform = marker.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(pctTime * graphWidth, 0.0f);
            rectTransform.sizeDelta = new Vector2(0.03f, 0.5f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            return marker;
        }

        private GameObject CreateAverageLine(RectTransform graphContainer, float averagePercent)
        {
            var graphHeight = graphContainer.sizeDelta.y;

            var line = new GameObject("pm_graphAverageLine", typeof(Image));
            line.transform.SetParent(graphContainer, false);

            var image = line.GetComponent<Image>();
            image.color = colorAverageLine;
            image.enabled = true;

            var rectTransform = line.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.sizeDelta = new Vector2(0f, 0.04f);
            rectTransform.anchoredPosition = new Vector2(0f, averagePercent * graphHeight);

            return line;
        }

        private GameObject CreateLineSegment(RectTransform graphContainer, Vector2 from, Vector2 to, Color color)
        {
            var segment = new GameObject("pm_graphLineSegment", typeof(Image));
            segment.transform.SetParent(graphContainer, false);

            var image = segment.GetComponent<Image>();
            image.color = color;
            image.enabled = true;

            var rectTransform = segment.GetComponent<RectTransform>();
            var direction = (to - from).normalized;
            var distance = Vector2.Distance(from, to);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(distance, 0.04f);
            rectTransform.anchoredPosition = from + direction * distance * .5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            return segment;
        }

        private static Color GetColorForPercent(float lifePct)
        {
            Color color;

            if (lifePct < 0.5f)
            {
                color = red;
            }
            else if (lifePct < 0.75f)
            {
                color = orange;
            }
            else if (lifePct < 0.9f)
            {
                color = yellowGreen;
            }
            else
            {
                color = green;
            }

            return color;
        }
    }
}
