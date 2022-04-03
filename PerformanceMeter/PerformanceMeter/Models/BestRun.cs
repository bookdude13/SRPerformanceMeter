using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter.Models
{
    public class BestRun
    {
        [BsonId]
        public Guid PlayConfigurationId { get; set; }
        public BestTotalScore TotalScore { get; set; }
    }
}
