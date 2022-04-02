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
    public class LocalScoreService
    {
        private readonly ILogger logger;
        private readonly LocalScoreRepository repo;

        public LocalScoreService(ILogger logger, LocalScoreRepository repo)
        {
            this.logger = logger;
            this.repo = repo;
        }

        public void AddScore(string username, string mapHash, string difficulty, string modifiers, List<CumulativeFrame> totalScoreFrames)
        {
            // TODO only set if higher score
            var localHighScore = new LocalScore(username, mapHash, difficulty, modifiers, totalScoreFrames);
            repo.SetLocalHighScore(localHighScore);
        }

        /// <summary>
        /// Gets the score frames for the locally saved high score. Returns null if no score set yet.
        /// </summary>
        /// <param name="user">User to look up score for</param>
        /// <param name="mapHash">Hash (identifier) of map</param>
        /// <param name="difficulty">Difficulty of map</param>
        /// <param name="modifiers">Modifiers used on map, in alphabetized, lowercase, csv format</param>
        /// <returns>High score if it exists, null if no score set yet</returns>
        public LocalScore GetHighScore(string user, string mapHash, string difficulty, string modifiers)
        {
            LocalScore highScore = repo.GetLocalHighScore(user, mapHash, difficulty, modifiers);
            return highScore;
        }
    }
}
