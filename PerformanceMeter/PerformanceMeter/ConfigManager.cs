using MelonLoader;
using SRModCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter
{
    public class ConfigManager
    {
        private static readonly int MARKER_PERIOD_MIN_MS = 1000;
        private static readonly int MARKER_PERIOD_MAX_MS = 5 * 60 * 1000;

        private readonly string modDirectory;

        public MelonPreferences_Category prefs;
        public bool isEnabled = true;
        public bool showLifePercentGraph = true;
        public bool showAverageLine = true;
        public bool showTotalScoreComparisonGraph = true;
        public int markerPeriodMs = 30000;

        public ConfigManager(string modDirectory)
        {
            this.modDirectory = modDirectory;
        }

        public void Initialize(SRLogger logger)
        {
            prefs = MelonPreferences.CreateCategory("MainPreferences", "Preferences");

            try
            {
                // Remove old empty config file from 1.0.0/1.1.0
                File.Delete("UserData/PerformanceMeter.cfg");
            }
            catch (Exception e)
            {
                logger.Msg("Failed to remove old config file: " + e.Message);
            }

            try
            {
                Directory.CreateDirectory(modDirectory);

                var configPath = Path.Combine(modDirectory, "PerformanceMeter.cfg");
                prefs.SetFilePath(configPath);

                var isEnabledEntry = prefs.CreateEntry("isEnabled", true, "Enabled");
                isEnabled = isEnabledEntry.Value;

                var showAverageLineEntry = prefs.CreateEntry("showAverageLine", true, "Show Average Line");
                showAverageLine = showAverageLineEntry.Value;

                var showLifePercentGraphEntry = prefs.CreateEntry("showLifePercentGraph", true, "Show Life Percentage Graph");
                showLifePercentGraph = showLifePercentGraphEntry.Value;

                var showTotalScoreComparisonGraphEntry = prefs.CreateEntry("showTotalScoreComparisonGraph", true, "Show Total Score Comparison Graph");
                showTotalScoreComparisonGraph = showTotalScoreComparisonGraphEntry.Value;

                var markerPeriodMsEntry = prefs.CreateEntry("markerPeriodMs", 30000, "Marker Period (ms)");
                markerPeriodMs = markerPeriodMsEntry.Value;
                if (markerPeriodMs < MARKER_PERIOD_MIN_MS)
                {
                    logger.Msg("markerPeroidMs is less than minimum; did you put in seconds instead of milliseconds? Using min of " + MARKER_PERIOD_MIN_MS);
                    markerPeriodMs = MARKER_PERIOD_MIN_MS;
                }
                markerPeriodMs = Math.Min(MARKER_PERIOD_MAX_MS, markerPeriodMs);

                logger.Msg("Config Loaded");
                logger.Msg("  Enabled? " + isEnabled);
                logger.Msg("  Show average line? " + showAverageLine);
                logger.Msg("  Show life percent graph? " + showLifePercentGraph);
                logger.Msg("  Show total score comparison graph? " + showTotalScoreComparisonGraph);
                logger.Msg("  markerPeriodMs: " + markerPeriodMs);
            }
            catch (Exception e)
            {
                logger.Msg("Failed to setup config values. Msg: " + e.Message);
            }
        }

    }
}
