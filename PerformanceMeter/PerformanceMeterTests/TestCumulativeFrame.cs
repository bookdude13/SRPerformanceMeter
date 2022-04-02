using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceMeter;
using PerformanceMeter.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeterTests
{
    [TestClass]
    public class TestCumulativeFrame
    {
        private readonly float delta = 0.0001f;

        [TestMethod]
        public void TestConvertToPercentFrame()
        {
            var cumulativeFrames = new List<CumulativeFrame>()
            {
                new CumulativeFrame(0.0f, 0.0f),
                new CumulativeFrame(1.0f, 1.5f),
                new CumulativeFrame(2.0f, 2.0f),
                new CumulativeFrame(3.5f, 6.0f)
            };

            var percentFrames = cumulativeFrames.Select(cumulative => cumulative.ToPercentFrame(6.0f));
            Assert.AreEqual(0.0f, percentFrames.ElementAt(0).timeMs);
            Assert.AreEqual(0.0f, percentFrames.ElementAt(0).percentOfTotal, delta);

            Assert.AreEqual(1.0f, percentFrames.ElementAt(1).timeMs);
            Assert.AreEqual(0.25f, percentFrames.ElementAt(1).percentOfTotal, delta);

            Assert.AreEqual(2.0f, percentFrames.ElementAt(2).timeMs);
            Assert.AreEqual(0.3333f, percentFrames.ElementAt(2).percentOfTotal, delta);

            Assert.AreEqual(3.5f, percentFrames.ElementAt(3).timeMs);
            Assert.AreEqual(1.0f, percentFrames.ElementAt(3).percentOfTotal, delta);
        }
    }
}
