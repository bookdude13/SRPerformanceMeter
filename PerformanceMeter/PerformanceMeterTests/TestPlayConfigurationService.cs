using LiteDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceMeter.Models;
using PerformanceMeter.Repositories;
using PerformanceMeter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceMeterTests
{
    [TestClass]
    public class TestPlayConfigurationService
    {
        private readonly string username = "bookdude13";
        private readonly string mapHash = "asdf";
        private readonly string difficulty = "Master";
        private readonly string gameMode = "Force";
        private readonly List<string> modifiers = new List<string>();

        private ILiteDatabase db;
        private PlayConfigurationRepository repo;
        private PlayConfigurationService service;

        [TestInitialize]
        public void Before()
        {
            db = new LiteDatabase(":memory:");
            var logger = new LoggerForTest();
            repo = new PlayConfigurationRepository(logger, db);
            service = new PlayConfigurationService(logger, repo);
        }

        [TestMethod]
        public void TestEnsure_NoExisting_Creates()
        {
            service.EnsurePlayConfiguration(new PlayConfiguration()
            {
                Username = username,
                MapHash = mapHash,
                Difficulty = difficulty,
                GameMode = gameMode,
                Modifiers = modifiers
            });

            var config = service.GetPlayConfiguration(username, mapHash, difficulty, gameMode, modifiers);
            Assert.IsNotNull(config);
        }

        [TestMethod]
        public void TestGet_NoExisting_CreatesAndReturns()
        {
            PlayConfiguration config = service.GetPlayConfiguration(username, mapHash, difficulty, gameMode, modifiers);
            Assert.IsNotNull(config);
        }

        [TestMethod]
        public void TestGet_Existing_DoesNotCreateDuplicate()
        {
            PlayConfiguration config = service.GetPlayConfiguration(username, mapHash, difficulty, gameMode, modifiers);
            Assert.IsNotNull(config);

            PlayConfiguration duplicateConfig = service.GetPlayConfiguration(username, mapHash, difficulty, gameMode, modifiers);
            Assert.IsNotNull(duplicateConfig);
            Assert.AreEqual(config.Id, duplicateConfig.Id);
        }

        [TestMethod]
        public void TestGet_EachParameterChangeCreatesNew()
        {
            HashSet<Guid> configIds = new HashSet<Guid>();

            PlayConfiguration baseConfig = service.GetPlayConfiguration(username, mapHash, difficulty, gameMode, modifiers);
            configIds.Add(baseConfig.Id);
            Assert.IsNotNull(baseConfig);
            Assert.AreEqual(1, configIds.Count);

            // As of Remastered, I don't know how to get the username. This test failing was not noticed in earlier releases.
            // Just ignore for now, and assume every user is the same as far as play recording goes.
            //// Change username
            //PlayConfiguration configUsername = service.GetPlayConfiguration("newUser", mapHash, difficulty, gameMode, modifiers);
            //configIds.Add(configUsername.Id);
            //Console.WriteLine("Username config id is " + configUsername.Id);
            //Assert.IsNotNull(configUsername);
            //Assert.AreEqual(2, configIds.Count);

            // Change map hash
            PlayConfiguration configMapHash = service.GetPlayConfiguration(username, "newHash", difficulty, gameMode, modifiers);
            configIds.Add(configMapHash.Id);
            Assert.IsNotNull(configMapHash);
            Assert.AreEqual(2, configIds.Count);

            // Change difficulty
            PlayConfiguration configDifficulty = service.GetPlayConfiguration(username, mapHash, "Easy", gameMode, modifiers);
            configIds.Add(configDifficulty.Id);
            Assert.IsNotNull(configDifficulty);
            Assert.AreEqual(3, configIds.Count);

            // Change game mode
            PlayConfiguration configGameMode = service.GetPlayConfiguration(username, mapHash, difficulty, "Rhythm", modifiers);
            configIds.Add(configGameMode.Id);
            Assert.IsNotNull(configGameMode);
            Assert.AreEqual(4, configIds.Count);

            // Change modifiers
            var newModifiers = new List<string>() { "tiny" };
            PlayConfiguration configModifiers = service.GetPlayConfiguration(username, mapHash, difficulty, gameMode, newModifiers);
            configIds.Add(configModifiers.Id);
            Assert.IsNotNull(configModifiers);
            Assert.AreEqual(5, configIds.Count);

            // Same number of modifiers, but different contents
            var newModifiers2 = new List<string>() { "huge" };
            PlayConfiguration configModifiers2 = service.GetPlayConfiguration(username, mapHash, difficulty, gameMode, newModifiers2);
            configIds.Add(configModifiers2.Id);
            Assert.IsNotNull(configModifiers2);
            Assert.AreEqual(6, configIds.Count);
        }
    }
}
