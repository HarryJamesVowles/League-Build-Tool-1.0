using Xunit;
using LeagueBuildTool.Core;

namespace LeagueBuildTool.Tests.Models
{
    /// <summary>
    /// Contains unit tests for the Item class, verifying its construction,
    /// property initialization, and string representation functionality.
    /// </summary>
    public class ItemTests
    {
        /// <summary>
        /// Verifies that the Item constructor correctly initializes all properties
        /// when provided with valid parameters.
        /// </summary>
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

        /// <summary>
        /// Verifies that the Item default constructor correctly initializes
        /// all properties to their default values.
        /// </summary>
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

        /// <summary>
        /// Verifies that the ToString method returns a properly formatted string
        /// containing the item's name, cost, and description.
        /// </summary>
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