using LiteDB;
using PerformanceMeter.Models;
using SRModCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Repositories
{
    public class PlayConfigurationRepository
    {
        private readonly SRLogger logger;
        private readonly ILiteCollection<PlayConfiguration> playConfigurations;

        public PlayConfigurationRepository(SRLogger logger, ILiteDatabase db)
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
                    // As of Synth Riders Remastered I don't know how to get the username...
                    // So, don't require it to match in the query (otherwise the nulls cause it to never get the existing record)
                    //.Where(config => config.Username == username)
                    .Where(config => config.MapHash == mapHash)
                    .Where(config => config.Difficulty == difficulty)
                    .Where(config => config.GameMode == gameMode)
                    .Where(config => config.Modifiers == modifiers)
                    .FirstOrDefault();

            }
            catch (Exception e)
            {
                logger.Msg($"Failed to retrieve play configuration for {username} {mapHash} {difficulty} {gameMode} {modifiers}: {e.Message}");

                return null;
            }
        }
    }
}
