using PerformanceMeter.Models;
using PerformanceMeter.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Services
{
    public class PlayConfigurationService
    {
        private readonly ILogger logger;
        private readonly PlayConfigurationRepository repo;

        public PlayConfigurationService(ILogger logger, PlayConfigurationRepository repo)
        {
            this.logger = logger;
            this.repo = repo;
        }

        /// <summary>
        /// Get the PlayConfiguration for this combined key, creating if it doesn't exist.
        /// </summary>
        /// <returns>PlayConfiguration, either previously existing or newly created. Null if failed to create.</returns>
        public PlayConfiguration GetPlayConfiguration(
            string username,
            string mapHash,
            string difficulty,
            string gameMode,
            List<string> modifiers
        ) {
            var configuration = repo.GetPlayConfiguration(username, mapHash, difficulty, gameMode, modifiers);
            if (configuration == null)
            {
                var newConfiguration = new PlayConfiguration()
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    MapHash = mapHash,
                    Difficulty = difficulty,
                    GameMode = gameMode,
                    Modifiers = modifiers
                };
                repo.AddPlayConfiguration(newConfiguration);
                configuration = repo.GetPlayConfiguration(username, mapHash, difficulty, gameMode, modifiers);
            }

            return configuration;
        }
    }
}
