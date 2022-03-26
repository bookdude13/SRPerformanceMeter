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

        public void Inject(
            MelonLogger.Instance logger,
            Dictionary<int, float> lifePctFrames
        ) {
            GameObject leftScreen = InjectLeftScreen(logger);

            InjectTitle(logger, leftScreen);

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

        private void InjectAverageLifePercentText(
            MelonLogger.Instance logger,
            GameObject leftScreen,
            float avgLifePct
        ) {
            Transform root = leftScreen.transform.Find("ScoreWrap/Streak");
            if (root == null)
            {
                logger.Msg("Failed to find root transform for average life percent text");
                return;
            }

            root.name = "pm_avgLifePct";

            var labelText = root.Find("Label").GetComponent<TMPro.TMP_Text>();
            labelText.SetText("Average Life Percent: ");

            var valueText = root.Find("Value").GetComponent<TMPro.TMP_Text>();
            valueText.SetText(string.Format("{0:0.###}%", avgLifePct * 100));

            var transforms = root.GetComponentsInChildren<Transform>();
            foreach (var transform in transforms) {
                transform.localPosition += new Vector3(2.0f, 0.0f, 0.0f);
            }

            UnityUtil.DeleteChildren(logger, root, new string[] { "Label", "Value", "Bg" });

            root.gameObject.SetActive(true);
        }

        /*private void InjectAverageLifePercent2(
            MelonLogger.Instance logger,
            GameObject leftScreen,
            float avgLifePct
        ) {
            // Use same background as center screen stats
            Sprite bgSprite = leftScreen.transform.Find("ScoreWrap/Streak/Bg")?.GetComponent<SpriteRenderer>()?.sprite;
            
            var lbl = leftScreen.transform.Find("ScoreWrap/Streak/Label");
            var fonts = lbl.GetComponentsInChildren<TMPro.TMP_FontAsset>();
            foreach (var font in fonts)
            {
                logger.Msg(font.name + " // " + font.boldStyle);
            }

            GameObject container = new GameObject("pm_avgLifePctContainer", typeof(Canvas));
            container.transform.SetParent(leftScreen.transform, false);
            var containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.0f, 0.0f);
            containerRect.anchorMax = new Vector2(1.0f, 0.0f);
            containerRect.sizeDelta = new Vector2(10.0f, 0.7f);

            GameObject bg = new GameObject("pm_avgLifePctBg", typeof(Image));
            bg.transform.SetParent(container.transform, false);
            var bgRect = bg.GetComponent<RectTransform>();
            FillParent(bgRect);
            var bgImage = bg.GetComponent<Image>();
            bgImage.sprite = bgSprite;

            GameObject text = new GameObject("pm_avgLifePctText", typeof(TMPro.TextMeshPro));
            text.transform.SetParent(container.transform, false);
            var textTMP = text.GetComponent<TMPro.TextMeshPro>();
            //textTMP.font = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>().First(t => t.name == "NotoSansJP-Bold"));
            textTMP.fontSize = 6.0f;
            textTMP.alignment = TMPro.TextAlignmentOptions.Center;
            textTMP.SetText(string.Format("Average Life Percentage: {0:0.###}%", avgLifePct * 100));
            textTMP.enableAutoSizing = true;

            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.0f, 0.0f);
            textRect.anchorMax = new Vector2(1.0f, 0.0f);
            textRect.localPosition = Vector3.zero;
            textRect.sizeDelta = new Vector2(5.0f, 0.4f);
        }*/

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

            // Remove unused pieces
            UnityUtil.DeleteChildren(logger, parent, new string[] { "pm_avgLifePct", "title" });

            // Container
            GameObject graphContainer = new GameObject("pm_lifePctGraphContainer", typeof(Canvas));
            graphContainer.transform.SetParent(parent, false);

            var containerRect = graphContainer.GetComponent<RectTransform>();
            containerRect.localPosition = Vector3.zero;
            containerRect.localEulerAngles = Vector3.zero;
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(20.0f, 15.0f);

            // Border
            var borderSprite = UnityUtil.CreateSpriteFromAssemblyResource(logger, "PerformanceMeter.Resources.Sprites.bg.png");

            GameObject graphBorder = GameObject.Instantiate(new GameObject("pm_lifePctGraphBg", typeof(Image)), graphContainer.transform);
            var borderImage = graphBorder.GetComponent<Image>();
            borderImage.sprite = borderSprite;
            borderImage.color = Color.black;

            FillParent(graphBorder.GetComponent<RectTransform>());

            // Graphable Region
            var padding = new Vector2(0.4f, 0.4f);
            var graphableRegionSize = containerRect.sizeDelta - padding;
            var graphableRect = CreateGraphableRegion(graphContainer.transform, graphableRegionSize);

            // Nodes
            var pointSprite = UnityUtil.CreateSpriteFromAssemblyResource(logger, "PerformanceMeter.Resources.Sprites.circle.png");
            AddPointsToGraph(graphableRect, pointSprite, lifePctFrames);
        }

        private RectTransform CreateGraphableRegion(Transform graphContainer, Vector2 sizeDelta)
        {
            var graphableRegion = new GameObject("pm_lifePctGraphArea", typeof(Canvas));
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

        private void AddPointsToGraph(RectTransform graphableRect, Sprite pointSprite, Dictionary<int, float> lifePctFrames)
        {
            float lastTimeMs = lifePctFrames.Last().Key;
            RectTransform previousDot = null;
            float previousPct = 0f;
            foreach (KeyValuePair<int, float> frameData in lifePctFrames)
            {
                float pctTime = frameData.Key / lastTimeMs;
                float lifePct = frameData.Value;
                GameObject dot = CreatePoint(graphableRect, pointSprite, pctTime, lifePct);
                RectTransform newRect = dot.GetComponent<RectTransform>();
                if (previousDot != null)
                {
                    CreateLineSegment(
                        graphableRect,
                        previousDot.anchoredPosition,
                        newRect.anchoredPosition,
                        GetColorForPercent((previousPct + lifePct) / 2.0f)
                    );
                }
                previousDot = newRect;
                previousPct = lifePct;
            }
        }

        private void FillParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        private GameObject CreatePoint(RectTransform graphContainer, Sprite sprite, float pctTime, float lifePct)
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
            image.color = GetColorForPercent(lifePct);
            image.enabled = true;

            float margin = 0.1f;
            var rectTransform = dot.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(pctTime * (graphWidth - margin), lifePct * (graphHeight - margin));
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
            rectTransform.sizeDelta = new Vector2(distance, 0.02f);
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
