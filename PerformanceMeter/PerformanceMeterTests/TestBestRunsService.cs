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
        private readonly string[] modifiers = new string[] { };

        private ILiteDatabase db;
        private BestRunsRepository repo;
        private BestRunsService service;
        private PlayConfiguration playConfiguration;

        [TestInitialize]
        public void Before()
        {
            db = new LiteDatabase(":memory:");
            var logger = new LoggerForTest();
            repo = new BestRunsRepository(logger, db);
            service = new BestRunsService(logger, repo);
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

        [TestMethod]
        public void TestGetBestTotalScore_NoExisting_ReturnsNull()
        {
            BestTotalScore highScore = service.GetBestTotalScore(playConfiguration);
            Assert.IsNull(highScore);
        }

        [TestMethod]
        public void TestUpdateBestTotalScore_NoExistingScore_SetsScore()
        {
            var frames = CreateTotalScoreFrames();
            service.UpdateBestTotalScore(playConfiguration, frames);

            // Should be set
            BestTotalScore bestTotalScore = service.GetBestTotalScore(playConfiguration);
            Assert.IsNotNull(bestTotalScore);
            Assert.AreEqual(frames.Last().Amount, bestTotalScore.EndScore);
            Assert.AreEqual(frames.Count, bestTotalScore.TotalScoreFrames.Count);
        }

        [TestMethod]
        public void TestAddScore_OnlyUpdatesIfLarger()
        {
            // Add initial score
            var frames = CreateTotalScoreFrames();
            frames.Add(new CumulativeFrame(999f, 99000.0f));
            service.UpdateBestTotalScore(playConfiguration, frames);

            // Same details, but smaller score - not updated
            var lowerScoreFrames = CreateTotalScoreFrames();
            lowerScoreFrames.Add(new CumulativeFrame(999f, 88000.0f));
            service.UpdateBestTotalScore(playConfiguration, lowerScoreFrames);

            BestTotalScore highScore = service.GetBestTotalScore(playConfiguration);
            Assert.IsNotNull(highScore);
            Assert.AreEqual(99000.0f, highScore.EndScore);

            // Same details, but higher score - updated
            var higherScoreFrames = CreateTotalScoreFrames();
            higherScoreFrames.Add(new CumulativeFrame(999f, 100900.0f));
            service.UpdateBestTotalScore(playConfiguration, higherScoreFrames);

            highScore = service.GetBestTotalScore(playConfiguration);
            Assert.IsNotNull(highScore);
            Assert.AreEqual(100900.0f, highScore.EndScore);
        }
    }
}
