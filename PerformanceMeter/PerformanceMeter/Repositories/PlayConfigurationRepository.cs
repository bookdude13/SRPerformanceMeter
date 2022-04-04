using LiteDB;
using PerformanceMeter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Repositories
{
    public class PlayConfigurationRepository
    {
        private readonly ILogger logger;
        private readonly ILiteCollection<PlayConfiguration> playConfigurations;

        public PlayConfigurationRepository(ILogger logger, ILiteDatabase db)
        {
            this.logger = logger;
            this.playConfigurations = db.GetCollection<PlayConfiguration>("PlayConfiguration");
        }

        public Guid? AddPlayConfiguration(PlayConfiguration playConfiguration) {
            try
            {
                return playConfigurations.Insert(playConfiguration);
            }
            catch (Exception e)
            {
                logger.Msg("Failed to add play configuration: " + e.Message);
                return null;
            }
        }

        public PlayConfiguration GetPlayConfiguration(
            string username,
            string mapHash,
            string difficulty,
            string gameMode,
            List<string> modifiers
        ) {
            try
            {
                return playConfigurations.Query()
                    .Where(config => config.Username == username)
                    .Where(config => config.MapHash == mapHash)
                    .Where(config => config.Difficulty == difficulty)
                    .Where(config => config.GameMode == gameMode)
                    .Where(config => config.Modifiers == modifiers)
                    .FirstOrDefault();

            }
            catch (Exception e)
            {
                logger.Msg(string.Format(
                    "Failed to retrieve play configuration for {0} {1} {2} {3} {4}: {5}",
                    username,
                    mapHash,
                    difficulty,
                    gameMode,
                    modifiers.ToString(),
                    e.Message
                ));

                return null;
            }
        }
    }
}
