using PerformanceMeter.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PerformanceMeter.Graphs
{
    abstract class EndGameGraph
    {
        protected static Color red = Color.red;
        protected static Color orange = new Color(1.0f, 0.65f, 0.0f);
        protected static Color yellowGreen = new Color(0.81f, 0.98f, 0.2f);
        protected static Color green = Color.green;
        protected static Color colorMarker = new Color(0.6f, 0.6f, 0.6f, 0.8f);
        protected static Color colorAverageLine = new Color(0.9f, 0.9f, 0.9f, 0.5f);

        protected ConfigManager config;
        protected RectTransform container = null;

        protected EndGameGraph(ConfigManager config)
        {
            this.config = config;
        }

        public abstract void Inject(MelonLoggerWrapper logger, Transform parent);
        public abstract string GetTitle();

        public void Show()
        {
            this.container?.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.container?.gameObject.SetActive(false);
        }

        /// <summary>
        /// Creates a container GameObject to hold any one of the graph types.
        /// Needs to be called from Inject() before being used as a parent Transform
        /// </summary>
        /// <returns>RectTransform of the created container GameObject</returns>
        protected void CreateGraphContainer(MelonLoggerWrapper logger, Transform parent, string containerName)
        {
            var container = new GameObject(containerName, typeof(Canvas));
            container.transform.SetParent(parent, false);
            container.AddComponent<CanvasRenderer>();
            container.AddComponent<Image>();

            var containerRect = container.GetComponent<RectTransform>();
            containerRect.localPosition = Vector3.zero;
            containerRect.localEulerAngles = Vector3.zero;
            containerRect.anchorMin = new Vector2(0f, 0.5f);
            containerRect.anchorMax = new Vector2(1f, 0.5f);
            containerRect.sizeDelta = new Vector2(20.0f, 14.0f);
            containerRect.anchoredPosition = new Vector2(0f, 5.0f);

            container.GetComponent<Image>().color = Color.clear;

            this.container = containerRect;
        }

        protected RectTransform CreateGraphableRegion(Transform graphContainer, Vector2 sizeDelta)
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

        protected void InjectPercentGraph(
            MelonLoggerWrapper logger,
            Transform parent,
            List<PercentFrame> percentFrames,
            Func<float, Color> fnGetColor,
            float averagePercent = -1f
        )
        {
            // Remove unused pieces
            UnityUtil.DeleteChildren(logger, parent, new string[] { "pm_avgPct", "title" });

            // Container
            GameObject graphContainer = new GameObject("pm_graphContainer", typeof(Canvas));
            graphContainer.transform.SetParent(parent, false);

            var containerRect = graphContainer.GetComponent<RectTransform>();
            containerRect.localPosition = Vector3.zero;
            containerRect.localEulerAngles = Vector3.zero;
            containerRect.anchorMin = new Vector2(0.5f, 1f);
            containerRect.anchorMax = new Vector2(0.5f, 1f);
            containerRect.sizeDelta = new Vector2(20.0f, 15.0f);
            containerRect.anchoredPosition = new Vector2(0f, -16.0f);

            // Background
            var backgroundSprite = UnityUtil.CreateSpriteFromAssemblyResource(logger, "PerformanceMeter.Resources.Sprites.bg.png");

            GameObject graphBackground = GameObject.Instantiate(new GameObject("pm_graphBg", typeof(Image)), graphContainer.transform);
            var backgroundImage = graphBackground.GetComponent<Image>();
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = Color.black;

            FillParent(graphBackground.GetComponent<RectTransform>());

            // Graphable Region
            var padding = new Vector2(0.4f, 0.4f);
            var graphableRegionSize = containerRect.sizeDelta - padding;
            var graphableRect = CreateGraphableRegion(graphContainer.transform, graphableRegionSize);

            // Nodes
            var pointSprite = UnityUtil.CreateSpriteFromAssemblyResource(logger, "PerformanceMeter.Resources.Sprites.circle.png");
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

        protected void AddPointsToGraph(
            RectTransform graphableRect,
            Sprite pointSprite,
            List<PercentFrame> pctFrames,
            Func<float, Color> fnGetColor
        )
        {
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
        protected void FillParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        protected GameObject CreatePoint(RectTransform graphContainer, Sprite sprite, float pctTime, float pctOfTotal, Color color)
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
            rectTransform.sizeDelta = new Vector2(0.04f, 0.04f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            return dot;
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
            rectTransform.sizeDelta = new Vector2(distance, 0.06f);
            rectTransform.anchoredPosition = from + direction * distance * .5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            return segment;
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
    }
}
