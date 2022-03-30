using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Messages
{
    public class EventDataSongStart
    {
        public string song = "";
        public string difficulty = "";
        public string author = "";
        public string beatMapper = "";
        public float length = 0.0f;
        public float bpm = 0.0f;
        public string albumArt = "";
    }
}
