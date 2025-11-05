using Xunit;
using LeagueBuildTool.Core;

namespace LeagueBuildTool.Tests.Models
{
    public class ItemTests
    {
        [Fact]
        public void Item_Constructor_InitializesCorrectly()
        {
            // Arrange
            string name = "Test Item";
            int cost = 1000;
            string description = "Test Description";

            // Act
            var item = new Item(name, cost, description);

            // Assert
            Assert.Equal(name, item.Name);
            Assert.Equal(cost, item.Cost);
            Assert.Equal(description, item.Description);
        }

        [Fact]
        public void Item_DefaultConstructor_InitializesWithDefaults()
        {
            // Arrange & Act
            var item = new Item();

            // Assert
            Assert.Empty(item.Name);
            Assert.Equal(0, item.Cost);
            Assert.Empty(item.Description);
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var item = new Item("Test Item", 1000, "Test Description");
            var expected = "Test Item (Cost: 1000 gold) - Test Description";

            // Act
            var result = item.ToString();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}