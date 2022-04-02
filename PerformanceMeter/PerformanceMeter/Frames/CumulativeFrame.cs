using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Frames
{
    public class CumulativeFrame
    {
        public float TimeMs { get; }
        public float Amount { get; }

        [BsonCtor]
        public CumulativeFrame(float timeMs, float amount)
        {
            this.TimeMs = timeMs;
            this.Amount = amount;
        }

        public PercentFrame ToPercentFrame(float totalAmount)
        {
            return new PercentFrame(TimeMs, Amount / totalAmount);
        }
    }
}
