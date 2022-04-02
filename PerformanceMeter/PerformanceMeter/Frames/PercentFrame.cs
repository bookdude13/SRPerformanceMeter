using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Frames
{
    public class PercentFrame
    {
        public float TimeMs { get; }
        public float PercentOfTotal { get; } // 0.0 to 1.0

        [BsonCtor]
        public PercentFrame(float timeMs, float percentOfTotal)
        {
            this.TimeMs = timeMs;
            this.PercentOfTotal = percentOfTotal;
        }
    }
}
