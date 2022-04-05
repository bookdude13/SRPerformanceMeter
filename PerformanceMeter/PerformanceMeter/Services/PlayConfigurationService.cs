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

        public Guid? EnsurePlayConfiguration(PlayConfiguration playConfiguration)
        {
            try
            {
                var configuration = repo.GetPlayConfiguration(
                    playConfiguration.Username,
                    playConfiguration.MapHash,
                    playConfiguration.Difficulty,
                    playConfiguration.GameMode,
                    playConfiguration.Modifiers
                );
                if (configuration == null)
                {
                    logger.Msg("Play config missing; adding...");
                    playConfiguration.Id = Guid.NewGuid();
                    return repo.AddPlayConfiguration(playConfiguration);
                }
                else
                {
                    return configuration.Id;
                }
            }
            catch (Exception e)
            {
                logger.Msg("Failed to ensure play config " + playConfiguration.Id + ": " + e.Message);
                return null;
            }
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
