using MelonLoader;
using PerformanceMeter;
using SRModCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeterTests
{
    public class LoggerForTest : SRLogger
    {
        public void Msg(string message)
        {
            Console.WriteLine(message);
        }

        public void Error(string message)
        {
            Console.Error.WriteLine(message);
        }

        public void Error(string message, Exception e)
        {
            Console.Error.WriteLine(message + ": " + e.Message);
        }
    }
}
