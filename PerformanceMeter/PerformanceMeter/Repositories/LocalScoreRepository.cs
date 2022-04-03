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
    public class LocalScoreRepository
    {
        private readonly ILogger logger;
        private readonly ILiteCollection<LocalScore> highScores;

        public LocalScoreRepository(ILogger logger, ILiteDatabase db)
        {
            this.logger = logger;
            this.highScores = db.GetCollection<LocalScore>("highScores");
        }

        public void SetLocalHighScore(LocalScore highScore)
        {
            try
            {
                var id = highScores.Upsert(highScore.Id, highScore);
                //highScores.EnsureIndex(score => score.Id);
            }
            catch (Exception e)
            {
                logger.Msg("Failed to set local high score: " + e.Message);
            }
        }

        public LocalScore GetLocalHighScore(string user, string mapHash, string difficulty, string modifiers)
        {
            try
            {
                var asdf = highScores.FindAll();
                return highScores.FindById(LocalScore.GetIdentifier(user, mapHash, difficulty, modifiers));
            }
            catch (Exception e)
            {
                logger.Msg("Failed to retrieve local high score: " + e.Message);
                return null;
            }
        }
    }
}
