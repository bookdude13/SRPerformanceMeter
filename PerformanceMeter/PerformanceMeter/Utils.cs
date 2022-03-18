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
        /// Assumes frame is present for first and last times (bookended). Returns 0 if less than two points
        /// </summary>
        /// <param name="lifePctFrames">(timeMs,  pct 0.0 to 1.0)</param>
        /// <returns>0 if less than two points, else the average life over time</returns>
        public static float CalculateAverageLifePercent(Dictionary<float, float> lifePctFrames)
        {
            List<float> times = lifePctFrames.Keys.ToList();
            times.Sort();

            if (times.Count < 2)
            {
                return 0f;
            }

            float songDurationMs = times.Last() - times.First();

            float sum = 0.0f;
            for (var i = 0; i < times.Count - 1; i++)
            {
                // Accumulate percentage value for the full duration of this chunk
                float timeDiffMs = times[i + 1] - times[i];
                float lifePct = lifePctFrames[times[i]];
                sum += timeDiffMs * lifePct;
            }

            return sum / songDurationMs;
        }
    }
}
