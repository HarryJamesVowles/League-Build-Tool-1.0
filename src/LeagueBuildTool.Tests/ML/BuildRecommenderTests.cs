using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeagueBuildTool.Core.ML;
using LeagueBuildTool.Core.Services;
using LeagueBuildTool.Core.Configuration;
using System.Threading.Tasks;

namespace LeagueBuildTool.Tests.ML
{
    [TestClass]
    public class BuildRecommenderTests
    {
        private static readonly ApiConfiguration _testConfig = new()
        {
            RiotApiKey = "RGAPI-7b5017c1-cdff-4745-9151-3de1c4054a92",
            Region = "na1",
            GameVersion = "13.18.1"
        };

        [TestMethod]
        public async Task TrainModelOnHighEloData_ShouldCreateValidModel()
        {
            // Arrange
            var collector = new HighEloMatchCollector(_testConfig);
            var recommender = new BuildRecommender("test_model.zip");

            // Act
            var trainingData = await collector.CollectHighEloMatchesAsync(100); // Start with 100 matches for testing
            await recommender.TrainModelAsync(trainingData);

            // Assert
            var testFeatures = new BuildPredictionFeatures
            {
                ChampionName = "Ahri",
                Role = "Mid",
                CurrentGold = 3000,
                GameTime = 15
            };

            var prediction = recommender.PredictBuildSuccess(testFeatures);
            Assert.IsTrue(prediction >= 0 && prediction <= 1, "Prediction should be a probability between 0 and 1");
        }

        [TestMethod]
        public async Task CollectHighEloMatches_ShouldReturnValidData()
        {
            // Arrange
            var collector = new HighEloMatchCollector(_testConfig);

            // Act
            var matches = await collector.CollectHighEloMatchesAsync(10);

            // Assert
            Assert.IsNotNull(matches);
            Assert.IsTrue(matches.Count > 0, "Should collect at least one match");
            Assert.IsTrue(matches.All(m => !string.IsNullOrEmpty(m.MatchId)), "All matches should have an ID");
            Assert.IsTrue(matches.All(m => !string.IsNullOrEmpty(m.ChampionName)), "All matches should have a champion name");
        }

        [TestMethod]
        public void BuildRecommender_ShouldHandleEdgeCases()
        {
            // Arrange
            var recommender = new BuildRecommender();

            // Act & Assert
            var features = new BuildPredictionFeatures
            {
                ChampionName = "",  // Empty champion name
                Role = "Invalid",   // Invalid role
                CurrentGold = -1,   // Invalid gold
                GameTime = 0        // Game just started
            };

            Assert.ThrowsException<InvalidOperationException>(() => recommender.PredictBuildSuccess(features),
                "Should throw when model is not trained");
        }
    }
}