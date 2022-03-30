using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Messages
{
    public class EventDataNoteMiss
    {
        public int multiplier = 1; // current, or after miss?
        public float lifeBarPercent = 1.0f;
    }
}
