using LiteDB;
using PerformanceMeter.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Models
{
    public class LocalScore
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string MapHash { get; set; }
        public string Difficulty { get; set; }
        public string Modifiers { get; set; }
        public List<CumulativeFrame> TotalScoreFrames { get; set; }
        public long EpochTimeSet { get; set; }

        public LocalScore()
        {
        }

        public LocalScore(string username, string mapHash, string difficulty, string modifiers, List<CumulativeFrame> totalScoreFrames)
        {
            this.Id = GetIdentifier(username, mapHash, difficulty, modifiers);
            this.Username = username;
            this.MapHash = mapHash;
            this.Difficulty = difficulty;
            this.Modifiers = modifiers;
            this.TotalScoreFrames = totalScoreFrames;
            this.EpochTimeSet = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public static string GetIdentifier(string username, string mapHash, string difficulty, string modifiers)
        {
            return username + "_" + mapHash + "_" + difficulty + "_" + modifiers;
        }
    }
}
