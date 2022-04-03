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
    public class BestRunsService
    {
        private readonly ILogger logger;
        private readonly BestRunsRepository repo;

        public BestRunsService(ILogger logger, BestRunsRepository repo)
        {
            this.logger = logger;
            this.repo = repo;
        }

        public BestTotalScore GetBestTotalScore(PlayConfiguration playConfiguration)
        {
            BestRun bestRun = repo.GetBestRun(playConfiguration.Id);
            return bestRun?.TotalScore;
        }

        public void UpdateBestTotalScore(PlayConfiguration playConfiguration, List<CumulativeFrame> totalScoreFrames)
        {
            // TODO distinguish between raw (no mod) and normal (with mod)
            float endScoreRaw = totalScoreFrames.Last().Amount;
            float endScore = totalScoreFrames.Last().Amount;
            BestTotalScore newScore = new BestTotalScore(endScoreRaw, endScore, totalScoreFrames);

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
            else if (newScore.EndScore >= bestRun.TotalScore.EndScore)
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

        public BestLifePercent GetBestLifePercent(PlayConfiguration playConfiguration)
        {
            BestRun bestRun = repo.GetBestRun(playConfiguration.Id);
            return bestRun?.LifePercent;
        }

        public void UpdateBestLifePercent(PlayConfiguration playConfiguration, float averageLifePercent, List<PercentFrame> lifePercentFrames)
        {
            // TODO distinguish between raw (no mod) and normal (with mod)
            BestLifePercent newEntry = new BestLifePercent(averageLifePercent, lifePercentFrames);

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
            else if (newEntry.AverageLifePercent >= bestRun.LifePercent.AverageLifePercent)
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
