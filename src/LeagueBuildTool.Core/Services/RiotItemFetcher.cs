using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using LeagueBuildTool.Core.RiotItemData;

namespace LeagueBuildTool.Core
{
    /// <summary>
    /// Service class responsible for fetching and caching League of Legends item data from Riot's Data Dragon API.
    /// Provides methods to retrieve both detailed item information and basic item summaries.
    /// </summary>
    public class RiotItemFetcher
    {
        /// <summary>
        /// The base URL for fetching item data from Riot's Data Dragon CDN.
        /// Version 13.18.1 is used for consistency in item data.
        /// </summary>
        private static readonly string url = "https://ddragon.leagueoflegends.com/cdn/13.18.1/data/en_US/item.json";

        /// <summary>
        /// Cached item data to minimize API calls. Data is loaded on first request and reused.
        /// </summary>
        private static RiotItemDataRoot? cachedRoot;

        /// <summary>
        /// Shared HttpClient instance for making API requests.
        /// Static to properly manage connections and prevent socket exhaustion.
        /// </summary>
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Retrieves and caches the complete item dataset from Riot's API.
        /// Implements a caching mechanism to minimize API calls and improve performance.
        /// </summary>
        /// <returns>A RiotItemDataRoot object containing all item data</returns>
        /// <exception cref="JsonException">Thrown when item data cannot be deserialized</exception>
        private static async Task<RiotItemDataRoot> GetRiotDataAsync()
        {
            if (cachedRoot != null)
                return cachedRoot;

            string json = await client.GetStringAsync(url);
            cachedRoot = JsonConvert.DeserializeObject<RiotItemDataRoot>(json);
            return cachedRoot ?? throw new JsonException("Failed to deserialize Riot item data");
        }

        /// <summary>
        /// Retrieves detailed information about a specific item by its name.
        /// </summary>
        /// <param name="itemName">The exact name of the item to look up</param>
        /// <returns>A RiotItem object containing detailed item information, or null if not found</returns>
        public static async Task<RiotItem?> GetRiotItemDetailsAsync(string itemName)
        {
            var root = await GetRiotDataAsync();
            return root.data.Values.FirstOrDefault(item => item.name == itemName);
        }

        /// <summary>
        /// Retrieves a list of all available items, converting them from Riot's data format
        /// to our internal Item representation. Handles null values and missing data gracefully.
        /// </summary>
        /// <returns>A list of Item objects containing basic information about all available items</returns>
        public static async Task<List<Item>> GetAllItemsAsync()
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
