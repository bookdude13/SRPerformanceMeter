using LiteDB;
using PerformanceMeter.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Models
{
    public class BestTotalScore
    {
        public long EpochTimeSet { get; set; }
        public double EndScoreRaw { get; set; }
        public double EndScore { get; set; }
        public List<CumulativeFrame> TotalScoreFrames { get; set; }

        public BestTotalScore()
        {
        }

        public BestTotalScore(double endScoreRaw, double endScore, List<CumulativeFrame> totalScoreFrames)
        {
            this.EpochTimeSet = DateTimeOffset.Now.ToUnixTimeSeconds();
            this.EndScoreRaw = endScoreRaw;
            this.EndScore = endScore;
            this.TotalScoreFrames = totalScoreFrames;
        }
    }
}
