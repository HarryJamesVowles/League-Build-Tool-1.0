using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;
using Xunit.Abstractions;
using LeagueBuildTool.Core;
using LeagueBuildTool.Core.Configuration;

namespace LeagueBuildTool.Tests.Services
{
    public class ItemStatsAnalyzerTests
    {
        private readonly ITestOutputHelper _output;
        private readonly RiotItemFetcher _fetcher;

        public ItemStatsAnalyzerTests(ITestOutputHelper output)
        {
            _output = output;
            var config = new RiotApiConfiguration();
            var httpClient = new HttpClient();
            _fetcher = new RiotItemFetcher(config, httpClient);
        }

        [Fact(DisplayName = "Analyze available item stats from Riot API")]
        public async Task AnalyzeItemStats_ShowsAvailableStats()
        {
            // Arrange & Act
            var items = await _fetcher.GetAllItemsAsync();
            var allStats = new HashSet<string>();
            var itemsWithStats = new Dictionary<string, Dictionary<string, double>>();
            var statExamples = new Dictionary<string, List<(string itemName, double value)>>();
            
            // Find items with stats and collect all unique stat types
            foreach (var item in items) // Look at all items
            {
                var riotItem = await _fetcher.GetRiotItemDetailsAsync(item.Name);
                if (riotItem?.stats != null && riotItem.stats.Any())
                {
                    itemsWithStats[item.Name] = riotItem.stats;
                    foreach (var stat in riotItem.stats)
                    {
                        allStats.Add(stat.Key);
                        if (!statExamples.ContainsKey(stat.Key))
                            statExamples[stat.Key] = new List<(string, double)>();
                        statExamples[stat.Key].Add((item.Name, stat.Value));
                    }
                }
            }

            // Output summary
            _output.WriteLine("\n=== Item Stats Analysis ===");
            _output.WriteLine($"\nFound {allStats.Count} unique stat types across {itemsWithStats.Count} items:\n");
            
            foreach (var stat in allStats.OrderBy(s => s))
            {
                var itemsWithThisStat = itemsWithStats.Count(x => x.Value.ContainsKey(stat));
                _output.WriteLine($"\n{stat}:");
                _output.WriteLine($"Found on {itemsWithThisStat} items");
                _output.WriteLine("Example values:");
                
                // Show up to 5 examples of items with different values for this stat
                var examples = statExamples[stat]
                    .GroupBy(x => x.value)
                    .OrderByDescending(g => g.Count())
                    .Take(5);
                
                foreach (var example in examples)
                {
                    var exampleItems = example.Select(x => x.itemName).Take(3);
                    _output.WriteLine($"  {example.Key:0.##} ({string.Join(", ", exampleItems)}{(example.Count() > 3 ? ", ..." : "")})");
                }
            }

            _output.WriteLine("\n=== Most Stat-Rich Items ===");
            foreach (var item in itemsWithStats.OrderByDescending(x => x.Value.Count).Take(10))
            {
                _output.WriteLine($"\n{item.Key} ({item.Value.Count} stats):");
                foreach (var stat in item.Value.OrderBy(x => x.Key))
                {
                    _output.WriteLine($"  {stat.Key}: {stat.Value}");
                }
            }

            // Assert we found some stats
            Assert.NotEmpty(allStats);
            Assert.NotEmpty(itemsWithStats);
        }
    }
}