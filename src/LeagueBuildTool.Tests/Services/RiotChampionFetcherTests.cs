using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;
using LeagueBuildTool.Core;

namespace LeagueBuildTool.Tests.Services
{
    /// <summary>
    /// Integration tests for the RiotChampionFetcher service.
    /// These tests will perform network calls to Riot's Data Dragon and may fail when offline.
    /// </summary>
    public class RiotChampionFetcherTests
    {
        [Fact(DisplayName = "Fetch all champions from Riot and verify list is not empty")]
        public async Task GetAllChampionsAsync_ShouldReturnChampions()
        {
            var champions = await RiotChampionFetcher.GetAllChampionsAsync();

            Assert.NotNull(champions);
            Assert.True(champions.Count > 0, "No champions were fetched from Riot");

            // pick a sample and ensure base stats and tags are present
            var sample = champions.First();
            foreach (var key in Champion.RequiredBaseStatKeys)
            {
                Assert.True(sample.BaseStats.ContainsKey(key));
            }
            Assert.NotNull(sample.Tags);
        }
    }
}
