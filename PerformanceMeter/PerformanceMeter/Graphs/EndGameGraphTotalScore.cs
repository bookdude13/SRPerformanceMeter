using PerformanceMeter.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerformanceMeter.Graphs
{
    class EndGameGraphTotalScore : EndGameGraph
    {
        private List<CumulativeFrame> bestScoreFrames;
        private List<CumulativeFrame> currentScoreFrames;

        public EndGameGraphTotalScore(
            ConfigManager config,
            List<CumulativeFrame> bestScoreFrames,
            List<CumulativeFrame> currentScoreFrames
        ) : base(config)
        {
            this.bestScoreFrames = bestScoreFrames;
            this.currentScoreFrames = currentScoreFrames;
        }

        public override void Inject(MelonLoggerWrapper logger, Transform parent)
        {
            CreateGraphContainer(logger, parent, "pm_totalScoreContainer");

            float topScore = Math.Max(bestScoreFrames.Last().Amount, currentScoreFrames.Last().Amount);
            var currentScorePctFrames = currentScoreFrames.Select(cumFrame => cumFrame.ToPercentFrame(topScore)).ToList();
            var bestScorePctFrames = bestScoreFrames.Select(cumFrame => cumFrame.ToPercentFrame(topScore)).ToList();

            InjectPercentGraph(logger, container, bestScorePctFrames, pct => Color.white);
            InjectPercentGraph(logger, container, currentScorePctFrames, pct => Color.yellow);
        }

        public override string GetTitle()
        {
            return "Total Score";
        }
    }
}
