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
    class EndGameGraphLifePercent : EndGameGraph
    {
        private List<PercentFrame> lifePctFrames;
        private GameObject clonedStatGameObject;

        public EndGameGraphLifePercent(
            ConfigManager config,
            List<PercentFrame> lifePctFrames,
            GameObject clonedStatGameObject
        ) : base(config)
        {
            this.lifePctFrames = lifePctFrames;
            this.clonedStatGameObject = clonedStatGameObject;
        }

        public override void Inject(MelonLoggerWrapper logger, Transform parent)
        {
            float avgLifePct = Utils.CalculateAveragePercent(lifePctFrames);
            logger.Msg("Average life pct: " + avgLifePct);

            CreateGraphContainer(logger, parent, "pm_lifePctContainer");
            InjectAverageStat(logger, container, clonedStatGameObject, "Average Life Percent: ", avgLifePct);
            InjectPercentGraph(logger, container, lifePctFrames, GetColorForLifePercent, avgLifePct);
        }

        public override string GetTitle()
        {
            return "Life Percentage";
        }

        private void InjectAverageStat(
            MelonLoggerWrapper logger,
            Transform parent,
            GameObject statToClone,
            string labelText,
            float averagePercent
        )
        {
            var averageStat = GameObject.Instantiate(statToClone, parent, false);
            averageStat.name = "pm_averageStatContainer";

            averageStat.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
            averageStat.transform.localEulerAngles = Vector3.zero;

            var labelTMP = averageStat.transform.Find("Label").GetComponent<TMPro.TMP_Text>();
            labelTMP.SetText(labelText);

            var valueText = averageStat.transform.Find("Value").GetComponent<TMPro.TMP_Text>();
            valueText.SetText(string.Format("{0:0.###}%", averagePercent * 100));

            UnityUtil.DeleteChildren(logger, averageStat.transform, new string[] { "Label", "Value", "Bg" });

            averageStat.SetActive(true);
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
