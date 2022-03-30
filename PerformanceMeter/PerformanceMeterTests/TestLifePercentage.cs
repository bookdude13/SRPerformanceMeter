using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using PerformanceMeter;

namespace PerformanceMeterTests
{
    [TestClass]
    public class TestLifePercentage
    {
        private List<LifePercentFrame> lifePctFrames;
        int songDurationMs = 10 * 1000;
        private float delta = 0.0001f;

        [TestInitialize()]
        public void Setup()
        {
            lifePctFrames = new List<LifePercentFrame>();
        }

        [TestMethod]
        public void TestCalculateLifePercentage_NotEnoughFrames_Returns0()
        {
            float avgLifePct = Utils.CalculateAverageLifePercent(lifePctFrames);
            Assert.AreEqual(0f, avgLifePct, delta);
        }

        [TestMethod]
        public void TestCalculateLifePercentage_OnlyStartEnd_Returns100()
        {
            lifePctFrames.Add(new LifePercentFrame(0, 1.0f));
            lifePctFrames.Add(new LifePercentFrame(songDurationMs, 1.0f));

            float avgLifePct = Utils.CalculateAverageLifePercent(lifePctFrames);
            Assert.AreEqual(1.0f, avgLifePct, delta);
        }

        [TestMethod]
        public void TestCalculateLifePercentage_SlowDrop_ReturnsAverage()
        {
            // Start at 1, then go down to 0.8 in middle, then 0.6 at last tenth
            // 0.5 * 1 + 0.4 * 0.8 + 0.1 * 0.6 = 0.88
            lifePctFrames.Add(new LifePercentFrame(0, 1.0f));
            lifePctFrames.Add(new LifePercentFrame(songDurationMs / 2, 0.8f));
            lifePctFrames.Add(new LifePercentFrame((int) (songDurationMs * 0.9f), 0.6f));
            lifePctFrames.Add(new LifePercentFrame(songDurationMs, 0.6f));

            float avgLifePct = Utils.CalculateAverageLifePercent(lifePctFrames);
            Assert.AreEqual(0.88f, avgLifePct, delta);
        }
    }
}
