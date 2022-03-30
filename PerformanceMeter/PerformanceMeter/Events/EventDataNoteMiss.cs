using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Messages
{
    public class EventDataNoteMiss
    {
        public int multiplier = 1; // multiplier before miss reset
        public float lifeBarPercent = 1.0f;
    }
}
