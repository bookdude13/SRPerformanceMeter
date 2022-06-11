using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using PerformanceMeter;
using PerformanceMeter.Frames;
using PerformanceMeter.Models;

namespace PerformanceMeterTests
{
    [TestClass]
    public class TestLifePercentage
    {
        private List<PercentFrame> percentFrames;
        private readonly int songDurationMs = 10 * 1000;
        private readonly float delta = 0.0001f;

        [TestInitialize()]
        public void Setup()
        {
            percentFrames = new List<PercentFrame>();
        }

        [TestMethod]
        public void TestCalculatePercentage_NotEnoughFrames_Returns0()
        {
            float averagePercent = LifePercentRun.CalculateAveragePercent(percentFrames);
            Assert.AreEqual(0f, averagePercent, delta);
        }

        [TestMethod]
        public void TestCalculatePercentage_OnlyStartEnd_Returns100()
        {
            percentFrames.Add(new PercentFrame(0, 1.0f));
            percentFrames.Add(new PercentFrame(songDurationMs, 1.0f));

            float averagePercent = LifePercentRun.CalculateAveragePercent(percentFrames);
            Assert.AreEqual(1.0f, averagePercent, delta);
        }

        [TestMethod]
        public void TestCalculatePercentage_SlowDrop_ReturnsAverage()
        {
            // Start at 1, then go down to 0.8 in middle, then 0.6 at last tenth
            // 0.5 * 1 + 0.4 * 0.8 + 0.1 * 0.6 = 0.88
            percentFrames.Add(new PercentFrame(0, 1.0f));
            percentFrames.Add(new PercentFrame(songDurationMs / 2, 0.8f));
            percentFrames.Add(new PercentFrame((int) (songDurationMs * 0.9f), 0.6f));
            percentFrames.Add(new PercentFrame(songDurationMs, 0.6f));

            float averagePercent = LifePercentRun.CalculateAveragePercent(percentFrames);
            Assert.AreEqual(0.88f, averagePercent, delta);
        }
    }
}
