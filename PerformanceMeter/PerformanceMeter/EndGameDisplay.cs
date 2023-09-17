using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using PerformanceMeter.Frames;
using PerformanceMeter.Models;
using SRModCore;
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

        private readonly ConfigManager config;

        public EndGameDisplay(ConfigManager config)
        {
            this.config = config;
        }

        /// <summary>
        /// Injects graph into end game screen
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="lifePctFrames">Frames throughout song tracking life percentage</param>
        /// <param name="bestScoreFrames">Best run's total score frames. If the same as current score, show last high score</param>
        /// <param name="currentScoreFrames">Frames throughout song tracking total score</param>
        public void Inject(
            SRLogger logger,
            List<PercentFrame> lifePctFrames,
            List<CumulativeFrame> bestScoreFrames,
            List<CumulativeFrame> currentScoreFrames
        ) {
            // If nothing is set to show, don't do anything
            if (!config.showLifePercentGraph && !config.showTotalScoreComparisonGraph)
            {
                logger.Msg("No graphs enabled, not injecting.");
                return;
            }

            GameObject leftScreen = InjectLeftScreen(logger);

            InjectTitle(logger, leftScreen);

            // Get some existing objects for later reference
            Transform parent = leftScreen.transform.Find("ScoreWrap");
            if (parent == null)
            {
                logger.Msg("Failed to find root transform for graph");
                return;
            }

            Transform clonedStatTransform = parent.Find("Streak");
            if (clonedStatTransform == null)
            {
                logger.Msg("Failed to find transform to clone for stats");
                return;
            }

            // Life percent
            if (config.showLifePercentGraph)
            {
                float avgLifePct = LifePercentRun.CalculateAveragePercent(lifePctFrames);
                InjectLifePercentGraph(logger, parent, clonedStatTransform.gameObject, avgLifePct, lifePctFrames);
            }

            // Total score comparison
            if (config.showTotalScoreComparisonGraph)
            {
                InjectTotalScoreComparisonGraph(logger, parent, bestScoreFrames, currentScoreFrames);
            }

            clonedStatTransform.gameObject.SetActive(false);
        }

        private void InjectTotalScoreComparisonGraph(
            SRLogger logger,
            Transform parent,
            List<CumulativeFrame> bestScoreFrames,
            List<CumulativeFrame> currentScoreFrames
        ) {
            RectTransform totalScoreGraphContainer = CreateGraphContainer(logger, parent, "pm_totalScoreContainer");

            float topScore = Math.Max(bestScoreFrames.Last().Amount, currentScoreFrames.Last().Amount);
            var currentScorePctFrames = currentScoreFrames.Select(cumFrame => cumFrame.ToPercentFrame(topScore)).ToList();
            var bestScorePctFrames = bestScoreFrames.Select(cumFrame => cumFrame.ToPercentFrame(topScore)).ToList();

            InjectPercentGraph(logger, totalScoreGraphContainer, bestScorePctFrames, pct => Color.white);
            InjectPercentGraph(logger, totalScoreGraphContainer, currentScorePctFrames, pct => Color.yellow);
        }

        private void InjectLifePercentGraph(
            SRLogger logger,
            Transform parent,
            GameObject clonedStatGameObject,
            float avgLifePct,
            List<PercentFrame> lifePctFrames
        ) {
            logger.Msg("Average life pct: " + avgLifePct);

            RectTransform lifePctGraphContainer = CreateGraphContainer(logger, parent, "pm_lifePctContainer");
            InjectAverageStat(logger, lifePctGraphContainer, clonedStatGameObject, "Average Life Percent: ", avgLifePct);
            InjectPercentGraph(logger, lifePctGraphContainer, lifePctFrames, GetColorForLifePercent, avgLifePct);
        }

        /// <summary>
        /// Creates a container GameObject to hold any one of the graph types
        /// </summary>
        /// <returns>RectTransform of the created container GameObject</returns>
        private RectTransform CreateGraphContainer(SRLogger logger, Transform parent, string containerName)
        {
            var container = new GameObject(containerName);
            container.transform.SetParent(parent, false);
            container.AddComponent<Canvas>();
            container.AddComponent<CanvasRenderer>();
            container.AddComponent<Image>();

            var containerRect = container.GetComponent<RectTransform>();
            containerRect.localPosition = Vector3.zero;
            containerRect.localEulerAngles = Vector3.zero;
            containerRect.anchorMin = new Vector2(0f, 0.5f);
            containerRect.anchorMax = new Vector2(1f, 0.5f);
            containerRect.sizeDelta = new Vector2(20.0f, 21.0f);
            containerRect.anchoredPosition = new Vector2(0f, 5.0f);

            container.GetComponent<Image>().color = Color.clear;

            return containerRect;
        }

        /// <summary>
        /// Clones the center screen and moves it to the left to put additional statistics.
        /// Returns the left screen parent GameObject.
        /// </summary>
        /// <param name="logger">Main logger</param>
        /// <returns>Root GameObject for created left screen</returns>
        private GameObject InjectLeftScreen(SRLogger logger)
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

        private void InjectTitle(SRLogger logger, GameObject leftScreen)
        {
            Transform root = leftScreen.transform.Find("ScoreWrap/TotalScore");
            if (root == null)
            {
                logger.Msg("Failed to find root transform");
                return;
            }

            root.name = "pm_title";

            var labelText = root.Find("Label").GetComponent<Il2CppTMPro.TMP_Text>();
            labelText.SetText("");

            var valueText = root.Find("Value").GetComponent<Il2CppTMPro.TMP_Text>();
            valueText.SetText("Performance");
            valueText.color = Color.white;

            UnityUtil.DeleteChildren(logger, root, new string[] { "Label", "Value" });

            root.gameObject.SetActive(true);
        }

        private void InjectAverageStat(
            SRLogger logger,
            Transform parent,
            GameObject statToClone,
            string labelText,
            float averagePercent
        ) {
            var averageStat = GameObject.Instantiate(statToClone, parent, false);
            averageStat.name = "pm_averageStatContainer";

            averageStat.transform.localPosition = new Vector3(0.0f, 6.0f, 0.0f);
            averageStat.transform.localEulerAngles = Vector3.zero;

            var labelTMP = averageStat.transform.Find("Label").GetComponent<Il2CppTMPro.TMP_Text>();
            labelTMP.SetText(labelText);

            var valueText = averageStat.transform.Find("Value").GetComponent<Il2CppTMPro.TMP_Text>();
            valueText.SetText(string.Format("{0:0.###}%", averagePercent * 100));

            UnityUtil.DeleteChildren(logger, averageStat.transform, new string[] { "Label", "Value", "Bg" });

            averageStat.SetActive(true);
        }

        private void InjectPercentGraph(
            SRLogger logger,
            Transform parent,
            List<PercentFrame> percentFrames,
            Func<float, Color> fnGetColor,
            float averagePercent = -1f
        ) {
            // Remove unused pieces
            UnityUtil.DeleteChildren(logger, parent, new string[] { "pm_avgPct", "title" });

            // Container
            GameObject graphContainer = new GameObject("pm_graphContainer");//, typeof(Canvas));
            graphContainer.transform.SetParent(parent, false);

            graphContainer.AddComponent<Canvas>();

            var containerRect = graphContainer.GetComponent<RectTransform>();
            containerRect.localPosition = Vector3.zero;
            containerRect.localEulerAngles = Vector3.zero;
            containerRect.anchorMin = new Vector2(0.5f, 1f);
            containerRect.anchorMax = new Vector2(0.5f, 1f);
            containerRect.sizeDelta = new Vector2(20.0f, 15.0f);
            containerRect.anchoredPosition = new Vector2(0f, -16.0f);

            // Background
            var backgroundSprite = UnityUtil.CreateSpriteFromAssemblyResource(logger, Assembly.GetExecutingAssembly(), "PerformanceMeter.Resources.Sprites.bg.png");

            //GameObject graphBackground = GameObject.Instantiate(new GameObject("pm_graphBg", typeof(Image)), graphContainer.transform);
            var graphBackground = new GameObject("pm_graphBg");
            graphBackground.transform.SetParent(graphContainer.transform);

            var backgroundImage = graphBackground.AddComponent<Image>();
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = Color.black;

            FillParent(graphBackground.GetComponent<RectTransform>());

            /*// Side indicators 0 / 100
            var label100 = CreateLabel(graphBackground.transform, new Vector2(-0.6f, 0.0f), "100");
            label100.anchorMin = new Vector2(0f, 1f);
            label100.anchorMax = new Vector2(0f, 1f);
            label100.GetComponent<TMPro.TextMeshPro>().alignment = TMPro.TextAlignmentOptions.MidlineRight;

            var label0 = CreateLabel(graphBackground.transform, new Vector2(-0.25f, 0.0f), "0");
            label0.anchorMin = new Vector2(0f, 0f);
            label0.anchorMax = new Vector2(0f, 0f);*/

            // Graphable Region
            var padding = new Vector2(0.4f, 0.4f);
            var graphableRegionSize = containerRect.sizeDelta - padding;
            var graphableRect = CreateGraphableRegion(graphContainer.transform, graphableRegionSize);

            // Nodes
            var pointSprite = UnityUtil.CreateSpriteFromAssemblyResource(logger, Assembly.GetExecutingAssembly(), "PerformanceMeter.Resources.Sprites.circle.png");
            AddPointsToGraph(graphableRect, pointSprite, percentFrames, fnGetColor);

            // Time markers
            // Treat last recorded event as end of song (ignoring outros etc)
            float songDurationMs = percentFrames.Last().TimeMs;
            for (var markerMs = config.markerPeriodMs; markerMs < songDurationMs; markerMs += config.markerPeriodMs)
            {
                float pctX = markerMs / songDurationMs;
                CreateTimeMarker(graphableRect, pctX);
            }

            // Average line
            if (config.showAverageLine && averagePercent >= 0)
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
            var label = new GameObject("pm_graphLabel");
            label.transform.SetParent(parent, false);

            // TODO this may break
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = anchoredPosition;

            var labelTMP = label.AddComponent<Il2CppTMPro.TextMeshPro>();
            labelTMP.fontSize = 6f;
            labelTMP.SetText(text);
            labelTMP.autoSizeTextContainer = true;

            return labelRect;
        }

        private RectTransform CreateGraphableRegion(Transform graphContainer, Vector2 sizeDelta)
        {
            var graphableRegion = new GameObject("pm_graphArea");//, typeof(Canvas));
            graphableRegion.transform.SetParent(graphContainer.transform, false);
            graphableRegion.AddComponent<Canvas>();
            graphableRegion.AddComponent<CanvasRenderer>();

            var graphableRect = graphableRegion.GetComponent<RectTransform>();
            graphableRect.localPosition = Vector3.zero;
            graphableRect.localEulerAngles = Vector3.zero;
            graphableRect.anchorMin = new Vector2(0.5f, 0.5f);
            graphableRect.anchorMax = new Vector2(0.5f, 0.5f);
            graphableRect.sizeDelta = sizeDelta;

            return graphableRect;
        }

        private void AddPointsToGraph(
            RectTransform graphableRect,
            Sprite pointSprite,
            List<PercentFrame> pctFrames,
            Func<float, Color> fnGetColor
        ) {
            float lastTimeMs = pctFrames.Last().TimeMs;
            RectTransform previousDot = null;
            float previousPct = 0f;
            foreach (PercentFrame frameData in pctFrames)
            {
                float percentTime = frameData.TimeMs / lastTimeMs;
                float percentOfTotal = frameData.PercentOfTotal;
                Color color = fnGetColor(percentOfTotal);
                GameObject dot = CreatePoint(graphableRect, pointSprite, percentTime, percentOfTotal, color);
                RectTransform newRect = dot.GetComponent<RectTransform>();
                if (previousDot != null)
                {
                    Color lineColor = fnGetColor((previousPct + percentOfTotal) / 2.0f);
                    CreateLineSegment(
                        graphableRect,
                        previousDot.anchoredPosition,
                        newRect.anchoredPosition,
                        lineColor
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

            var dot = new GameObject("pm_graphCircle");
            dot.transform.SetParent(graphContainer, false);

            var image = dot.AddComponent<Image>();
            if (sprite != null)
            {
                image.sprite = sprite;
            }
            image.color = color;
            image.enabled = true;

            var rectTransform = dot.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(pctTime * graphWidth, pctOfTotal * graphHeight);
            rectTransform.sizeDelta = new Vector2(0.04f, 0.04f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            return dot;
        }

        private GameObject CreateTimeMarker(RectTransform graphContainer, float pctTime)
        {
            float graphWidth = graphContainer.sizeDelta.x;
            float graphHeight = graphContainer.sizeDelta.y;

            var marker = new GameObject("pm_graphTimeMark");
            marker.transform.SetParent(graphContainer, false);

            var image = marker.AddComponent<Image>();
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

            var line = new GameObject("pm_graphAverageLine");
            line.transform.SetParent(graphContainer, false);

            var image = line.AddComponent<Image>();
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
            var segment = new GameObject("pm_graphLineSegment");
            segment.transform.SetParent(graphContainer, false);

            var image = segment.AddComponent<Image>();
            image.color = color;
            image.enabled = true;

            var rectTransform = segment.GetComponent<RectTransform>();
            var direction = (to - from).normalized;
            var distance = Vector2.Distance(from, to);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(distance, 0.06f);
            rectTransform.anchoredPosition = from + direction * distance * .5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            return segment;
        }

        private static Color GetColorForLifePercent(float lifePct)
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
