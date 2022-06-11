using LiteDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceMeter.Frames;
using PerformanceMeter.Models;
using PerformanceMeter.Repositories;
using PerformanceMeter.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeterTests
{
    [TestClass]
    public class TestBestRunsService
    {
        private readonly string username = "bookdude13";
        private readonly string mapHash = "asdf";
        private readonly string difficulty = "Master";
        private readonly string gameMode = "Force";
        private readonly List<string> modifiers = new List<string>();

        private ILiteDatabase db;
        private BestRunRepository repo;
        private BestRunService service;
        private PlayConfiguration playConfiguration;

        [TestInitialize]
        public void Before()
        {
            db = new LiteDatabase(":memory:");
            var logger = new LoggerForTest();
            repo = new BestRunRepository(logger, db);
            service = new BestRunService(logger, repo);
            playConfiguration = new PlayConfiguration()
            {
                Id = Guid.NewGuid(),
                Username = username,
                MapHash = mapHash,
                Difficulty = difficulty,
                GameMode = gameMode,
                Modifiers = modifiers
            };
        }

        private List<CumulativeFrame> CreateTotalScoreFrames()
        {
            return new List<CumulativeFrame>()
            {
                new CumulativeFrame(0.0f, 0.0f),
                new CumulativeFrame(1.0f, 500.0f),
                new CumulativeFrame(2.5f, 2350.0f),
            };
        }

        private List<PercentFrame> CreateLifePercentFrames(float averagePercent)
        {
            return new List<PercentFrame>()
            {
                new PercentFrame(0.0f, 1.0f),
                new PercentFrame(1.0f, averagePercent),
                new PercentFrame(2.0f, averagePercent),
                new PercentFrame(2.5f, averagePercent - (1.0f - averagePercent)),
                new PercentFrame(3.5f, averagePercent - (1.0f - averagePercent)),
            };
        }

        [TestMethod]
        public void TestGetBestTotalScore_NoExisting_ReturnsNull()
        {
            TotalScoreRun highScore = service.GetBestTotalScore(playConfiguration);
            Assert.IsNull(highScore);
        }

        [TestMethod]
        public void TestUpdateBestTotalScore_NoExistingScore_SetsScore()
        {
            var frames = CreateTotalScoreFrames();
            service.UpdateBestTotalScore(playConfiguration, frames);

            // Should be set
            TotalScoreRun bestTotalScore = service.GetBestTotalScore(playConfiguration);
            Assert.IsNotNull(bestTotalScore);
            Assert.AreEqual(frames.Last().Amount, bestTotalScore.EndScore);
            Assert.AreEqual(frames.Count, bestTotalScore.TotalScoreFrames.Count);
        }

        [TestMethod]
        public void TestUpdateBestTotalScore_OnlyUpdatesIfLarger()
        {
            // Add initial score
            var frames = CreateTotalScoreFrames();
            frames.Add(new CumulativeFrame(999f, 99000.0f));
            service.UpdateBestTotalScore(playConfiguration, frames);

            // Same details, but smaller score - not updated
            var lowerScoreFrames = CreateTotalScoreFrames();
            lowerScoreFrames.Add(new CumulativeFrame(999f, 88000.0f));
            service.UpdateBestTotalScore(playConfiguration, lowerScoreFrames);

            TotalScoreRun highScore = service.GetBestTotalScore(playConfiguration);
            Assert.IsNotNull(highScore);
            Assert.AreEqual(99000.0f, highScore.EndScore);

            // Same details, but higher score - updated
            var higherScoreFrames = CreateTotalScoreFrames();
            higherScoreFrames.Add(new CumulativeFrame(999f, 100900.0f));
            service.UpdateBestTotalScore(playConfiguration, higherScoreFrames);

            var newHighScore = service.GetBestTotalScore(playConfiguration);
            Assert.IsNotNull(newHighScore);
            Assert.AreEqual(100900.0f, newHighScore.EndScore);
        }

        [TestMethod]
        public void TestGetBestLifePercent_NoExisting_ReturnsNull()
        {
            LifePercentRun bestLifePercent = service.GetBestLifePercent(playConfiguration);
            Assert.IsNull(bestLifePercent);
        }

        [TestMethod]
        public void TestUpdateBestLifePercent_NoExisting_Sets()
        {
            float averageLifePercent = 0.8f;
            var frames = CreateLifePercentFrames(averageLifePercent);

            service.UpdateBestLifePercent(playConfiguration, frames);

            // Should be set
            LifePercentRun best = service.GetBestLifePercent(playConfiguration);
            Assert.IsNotNull(best);
            Assert.AreEqual(0.8f, best.AverageLifePercent, 0.00001);
            Assert.AreEqual(frames.Count, best.LifePercentFrames.Count);
        }

        [TestMethod]
        public void TestUpdateLifePercent_OnlyUpdatesIfLarger()
        {
            var initialLifePercent = 0.9f;

            // Add initial
            service.UpdateBestLifePercent(playConfiguration, CreateLifePercentFrames(initialLifePercent));

            // Same details, but smaller - not updated
            service.UpdateBestLifePercent(playConfiguration, CreateLifePercentFrames(initialLifePercent - 0.01f));

            LifePercentRun best = service.GetBestLifePercent(playConfiguration);
            Assert.IsNotNull(best);
            Assert.AreEqual(0.9f, best.AverageLifePercent);

            // Same details, but higher - updated
            service.UpdateBestLifePercent(playConfiguration, CreateLifePercentFrames(initialLifePercent + 0.01f));
            LifePercentRun newBest = service.GetBestLifePercent(playConfiguration);
            Assert.IsNotNull(best);
            Assert.AreEqual(0.9f, best.AverageLifePercent);
        }
    }
}
