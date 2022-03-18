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
            Game_ScoreSceneController scoreSceneController,
            float avgLifePct
        ) {
            InjectLeftScreen(logger);
            InjectAverageLifePctText(logger, scoreSceneController, avgLifePct);
        }

        private void InjectLeftScreen(MelonLogger.Instance logger)
        {
            // "Leaderboards" object from main scene used as reference for rotation and position
            // Leaderboards at local position (17.2, 0.0, -7.8) (global position (2.8, 2.0, -0.1)) with rotation (0.0, 0.4, 0.0, 0.9) (local rotation (0.0, 0.4, 0.0, 0.9))
            // "Modifiers" in main menu also helpful
            // Modifiers at local position (-17.2, 0.0, -8.3) (global position (-2.8, 2.0, -0.1)) with rotation (0.0, -0.4, 0.0, 0.9) (local rotation (0.0, -0.4, 0.0, 0.9))

            // But, the end game screen is further back in z, so adjust by "No Multiplayer/ScoreWrap"

            GameObject displayWrap = GameObject.Find("DisplayWrap");

            // Center screen
            GameObject centerScreen = displayWrap.transform.Find("No Multiplayer").gameObject;
            Transform centerScoreWrap = centerScreen.transform.Find("ScoreWrap");

            Vector3 leftScreenPosition = new Vector3(-20.0f, 0.0f, -8.0f);
            Quaternion leftScreenRotation = Quaternion.Euler(0, -90, 0);

            // Clone
            GameObject leftScreen = GameObject.Instantiate(
                centerScreen.gameObject,
                leftScreenPosition,
                leftScreenRotation,
                displayWrap.transform
            );
            leftScreen.name = "pmGameEndLeftScreen";
            Transform leftScreenScoreWrap = leftScreen.transform.Find("ScoreWrap");

            // Remove all unwanted children
            foreach (Transform child in leftScreen.transform)
            {
                if (child.name != "ScoreWrap")
                {
                    logger.Msg("Destroying child " + child.name);
                    GameObject.Destroy(child.gameObject);
                }
            }

            foreach (Transform child in leftScreenScoreWrap.transform)
            {
                if (child.name == "Streak")
                {
                    foreach (SpriteRenderer c in child.GetComponents<SpriteRenderer>())
                    {
                        logger.Msg("Component " + c + " " + c.color);
                    }
                }
                if (child.name != "ScoreTestMode")
                {
                    logger.Msg("Destroying child " + child.name);
                    GameObject.Destroy(child.gameObject);
                }
            }

            Transform scoreTestMode = leftScreenScoreWrap.Find("ScoreTestMode");

            // Main graph area
            GameObject graphWrap = scoreTestMode.gameObject;
            graphWrap.transform.localPosition = Vector3.zero;

            RectTransform rectTransform = graphWrap.GetComponent<RectTransform>();
            MeshRenderer meshRenderer = graphWrap.GetComponentInChildren<MeshRenderer>();
            CanvasRenderer canvasRenderer = graphWrap.GetComponentInChildren<CanvasRenderer>();
            TMPro.TextMeshPro tmPro = graphWrap.GetComponentInChildren<TMPro.TextMeshPro>();
            MeshFilter meshFilter = graphWrap.GetComponentInChildren<MeshFilter>();
            logger.Msg("rectTransform " + rectTransform.rect + "  " + rectTransform.anchoredPosition + "  " + rectTransform.localScale);
            rectTransform.rect.Set(0, 0, 400, 300);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            logger.Msg("rectTransform after: " + rectTransform.rect + "  " + rectTransform.anchoredPosition + "  " + rectTransform.localScale);
            meshRenderer.material.color = Color.white;
            tmPro.SetText("Average life percentage: XX.XXX%");

            graphWrap.AddComponent<SpriteRenderer>();
            graphWrap.GetComponent<SpriteRenderer>().color = Color.blue;

            /*// Logging
            Component[] components = leftScreenScoreWrap.GetComponentsInChildren<Component>();
            foreach (var component in components)
            {
                LoggerInstance.Msg("**Component " + component);
            }*/

            // Add in background

            //Synth.Finder.ButtonsFinder.this_instance.hoverCollider.transform.localPosition

            // Show
            //leftScreen.SetActive(true);

            //Util.LogGameObjectHierarchy(GameObject.Find("DisplayWrap").transform);
        }

        private void InjectAverageLifePctText(
            MelonLogger.Instance logger,
            Game_ScoreSceneController scoreSceneController,
            float avgLifePct
        ) {
            TMPro.TMP_Text duplicatedObject = scoreSceneController.totalScore;
            Transform parentTransform = duplicatedObject.transform.parent;

            GameObject duplicateText = GameObject.Instantiate(
                duplicatedObject.gameObject,
                parentTransform
            );

            string labelChildName = "Label";
            string valueChildName = "Value";

            int numChildren = duplicateText.transform.childCount;
            for (int i = numChildren - 1; i >= 0; i--)
            {
                Transform child = duplicateText.transform.GetChild(i);
                if (child.name == labelChildName)
                {
                    UnityUtil.SetTMProText(child, "Avg life pct: ");
                }
                else if (child.name == valueChildName)
                {
                    UnityUtil.SetTMProText(child, string.Format("{0:0.##}%", avgLifePct * 100));
                }
                else
                {
                    logger.Msg("Removing child " + child);
                    GameObject.Destroy(child.gameObject);
                }
            }

            duplicateText.name = "pmAvgLifePct";


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
    }
}
