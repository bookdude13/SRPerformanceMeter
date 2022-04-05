using LiteDB;
using PerformanceMeter.Frames;
using PerformanceMeter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Repositories
{
    public class BestRunRepository
    {
        private readonly ILogger logger;
        private readonly ILiteCollection<BestRun> bestRuns;

        public BestRunRepository(ILogger logger, ILiteDatabase db)
        {
            this.logger = logger;
            this.bestRuns = db.GetCollection<BestRun>("BestRun");
        }

        public void UpsertBestRun(BestRun bestRun)
        {
            try
            {
                var id = bestRuns.Upsert(bestRun.PlayConfigurationId, bestRun);
            }
            catch (Exception e)
            {
                logger.Msg("Failed to upsert best run with play configuration " + bestRun.PlayConfigurationId + ": " + e.Message);
            }
        }

        public BestRun GetBestRun(Guid playConfigurationId)
        {
            try
            {
                return bestRuns.FindById(playConfigurationId);
            }
            catch (Exception e)
            {
                logger.Msg("Failed to retrieve best run for play " + playConfigurationId + ": " + e.Message);
                return null;
            }
        }
    }
}
