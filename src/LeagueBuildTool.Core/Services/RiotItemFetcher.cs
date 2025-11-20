using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using LeagueBuildTool.Core.RiotItemData;
using LeagueBuildTool.Core.Configuration;

namespace LeagueBuildTool.Core
{
    /// <summary>
    /// Service class responsible for fetching and caching League of Legends item data from Riot's Data Dragon API.
    /// Provides methods to retrieve both detailed item information and basic item summaries.
    /// </summary>
    public class RiotItemFetcher
    {
        private readonly RiotApiConfiguration _config;
        private readonly HttpClient _client;
        private RiotItemDataRoot? _cachedRoot;

        public RiotItemFetcher(RiotApiConfiguration config, HttpClient client)
        {
            _config = config;
            _client = client;
        }

        /// <summary>
        /// Retrieves and caches the complete item dataset from Riot's API.
        /// Implements a caching mechanism to minimize API calls and improve performance.
        /// </summary>
        /// <returns>A RiotItemDataRoot object containing all item data</returns>
        /// <exception cref="JsonException">Thrown when item data cannot be deserialized</exception>
        private async Task<RiotItemDataRoot> GetRiotDataAsync()
        {
            if (_cachedRoot != null)
                return _cachedRoot;

            string json = await _client.GetStringAsync(_config.GetItemListUrl());
            _cachedRoot = JsonConvert.DeserializeObject<RiotItemDataRoot>(json);
            return _cachedRoot ?? throw new JsonException("Failed to deserialize Riot item data");
        }

        /// <summary>
        /// Retrieves detailed information about a specific item by its name.
        /// </summary>
        /// <param name="itemName">The exact name of the item to look up</param>
        /// <returns>A RiotItem object containing detailed item information, or null if not found</returns>
        public async Task<RiotItem?> GetRiotItemDetailsAsync(string itemName)
        {
            var root = await GetRiotDataAsync();
            return root.data.Values.FirstOrDefault(item => item.name == itemName);
        }

        /// <summary>
        /// Retrieves a list of all available items, converting them from Riot's data format
        /// to our internal Item representation. Handles null values and missing data gracefully.
        /// </summary>
        /// <returns>A list of Item objects containing basic information about all available items</returns>
        public async Task<List<Item>> GetAllItemsAsync()
        {
            var root = await GetRiotDataAsync();
            List<Item> items = new List<Item>();

            if (root?.data != null) // null-safe check using null-conditional operator
            {
                foreach (var entry in root.data)
                {
                    RiotItem ri = entry.Value;

                    // Use null-coalescing operator for safe defaults
                    string name = ri.name ?? "Unknown Item";
                    string description = ri.plaintext ?? "No description available";
                    int cost = ri.gold?.total ?? 0;

                    var item = new Item(name, cost, description);

                    // copy stats dictionary if present
                    if (ri.stats != null)
                    {
                        // normalize Riot stat keys to canonical keys and aggregate values
                        var normalized = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                        foreach (var kv in ri.stats)
                        {
                            var normKey = LeagueBuildTool.Core.Utils.StatMapper.NormalizeStatKey(kv.Key);
                            if (normalized.ContainsKey(normKey)) normalized[normKey] += kv.Value;
                            else normalized[normKey] = kv.Value;
                        }
                        item.Stats = normalized;
                        item.Tags = LeagueBuildTool.Core.Utils.StatMapper.DeriveTagsFromStats(normalized);
                    }

                    items.Add(item);
                }
            }

            return items;
        }
    }
}
