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

        [TestMethod]
        public void TestDeserializeSongEnd()
        {
            string message = @"
            {
                'eventType': 'SongEnd',
                'data': {
                    'song': 'ANIMA',
                    'perfect': 12,
                    'normal': 3,
                    'bad': 4,
                    'fail': 1,
                    'highestCombo': 11
                }
            }";

            var parsedEvent = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataSongEnd>>(message);

            Assert.AreEqual("SongEnd", parsedEvent.eventType);
            Assert.IsNotNull(parsedEvent.data);
            Assert.AreEqual("ANIMA", parsedEvent.data.song);
            Assert.AreEqual(12, parsedEvent.data.perfect);
            Assert.AreEqual(3, parsedEvent.data.normal);
            Assert.AreEqual(4, parsedEvent.data.bad);
            Assert.AreEqual(1, parsedEvent.data.fail);
            Assert.AreEqual(11, parsedEvent.data.highestCombo);
        }

        [TestMethod]
        public void TestDeserializePlayTime()
        {
            string message = @"
            {
                'eventType': 'PlayTime',
                'data': {
                    'playTimeMS': 12345.67
                }
            }";

            var parsedEvent = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataPlayTime>>(message);

            Assert.AreEqual("PlayTime", parsedEvent.eventType);
            Assert.IsNotNull(parsedEvent.data);
            Assert.AreEqual(12345.67, parsedEvent.data.playTimeMS, 0.0001);
        }

        [TestMethod]
        public void TestDeserializeNoteHit()
        {
            string message = @"
            {
                'eventType': 'NoteHit',
                'data': {
                    'score': 12300,
                    'combo': 312,
                    'multiplier': 3,
                    'completed': 1.0,
                    'lifeBarPercent': 0.83333
                }
            }";

            var parsedEvent = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataNoteHit>>(message);

            Assert.AreEqual("NoteHit", parsedEvent.eventType);
            Assert.IsNotNull(parsedEvent.data);
            Assert.AreEqual(12300, parsedEvent.data.score);
            Assert.AreEqual(312, parsedEvent.data.combo);
            Assert.AreEqual(3, parsedEvent.data.multiplier);
            Assert.AreEqual(1.0, parsedEvent.data.completed, 0.00001);
            Assert.AreEqual(0.83333, parsedEvent.data.lifeBarPercent, 0.00001);
        }

        [TestMethod]
        public void TestDeserializeNoteMiss()
        {
            string message = @"
            {
                'eventType': 'NoteMiss',
                'data': {
                    'multiplier': 3,
                    'lifeBarPercent': 0.83333
                }
            }";

            var parsedEvent = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataNoteMiss>>(message);

            Assert.AreEqual("NoteMiss", parsedEvent.eventType);
            Assert.IsNotNull(parsedEvent.data);
            Assert.AreEqual(3, parsedEvent.data.multiplier);
            Assert.AreEqual(0.83333, parsedEvent.data.lifeBarPercent, 0.00001);
        }

        [TestMethod]
        public void TestDeserializeSceneChange()
        {
            string message = @"
            {
                'eventType': 'SceneChange',
                'data': {
                    'sceneName': '3.GameEnd'
                }
            }";

            var parsedEvent = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataSceneChange>>(message);

            Assert.AreEqual("SceneChange", parsedEvent.eventType);
            Assert.IsNotNull(parsedEvent.data);
            Assert.AreEqual("3.GameEnd", parsedEvent.data.sceneName);
        }
    }
}
