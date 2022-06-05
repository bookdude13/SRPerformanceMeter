using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using PerformanceMeter.Frames;
using PerformanceMeter.Graphs;
using UnityEngine;
using UnityEngine.UI;

namespace PerformanceMeter
{
    class EndGameDisplay
    {
        private readonly ConfigManager config;
        private List<EndGameGraph> graphDisplays = new List<EndGameGraph>();
        private TMPro.TextMeshProUGUI selectionTitle;

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
            MelonLoggerWrapper logger,
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

            InjectMainTitle(logger, leftScreen);

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

            Transform clonedButtonTransform = leftScreen.transform.Find("Buttons/StandardButton - Retry");
            if (clonedButtonTransform == null)
            {
                logger.Msg("Failed to find transform to clone for button");
                return;
            }

            // Life percent
            if (config.showLifePercentGraph)
            {
                var graphLifePct = new EndGameGraphLifePercent(config, lifePctFrames, clonedStatTransform.gameObject);
                graphLifePct.Inject(logger, parent);
                graphDisplays.Add(graphLifePct);
            }

            // Total score comparison
            if (config.showTotalScoreComparisonGraph)
            {
                var graphTotalScore = new EndGameGraphTotalScore(config, bestScoreFrames, currentScoreFrames);
                graphTotalScore.Inject(logger, parent);
                graphDisplays.Add(graphTotalScore);
            }

            // Select each graph type
            var selectionContainer = InjectSelectionButtons(logger, leftScreen.transform);
            selectionTitle = InjectSelectionTitle(logger, selectionContainer, "No graph set in config");

            SelectGraph(logger, 1);

            clonedStatTransform.gameObject.SetActive(false);
        }

        private void SelectGraph(MelonLoggerWrapper logger, int index)
        {
            if (index < 0 || index >= graphDisplays.Count)
            {
                logger.Msg("Graph index out of range; ignoring index " + index + " with count " + graphDisplays.Count);
                return;
            }

            selectionTitle.SetText("");
            foreach (var graphDisplay in graphDisplays)
            {
                graphDisplay.Hide();
            }

            selectionTitle.SetText(graphDisplays[index].GetTitle());
            graphDisplays[index].Show();
        }

        private TMPro.TextMeshProUGUI InjectSelectionTitle(MelonLoggerWrapper logger, Transform parent, string titleText)
        {
            var label = new GameObject("pm_selectionTitle", typeof(TMPro.TextMeshProUGUI));
            label.transform.SetParent(parent, false);

            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;

            var labelTMP = label.GetComponent<TMPro.TextMeshProUGUI>();
            labelTMP.fontSize = 1f;
            labelTMP.SetText(titleText);
            labelTMP.autoSizeTextContainer = true;

            return labelTMP;
        }

        private Transform InjectSelectionButtons(MelonLoggerWrapper logger, Transform parent)
        {
            var selectionContainer = new GameObject("pm_selectionContainer", typeof(Canvas));
            selectionContainer.transform.SetParent(parent, false);
            selectionContainer.transform.localPosition = new Vector3(0f, 0f, 12.5f);
            selectionContainer.transform.localEulerAngles = Vector3.zero;

            var containerRect = selectionContainer.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.0f);
            containerRect.anchorMax = new Vector2(0.5f, 0.0f);
            containerRect.sizeDelta = new Vector2(18.0f, 3.0f);
            containerRect.anchoredPosition = new Vector2(0.0f, 12.0f);

            var image = selectionContainer.AddComponent<Image>();
            image.color = Color.green;

            // Left
            var btnLeft = new GameObject("pm_selectionBtnLeft", typeof(RectTransform));
            btnLeft.transform.SetParent(containerRect);
            btnLeft.transform.localPosition = Vector3.zero;
            btnLeft.transform.localEulerAngles = Vector3.zero;

            var btnLeftRect = btnLeft.GetComponent<RectTransform>();
            btnLeftRect.anchorMin = new Vector2(0.0f, 0.0f);
            btnLeftRect.anchorMax = new Vector2(0.0f, 1.0f);
            btnLeftRect.sizeDelta = new Vector2(2.0f, 1.0f);
            btnLeftRect.anchoredPosition = new Vector2(1.0f, 0.0f);

            var leftImg = btnLeft.AddComponent<Image>();
            leftImg.color = Color.blue;

            // Right
            var btnRight = new GameObject("pm_selectionBtnRight", typeof(RectTransform));
            btnRight.transform.SetParent(containerRect);
            btnRight.transform.localPosition = Vector3.zero;
            btnRight.transform.localEulerAngles = Vector3.zero;

            var btnRightRect = btnRight.GetComponent<RectTransform>();
            btnRightRect.anchorMin = new Vector2(1.0f, 0.0f);
            btnRightRect.anchorMax = new Vector2(1.0f, 1.0f);
            btnRightRect.sizeDelta = new Vector2(2.0f, 1.0f);
            btnRightRect.anchoredPosition = new Vector2(-1.0f, 0.0f);

            var rightImg = btnRight.AddComponent<Image>();
            rightImg.color = Color.red;

            return selectionContainer.transform;
        }

        /// <summary>
        /// Clones the center screen and moves it to the left to put additional statistics.
        /// Returns the left screen parent GameObject.
        /// </summary>
        /// <param name="logger">Main logger instance</param>
        /// <returns>Root GameObject for created left screen</returns>
        private GameObject InjectLeftScreen(MelonLoggerWrapper logger)
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

        private void InjectMainTitle(MelonLoggerWrapper logger, GameObject leftScreen)
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
    }
}
