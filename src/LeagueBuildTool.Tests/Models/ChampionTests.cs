using Xunit;
using LeagueBuildTool.Core;

namespace LeagueBuildTool.Tests.Models
{
    public class ChampionTests
    {
        [Fact]
        public void Champion_DefaultConstructor_InitializesCorrectly()
        {
            // Arrange & Act
            var champion = new Champion();

            // Assert
            Assert.Empty(champion.Name);
            Assert.Empty(champion.Lane);
            Assert.Equal(0, champion.Health);
            Assert.Equal(0, champion.AttackDamage);
            Assert.Equal(0, champion.AbilityPower);
            Assert.Equal(0, champion.Gold);
            Assert.Empty(champion.Build);
        }

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