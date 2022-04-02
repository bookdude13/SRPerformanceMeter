using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Frames
{
    public class CumulativeFrame
    {
        public readonly float timeMs;
        public readonly float amount;

        public CumulativeFrame(float timeMs, float amount)
        {
            this.timeMs = timeMs;
            this.amount = amount;
        }

        public PercentFrame ToPercentFrame(float totalAmount)
        {
            return new PercentFrame(timeMs, amount / totalAmount);
        }
    }
}
