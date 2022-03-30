using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceMeter.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PerformanceMeterTests
{
    [TestClass]
    public class TestMessageDeserialization
    {
        [TestMethod]
        public void TestDeserializeTopLevel()
        {
            string message = @"
            {
                'eventType': 'EventType',
                'data': {}
            }";

            var parsedEvent = JsonConvert.DeserializeObject<SynthRidersEvent<object>>(message);

            Assert.AreEqual("EventType", parsedEvent.eventType);
            Assert.IsNotNull(parsedEvent.data);
        }

        [TestMethod]
        public void TestDeserializeSongStart()
        {
            string message = @"
            {
                'eventType': 'SongStart',
                'data': {
                    'song': 'ANIMA',
                    'difficulty': 'Master',
                    'author': 'bookdude13',
                    'beatMapper': 'KK964',
                    'length': 269.19,
                    'bpm': 192.7,
                    'albumArt': ''
                }
            }";

            var parsedEvent = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataSongStart>>(message);

            Assert.AreEqual("SongStart", parsedEvent.eventType);
            Assert.IsNotNull(parsedEvent.data);
            Assert.AreEqual("ANIMA", parsedEvent.data.song);
            Assert.AreEqual("Master", parsedEvent.data.difficulty);
            Assert.AreEqual("bookdude13", parsedEvent.data.author);
            Assert.AreEqual("KK964", parsedEvent.data.beatMapper);
            Assert.AreEqual(269.19, parsedEvent.data.length, 0.00001);
            Assert.AreEqual(192.7, parsedEvent.data.bpm, 0.00001);
            Assert.AreEqual("", parsedEvent.data.albumArt);
        }
    }
}
