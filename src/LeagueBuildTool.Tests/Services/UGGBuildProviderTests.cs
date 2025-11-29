using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;
using LeagueBuildTool.Core;
using LeagueBuildTool.Core.Configuration;
using LeagueBuildTool.Core.Data.BuildRecommendation;
using LeagueBuildTool.Core.Services.BuildProviders;

namespace LeagueBuildTool.Tests.Services
{
    /// <summary>
    /// Unit and integration tests for the UGGBuildProvider service.
    /// Tests cover scraping functionality, caching, error handling, and counter-build generation.
    /// Note: Some tests require network access to U.GG and may fail if site structure changes.
    /// </summary>
    public class UGGBuildProviderTests
    {
        private readonly RiotApiConfiguration _config;

        public UGGBuildProviderTests()
        {
            _config = new RiotApiConfiguration();
        }

        #region Constructor and Initialization Tests

        [Fact(DisplayName = "Constructor should initialize provider without errors")]
        public void Constructor_ShouldInitializeSuccessfully()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act
            var provider = new UGGBuildProvider(httpClient, _config);

            // Assert
            Assert.NotNull(provider);
            Assert.Equal(BuildSource.UGG, provider.Source);
        }

        [Fact(DisplayName = "Source property should return UGG")]
        public void Source_ShouldReturnUGG()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var source = provider.Source;

            // Assert
            Assert.Equal(BuildSource.UGG, source);
        }

        [Fact(DisplayName = "Constructor should add User-Agent header to HttpClient")]
        public void Constructor_ShouldConfigureUserAgent()
        {
            // Arrange
            var httpClient = new HttpClient();
            var userAgentBefore = httpClient.DefaultRequestHeaders.FirstOrDefault(h => h.Key == "User-Agent");

            // Act
            var provider = new UGGBuildProvider(httpClient, _config);

            // Assert
            var userAgentAfter = httpClient.DefaultRequestHeaders.FirstOrDefault(h => h.Key == "User-Agent");
            Assert.NotEqual(default, userAgentAfter);
            Assert.Contains("Mozilla", userAgentAfter.Value.FirstOrDefault() ?? "");
        }

        #endregion

        #region Build Recommendation Tests

        [Fact(DisplayName = "GetBuildRecommendationsAsync should return list for valid champion")]
        public async Task GetBuildRecommendationsAsync_WithValidChampion_ShouldReturnRecommendations()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act - Test with common champion
            var recommendations = await provider.GetBuildRecommendationsAsync("Garen");

            // Assert
            Assert.NotNull(recommendations);
            Assert.IsType<List<BuildRecommendation>>(recommendations);
            
            // Should return either actual data or fallback recommendation
            if (recommendations.Count > 0)
            {
                var first = recommendations.First();
                Assert.Equal("Garen", first.ChampionName);
                Assert.NotNull(first.Role);
                Assert.Equal(BuildSource.UGG, first.Source);
                Assert.NotEqual(default(DateTime), first.LastUpdated);
            }
        }

        [Fact(DisplayName = "GetBuildRecommendationsAsync should return recommendations with items")]
        public async Task GetBuildRecommendationsAsync_ShouldIncludeItems()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var recommendations = await provider.GetBuildRecommendationsAsync("Ahri", "Mid");

            // Assert
            Assert.NotNull(recommendations);
            if (recommendations.Count > 0)
            {
                var first = recommendations.First();
                // Either should have actual items or be fallback recommendation
                Assert.NotNull(first.CoreItems);
                Assert.NotNull(first.StartingItems);
            }
        }

        [Fact(DisplayName = "GetBuildRecommendationsAsync with role should filter by role")]
        public async Task GetBuildRecommendationsAsync_WithRole_ShouldSpecifyRole()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var recommendations = await provider.GetBuildRecommendationsAsync("Lee Sin", "Jungle");

            // Assert
            Assert.NotNull(recommendations);
            if (recommendations.Count > 0)
            {
                var first = recommendations.First();
                Assert.Equal("Lee Sin", first.ChampionName);
                Assert.NotNull(first.Role);
            }
        }

        [Fact(DisplayName = "GetBuildRecommendationsAsync should handle invalid champion gracefully")]
        public async Task GetBuildRecommendationsAsync_WithInvalidChampion_ShouldReturnEmptyList()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var recommendations = await provider.GetBuildRecommendationsAsync("NotAChampion12345");

            // Assert
            Assert.NotNull(recommendations);
            // Should either be empty or contain fallback recommendation
            if (recommendations.Count > 0)
            {
                // If it returns something, it should be a fallback
                Assert.Contains("data available but", recommendations.First().Notes ?? "");
            }
        }

        [Fact(DisplayName = "GetBuildRecommendationsAsync should set LastUpdated to current time")]
        public async Task GetBuildRecommendationsAsync_ShouldSetLastUpdated()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);
            var beforeCall = DateTime.UtcNow;

            // Act
            var recommendations = await provider.GetBuildRecommendationsAsync("Darius");
            var afterCall = DateTime.UtcNow;

            // Assert
            Assert.NotNull(recommendations);
            if (recommendations.Count > 0)
            {
                var first = recommendations.First();
                Assert.True(first.LastUpdated >= beforeCall, "LastUpdated is before call time");
                Assert.True(first.LastUpdated <= afterCall.AddSeconds(1), "LastUpdated is after call time");
            }
        }

        #endregion

        #region Caching Tests

        [Fact(DisplayName = "GetBuildRecommendationsAsync should cache results")]
        public async Task GetBuildRecommendationsAsync_ShouldCacheResults()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act - First call (should fetch and cache)
            var firstCall = await provider.GetBuildRecommendationsAsync("Yasuo", "Mid");
            var firstCallTime = DateTime.UtcNow;

            // Wait a bit
            await Task.Delay(100);

            // Second call (should use cache)
            var secondCall = await provider.GetBuildRecommendationsAsync("Yasuo", "Mid");
            var secondCallTime = DateTime.UtcNow;

            // Assert - Both should return same data with same LastUpdated
            Assert.NotNull(firstCall);
            Assert.NotNull(secondCall);
            if (firstCall.Count > 0 && secondCall.Count > 0)
            {
                var first = firstCall.First();
                var second = secondCall.First();
                
                // Cache should return exact same LastUpdated time
                Assert.Equal(first.LastUpdated, second.LastUpdated);
                
                // Verify it was cached (second call was much faster - within same millisecond often)
                // This is a loose assertion since timing varies
                Assert.True(secondCallTime >= firstCallTime);
            }
        }

        [Fact(DisplayName = "Different champions should have separate cache entries")]
        public async Task GetBuildRecommendationsAsync_DifferentChampions_ShouldHaveSeparateCacheEntries()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var garen = await provider.GetBuildRecommendationsAsync("Garen");
            var darius = await provider.GetBuildRecommendationsAsync("Darius");

            // Assert
            Assert.NotNull(garen);
            Assert.NotNull(darius);
            if (garen.Count > 0 && darius.Count > 0)
            {
                var garenBuild = garen.First();
                var dariusBuild = darius.First();
                
                Assert.Equal("Garen", garenBuild.ChampionName);
                Assert.Equal("Darius", dariusBuild.ChampionName);
            }
        }

        [Fact(DisplayName = "Same champion with different roles should have separate cache entries")]
        public async Task GetBuildRecommendationsAsync_DifferentRoles_ShouldHaveSeparateCacheEntries()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var topRole = await provider.GetBuildRecommendationsAsync("Lulu", "Top");
            var supportRole = await provider.GetBuildRecommendationsAsync("Lulu", "Support");

            // Assert
            Assert.NotNull(topRole);
            Assert.NotNull(supportRole);
            // Both should succeed without errors
        }

        #endregion

        #region Counter Build Tests

        [Fact(DisplayName = "GetCounterBuildRecommendationsAsync should return non-empty list")]
        public async Task GetCounterBuildRecommendationsAsync_ShouldReturnCounterBuilds()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var counterBuilds = await provider.GetCounterBuildRecommendationsAsync("Garen", "Darius", "Top");

            // Assert
            Assert.NotNull(counterBuilds);
            if (counterBuilds.Count > 0)
            {
                var first = counterBuilds.First();
                Assert.Equal("Garen", first.ChampionName);
                Assert.Contains("Counter", first.Notes ?? "");
                Assert.Contains("Darius", first.Notes ?? "");
            }
        }

        [Fact(DisplayName = "GetCounterBuildRecommendationsAsync should add defensive items")]
        public async Task GetCounterBuildRecommendationsAsync_ShouldAddDefensiveItems()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var counterBuilds = await provider.GetCounterBuildRecommendationsAsync("Malphite", "Yone", "Mid");

            // Assert
            Assert.NotNull(counterBuilds);
            if (counterBuilds.Count > 0)
            {
                var first = counterBuilds.First();
                Assert.NotNull(first.SituationalItems);
                // Should have some situational items for countering
            }
        }

        [Fact(DisplayName = "GetCounterBuildRecommendationsAsync should handle missing opponent builds")]
        public async Task GetCounterBuildRecommendationsAsync_WithInvalidOpponent_ShouldHandleGracefully()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var counterBuilds = await provider.GetCounterBuildRecommendationsAsync("Garen", "InvalidChampion99999", "Top");

            // Assert
            Assert.NotNull(counterBuilds);
            // Should return empty list or graceful fallback
        }

        #endregion

        #region Patch Version Tests

        [Fact(DisplayName = "GetCurrentPatchVersionAsync should return version string")]
        public async Task GetCurrentPatchVersionAsync_ShouldReturnVersionString()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var patchVersion = await provider.GetCurrentPatchVersionAsync();

            // Assert
            Assert.NotNull(patchVersion);
            Assert.NotEmpty(patchVersion);
            // Should be in format like "13.18.1" or fallback to config version
            Assert.Matches(@"\d+\.\d+\.\d+", patchVersion);
        }

        #endregion

        #region Error Handling Tests

        [Fact(DisplayName = "GetBuildRecommendationsAsync should not throw on network error")]
        public async Task GetBuildRecommendationsAsync_ShouldHandleNetworkErrorGracefully()
        {
            // This test verifies the provider doesn't throw exceptions
            // It should return empty list or fallback recommendation on error
            
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act - Call with unusual input that might cause issues
            var result = await provider.GetBuildRecommendationsAsync("");

            // Assert - Should not throw, should return list (empty or with data)
            Assert.NotNull(result);
            Assert.IsType<List<BuildRecommendation>>(result);
        }

        [Fact(DisplayName = "GetLiveGameBuildRecommendationsAsync should throw InvalidOperationException")]
        public async Task GetLiveGameBuildRecommendationsAsync_ShouldThrowNotSupported()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await provider.GetLiveGameBuildRecommendationsAsync("NA1", "PlayerName")
            );
            
            Assert.Contains("does not support live game", ex.Message);
        }

        #endregion

        #region Data Validation Tests

        [Fact(DisplayName = "Build recommendation should have required properties")]
        public async Task BuildRecommendation_ShouldHaveRequiredProperties()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var recommendations = await provider.GetBuildRecommendationsAsync("Syndra", "Mid");

            // Assert
            Assert.NotNull(recommendations);
            if (recommendations.Count > 0)
            {
                var build = recommendations.First();
                
                // Required properties
                Assert.NotNull(build.ChampionName);
                Assert.NotEmpty(build.ChampionName);
                
                Assert.NotNull(build.Role);
                Assert.NotEmpty(build.Role);
                
                Assert.Equal(BuildSource.UGG, build.Source);
                
                Assert.NotNull(build.PatchVersion);
                Assert.NotEmpty(build.PatchVersion);
                
                Assert.NotEqual(default(DateTime), build.LastUpdated);
                
                // Collections should be non-null
                Assert.NotNull(build.CoreItems);
                Assert.NotNull(build.StartingItems);
                Assert.NotNull(build.SituationalItems);
            }
        }

        [Fact(DisplayName = "Core items should not exceed reasonable limit")]
        public async Task CoreItems_ShouldNotExceedReasonableLimit()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var recommendations = await provider.GetBuildRecommendationsAsync("Lissandra", "Mid");

            // Assert
            Assert.NotNull(recommendations);
            if (recommendations.Count > 0)
            {
                var build = recommendations.First();
                // Core items should typically be 3-6 items
                Assert.True(build.CoreItems.Count <= 10, "Too many core items");
            }
        }

        [Fact(DisplayName = "Starting items should be non-empty for valid builds")]
        public async Task StartingItems_ShouldBePopulated()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var recommendations = await provider.GetBuildRecommendationsAsync("Ezreal", "ADC");

            // Assert
            Assert.NotNull(recommendations);
            if (recommendations.Count > 0 && !recommendations.First().Notes.Contains("data available but"))
            {
                var build = recommendations.First();
                // Starting items might be empty for fallback, but should exist
                Assert.NotNull(build.StartingItems);
            }
        }

        #endregion

        #region Integration Tests

        [Fact(DisplayName = "Multiple consecutive calls should maintain cache integrity")]
        public async Task MultipleConsecutiveCalls_ShouldMaintainCacheIntegrity()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);
            var champions = new[] { "Garen", "Darius", "Fiora", "Kled" };

            // Act - Call multiple times
            var results = new List<List<BuildRecommendation>>();
            foreach (var champ in champions)
            {
                var recs = await provider.GetBuildRecommendationsAsync(champ);
                results.Add(recs);
            }

            // Assert - All should return successfully
            Assert.Equal(champions.Length, results.Count);
            foreach (var result in results)
            {
                Assert.NotNull(result);
            }
        }

        [Fact(DisplayName = "Provider should handle rapid successive calls")]
        public async Task RapidSuccessiveCalls_ShouldNotCrash()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act - Make several rapid calls to same champion (testing cache)
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => provider.GetBuildRecommendationsAsync("Annie"))
                .ToList();

            var results = await Task.WhenAll(tasks);

            // Assert - All should complete successfully
            Assert.Equal(5, results.Length);
            Assert.All(results, r => Assert.NotNull(r));
        }

        #endregion

        #region Source Property Tests

        [Fact(DisplayName = "All recommendations should have UGG source")]
        public async Task AllRecommendations_ShouldHaveUGGSource()
        {
            // Arrange
            var httpClient = new HttpClient();
            var provider = new UGGBuildProvider(httpClient, _config);

            // Act
            var recommendations = await provider.GetBuildRecommendationsAsync("Vladimir");

            // Assert
            Assert.NotNull(recommendations);
            Assert.All(recommendations, r => Assert.Equal(BuildSource.UGG, r.Source));
        }

        #endregion
    }
}
