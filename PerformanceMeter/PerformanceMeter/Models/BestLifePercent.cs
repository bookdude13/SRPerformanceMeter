﻿using PerformanceMeter.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Models
{
    public class BestLifePercent
    {
        public long EpochTimeSet { get; set; }
        public float AverageLifePercent { get; set; }
        public List<PercentFrame> LifePercentFrames { get; set; }

        public BestLifePercent()
        {
        }

        public BestLifePercent(float averageLifePercent, List<PercentFrame> lifePercentFrames)
        {
            this.EpochTimeSet = DateTimeOffset.Now.ToUnixTimeSeconds();
            this.AverageLifePercent = averageLifePercent;
            this.LifePercentFrames = lifePercentFrames;
        }
    }
}
