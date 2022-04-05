using MelonLoader;
using PerformanceMeter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeterTests
{
    public class LoggerForTest : ILogger
    {
        public void Msg(string message)
        {
            Console.WriteLine(message);
        }
    }
}
