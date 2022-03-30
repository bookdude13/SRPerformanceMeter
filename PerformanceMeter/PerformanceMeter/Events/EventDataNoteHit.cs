using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Messages
{
    public class EventDataNoteHit
    {
        public int score = 0; // score _after_ note hit
        public int combo = 0;
        public int multiplier = 1;
        public float completed = 1.0f; // may be wrong
        public float lifeBarPercent = 1.0f;
    }
}
