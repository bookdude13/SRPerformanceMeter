using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter
{
    public class PercentFrame
    {
        public readonly float timeMs;
        public readonly float percentOfTotal; // 0.0 to 1.0

        public PercentFrame(float timeMs, float percentOfTotal)
        {
            this.timeMs = timeMs;
            this.percentOfTotal = percentOfTotal;
        }
    }
}
