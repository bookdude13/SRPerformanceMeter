using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter
{
    public static class Utils
    {
        /// <summary>
        /// Assumes frame is present for first and last times (book-ended). Returns 0 if less than two points
        /// </summary>
        /// <param name="lifePctFrames">Ordered list of time/pct values</param>
        /// <returns>0 if less than two points, else the average life over time</returns>
        public static float CalculateAverageLifePercent(List<LifePercentFrame> lifePctFrames)
        {
            if (lifePctFrames.Count < 2)
            {
                return 0f;
            }

            float songDurationMs = lifePctFrames.Last().timeMs - lifePctFrames.First().timeMs;

            float sum = 0.0f;
            for (var i = 0; i < lifePctFrames.Count - 1; i++)
            {
                // Accumulate percentage value for the full duration of this chunk
                float timeDiffMs = lifePctFrames[i + 1].timeMs - lifePctFrames[i].timeMs;
                float lifePct = lifePctFrames[i].lifePercent;
                sum += timeDiffMs * lifePct;
            }

            return sum / songDurationMs;
        }
    }
}
