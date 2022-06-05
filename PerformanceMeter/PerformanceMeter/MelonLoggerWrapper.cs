using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeter
{
    public class MelonLoggerWrapper : ILogger
    {
        private readonly MelonLogger.Instance melonLogger;

        public MelonLoggerWrapper(MelonLogger.Instance melonLogger)
        {
            this.melonLogger = melonLogger;
        }

        public void Msg(string message)
        {
            melonLogger.Msg(message);
        }

        public void Error(string message, Exception ex)
        {
            melonLogger.Error(message, ex);
        }
    }
}
