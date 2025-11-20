using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;
using LeagueBuildTool.Core;
using LeagueBuildTool.Core.Configuration;

namespace LeagueBuildTool.Tests.Services
{
    /// <summary>
    /// Contains integration tests for the RiotItemFetcher service, verifying
    /// its ability to fetch and parse item data from the Riot Games API.
    /// </summary>
    public class RiotItemFetcherTests
    {
        private readonly RiotItemFetcher _fetcher;

        public RiotItemFetcherTests()
        {
            var config = new RiotApiConfiguration();
            var httpClient = new HttpClient();
            _fetcher = new RiotItemFetcher(config, httpClient);
        }
        /// <summary>
        /// Verifies that the GetAllItemsAsync method successfully fetches items from
        /// the Riot Data Dragon API and that the returned list contains expected items.
        /// Tests for non-null response, non-empty list, and presence of a known item.
        /// </summary>
        /// <returns>A Task representing the asynchronous test operation.</returns>
        [Fact(DisplayName = "Fetch all items from Riot and verify list is not empty")]
        public async Task GetAllItemsAsync_ShouldReturnItems()
        {
            // Act: fetch items from Riot Data Dragon
            var items = await _fetcher.GetAllItemsAsync();

            // Assert: list should not be null
            Assert.NotNull(items);

            // Assert: list should have at least one item
            Assert.True(items.Count > 0, "No items were fetched from Riot");

            // Assert: known item exists in list
            Assert.Contains(items, i => i.Name == "Infinity Edge");
        }

        /// <summary>
        /// Verifies that the items fetched from the Riot API have valid properties.
        /// Checks the first 5 items to ensure they have non-empty names and descriptions,
        /// and non-negative costs.
        /// </summary>
        /// <returns>A Task representing the asynchronous test operation.</returns>
        [Fact(DisplayName = "Check fetched items have valid properties")]
        public async Task Items_ShouldHaveValidProperties()
        {
            var items = await _fetcher.GetAllItemsAsync();

            // Check first 5 items for valid properties
            foreach (var item in items.Take(5))
            {
                Assert.False(string.IsNullOrWhiteSpace(item.Name), "Item Name is missing");
                Assert.True(item.Cost >= 0, $"Item {item.Name} has invalid cost");
                Assert.False(string.IsNullOrWhiteSpace(item.Description), $"Item {item.Name} is missing description");
            }
        }
    }
}