using System.Linq;
using System.Threading.Tasks;
using Xunit;
using LeagueBuildTool.Core;
using LeagueBuildTool.Core;

namespace LeagueBuildTool.Tests.Services
{
    public class RiotItemFetcherTests
    {
        [Fact(DisplayName = "Fetch all items from Riot and verify list is not empty")]
        public async Task GetAllItemsAsync_ShouldReturnItems()
        {
            // Act: fetch items from Riot Data Dragon
            var items = await RiotItemFetcher.GetAllItemsAsync();

            // Assert: list should not be null
            Assert.NotNull(items);

            // Assert: list should have at least one item
            Assert.True(items.Count > 0, "No items were fetched from Riot");

            // Assert: known item exists in list
            Assert.Contains(items, i => i.Name == "Infinity Edge");
        }

        [Fact(DisplayName = "Check fetched items have valid properties")]
        public async Task Items_ShouldHaveValidProperties()
        {
            var items = await RiotItemFetcher.GetAllItemsAsync();

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