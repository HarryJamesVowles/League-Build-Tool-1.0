using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using LeagueBuildTool.Core.RiotItemData;

namespace LeagueBuildTool.Core
{
    // Fetches item data from Riot's Data Dragon service.
    public class RiotItemFetcher
    {
        // URL to fetch item data in JSON format.
        private static readonly string url = "https://ddragon.leagueoflegends.com/cdn/13.18.1/data/en_US/item.json";

        // Asynchronously retrieves all items from the Riot API.
        public static async Task<List<Item>> GetAllItemsAsync()
        {
            using HttpClient client = new HttpClient(); // dispose HttpClient automatically
            string json = await client.GetStringAsync(url);

            // Deserialize the top-level JSON into RiotItemDataRoot
            var root = JsonConvert.DeserializeObject<RiotItemDataRoot>(json);

            List<Item> items = new List<Item>();

            if (root != null && root.data != null) // null-safe check
            {
                // Iterate through each item in the "data" dictionary
                foreach (var entry in root.data)
                {
                    RiotItem ri = entry.Value;

                    // Null-safe assignments
                    string name = ri.name ?? "Unknown Item";
                    string description = ri.plaintext ?? "No description available";
                    int cost = ri.gold?.total ?? 0;

                    items.Add(new Item(name, cost, description));
                }
            }

            return items;
        }
    }
}
