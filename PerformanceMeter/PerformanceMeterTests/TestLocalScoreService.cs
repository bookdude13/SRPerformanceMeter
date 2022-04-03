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
    public class TestLocalScoreService
    {
        private readonly string username = "bookdude13";
        private readonly string mapHash = "asdf";
        private readonly string difficulty = "Master";
        private readonly string modifiers = "";

        private ILiteDatabase db;
        private LocalScoreRepository repo;
        private LocalScoreService service;

        [TestInitialize]
        public void Before()
        {
            db = new LiteDatabase(":memory:");
            var logger = new LoggerForTest();
            repo = new LocalScoreRepository(logger, db);
            service = new LocalScoreService(logger, repo);
        }

        private List<CumulativeFrame> CreateScoreFrames()
        {
            return new List<CumulativeFrame>()
            {
                new CumulativeFrame(0.0f, 0.0f),
                new CumulativeFrame(1.0f, 500.0f),
                new CumulativeFrame(2.5f, 2350.0f),
            };
        }

        [TestMethod]
        public void TestGetHighScore_NoExistingHighScore_ReturnsNull()
        {
            LocalScore highScore = service.GetHighScore(username, mapHash, difficulty, modifiers);
            Assert.IsNull(highScore);
        }

        [TestMethod]
        public void TestAddScore_NoExistingHighScore_SetsHighScore()
        {
            var totalScoreFrames = CreateScoreFrames();
            service.AddScore(username, mapHash, difficulty, modifiers, totalScoreFrames);

            // Added, so now high score should be set
            LocalScore highScore = service.GetHighScore(username, mapHash, difficulty, modifiers);
            Assert.IsNotNull(highScore);
            for (var i = 0; i < totalScoreFrames.Count; i++)
            {
                Assert.AreEqual(totalScoreFrames.ElementAt(i).TimeMs, highScore.TotalScoreFrames.ElementAt(i).TimeMs);
                Assert.AreEqual(totalScoreFrames.ElementAt(i).Amount, highScore.TotalScoreFrames.ElementAt(i).Amount);
            }
        }

        [TestMethod]
        public void TestAddScore_OnlyUpdatesIfLarger()
        {
            // Add initial score
            var highScoreFrames = CreateScoreFrames();
            highScoreFrames.Add(new CumulativeFrame(999f, 99000.0f));
            service.AddScore(username, mapHash, difficulty, modifiers, highScoreFrames);

            // Same details, but smaller score - not updated
            var lowerScoreFrames = CreateScoreFrames();
            lowerScoreFrames.Add(new CumulativeFrame(999f, 88000.0f));
            service.AddScore(username, mapHash, difficulty, modifiers, lowerScoreFrames);
            LocalScore highScore = service.GetHighScore(username, mapHash, difficulty, modifiers);
            Assert.IsNotNull(highScore);
            Assert.AreEqual(99000.0f, highScore.TotalScoreFrames.Last().Amount);

            // Same details, but higher score - updated
            var higherScoreFrames = CreateScoreFrames();
            higherScoreFrames.Add(new CumulativeFrame(999f, 100999.0f));
            service.AddScore(username, mapHash, difficulty, modifiers, higherScoreFrames);
            highScore = service.GetHighScore(username, mapHash, difficulty, modifiers);
            Assert.IsNotNull(highScore);
            Assert.AreEqual(100999.0f, highScore.TotalScoreFrames.Last().Amount);
        }
    }
}
