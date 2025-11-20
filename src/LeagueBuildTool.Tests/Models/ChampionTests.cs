using Xunit;
using Assert = Xunit.Assert;
using LeagueBuildTool.Core;

namespace LeagueBuildTool.Tests.Models
{
    /// <summary>
    /// Contains unit tests for the Champion class, verifying its initialization
    /// and item management functionality.
    /// </summary>
    public class ChampionTests
    {
        /// <summary>
        /// Verifies that the Champion default constructor correctly initializes
        /// all properties to their default values.
        /// </summary>
        [Fact]
        public void Champion_DefaultConstructor_InitializesCorrectly()
        {
            // Arrange & Act
            var champion = new Champion();

            // Assert
            Assert.Empty(champion.Name);
            Assert.Empty(champion.Lane);
            // Ensure BaseStats contains the required keys and defaults to 0
            foreach (var key in Champion.RequiredBaseStatKeys)
            {
                Assert.True(champion.BaseStats.ContainsKey(key), $"Missing base stat key: {key}");
                Assert.Equal(0.0, champion.BaseStats[key]);
            }
            Assert.Empty(champion.Tags);
            Assert.Equal(0, champion.Gold);
            Assert.Empty(champion.Build);
        }

        /// <summary>
        /// Verifies that the AddItemToBuild method correctly adds a new item
        /// to the champion's build list.
        /// </summary>
        [Fact]
        public void AddItemToBuild_AddsItemCorrectly()
        {
            // Arrange
            var champion = new Champion();
            var item = new Item("Test Item", 1000, "Test Description");

            // Act
            champion.AddItemToBuild(item);

            // Assert
            Assert.Single(champion.Build);
            Assert.Equal(item, champion.Build[0]);
        }
    }
}