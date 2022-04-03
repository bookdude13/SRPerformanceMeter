using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Models
{
    public class PlayConfiguration
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string MapHash { get; set; }
        public string Difficulty { get; set; }
        public string GameMode { get; set; }
        public string[] Modifiers { get; set; }
    }
}
