using PerformanceMeter.Frames;
using PerformanceMeter.Models;
using PerformanceMeter.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Services
{
    public class BestRunService
    {
        private readonly ILogger logger;
        private readonly BestRunRepository repo;

        public BestRunService(ILogger logger, BestRunRepository repo)
        {
            this.logger = logger;
            this.repo = repo;
        }

        public TotalScoreRun GetBestTotalScore(PlayConfiguration playConfiguration)
        {
            BestRun bestRun = repo.GetBestRun(playConfiguration.Id);
            return bestRun?.TotalScore;
        }

        public void UpdateBestTotalScore(PlayConfiguration playConfiguration, List<CumulativeFrame> totalScoreFrames)
        {
            // TODO distinguish between raw (no mod) and normal (with mod)
            float endScoreRaw = totalScoreFrames.Last().Amount;
            float endScore = totalScoreFrames.Last().Amount;
            TotalScoreRun newScore = new TotalScoreRun(endScoreRaw, endScore, totalScoreFrames);

            BestRun bestRun = repo.GetBestRun(playConfiguration.Id);
            if (bestRun == null)
            {
                logger.Msg(string.Format("No best run found for {0}, creating", playConfiguration.Id));
                repo.UpsertBestRun(new BestRun()
                {
                    PlayConfigurationId = playConfiguration.Id,
                    TotalScore = newScore
                });
            }
            else if (newScore.EndScore >= (bestRun?.TotalScore?.EndScore ?? 0))
            {
                logger.Msg("New high score! Updating");
                bestRun.TotalScore = newScore;
                repo.UpsertBestRun(bestRun);
            }
            else
            {
                logger.Msg("Less than high score, ignoring");
            }
        }

        public LifePercentRun GetBestLifePercent(PlayConfiguration playConfiguration)
        {
            BestRun bestRun = repo.GetBestRun(playConfiguration.Id);
            return bestRun?.LifePercent;
        }

        public void UpdateBestLifePercent(PlayConfiguration playConfiguration, float averageLifePercent, List<PercentFrame> lifePercentFrames)
        {
            // TODO distinguish between raw (no mod) and normal (with mod)
            LifePercentRun newEntry = new LifePercentRun(averageLifePercent, lifePercentFrames);

            BestRun bestRun = repo.GetBestRun(playConfiguration.Id);
            if (bestRun == null)
            {
                logger.Msg(string.Format("No best run found for {0}, creating", playConfiguration.Id));
                repo.UpsertBestRun(new BestRun()
                {
                    PlayConfigurationId = playConfiguration.Id,
                    LifePercent = newEntry
                });
            }
            else if (newEntry.AverageLifePercent >= (bestRun?.LifePercent?.AverageLifePercent ?? 0))
            {
                logger.Msg("New best life percent average! Updating");
                bestRun.LifePercent = newEntry;
                repo.UpsertBestRun(bestRun);
            }
            else
            {
                logger.Msg("Less than best average life percent, ignoring");
            }
        }
    }
}
