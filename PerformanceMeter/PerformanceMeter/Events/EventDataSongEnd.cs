using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Messages
{
    public class EventDataSongEnd
    {
        public string song = "";
        public int perfect = 0;
        public int normal = 0;
        public int bad = 0;
        public int fail = 0;
        public int highestCombo = 0;
    }
}
