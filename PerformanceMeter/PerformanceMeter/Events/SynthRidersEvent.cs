using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;

namespace PerformanceMeter.Messages
{
    public class SynthRidersEvent<T>
    {
        public string eventType;
        public T data;
    }
}
