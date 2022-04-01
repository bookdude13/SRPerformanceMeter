using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter
{
    public static class Utils
    {
        private static bool IsSceneStage(string sceneName)
        {
            if (sceneName == null || !sceneName.Contains("."))
            {
                return false;
            }

            string subName = sceneName.Split('.')[1];
            bool isNormalStage = subName.StartsWith("Stage");
            bool isSpinStage = subName.StartsWith("Static Stage");
            bool isSpiralStage = subName.StartsWith("Spiral Stage");

            return isNormalStage || isSpinStage || isSpiralStage;
        }


        /// <summary>
        /// Assumes frame is present for first and last times (book-ended). Returns 0 if less than two points
        /// </summary>
        /// <param name="pctFrames">Ordered list of time/pct values</param>
        /// <returns>0 if less than two points, else the average percent over time</returns>
        public static float CalculateAveragePercent(List<PercentFrame> pctFrames)
        {
            if (pctFrames.Count < 2)
            {
                return 0f;
            }

            float songDurationMs = pctFrames.Last().timeMs - pctFrames.First().timeMs;

            float sum = 0.0f;
            for (var i = 0; i < pctFrames.Count - 1; i++)
            {
                // Accumulate percentage value for the full duration of this chunk
                float timeDiffMs = pctFrames[i + 1].timeMs - pctFrames[i].timeMs;
                float percentOfTotal = pctFrames[i].percentOfTotal;
                sum += timeDiffMs * percentOfTotal;
            }

            return sum / songDurationMs;
        }
    }
}
