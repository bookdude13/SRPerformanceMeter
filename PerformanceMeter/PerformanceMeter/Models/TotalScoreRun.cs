using LiteDB;
using PerformanceMeter.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Models
{
    public class TotalScoreRun
    {
        public long EpochTimeSet { get; set; }
        public float EndScoreRaw { get; set; }
        public float EndScore { get; set; }
        public List<CumulativeFrame> TotalScoreFrames { get; set; }

        public TotalScoreRun()
        {
        }

        public TotalScoreRun(float endScoreRaw, float endScore, List<CumulativeFrame> totalScoreFrames)
        {
            this.EpochTimeSet = DateTimeOffset.Now.ToUnixTimeSeconds();
            this.EndScoreRaw = endScoreRaw;
            this.EndScore = endScore;
            this.TotalScoreFrames = totalScoreFrames;
        }
    }
}
