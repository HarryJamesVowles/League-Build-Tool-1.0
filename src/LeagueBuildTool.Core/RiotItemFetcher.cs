using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

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
            HttpClient client = new HttpClient();
            string json = await client.GetStringAsync(url);

            // The JSON structure has a "data" object containing all items
            var raw = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, RiotItem>>>(json);

            List<Item> items = new List<Item>();

            // Iterate through each item entry in the "data" object
            foreach (var entry in raw["data"])
            {
                RiotItem ri = entry.Value;
                items.Add(new Item(ri.name, ri.gold.total, ri.plaintext));
            }

            return items;
        }
    }

    // DTO to deserialize Riot JSON
    public class RiotItem
    {
        public string name { get; set; }
        public string plaintext { get; set; }
        public Gold gold { get; set; }
        public Dictionary<string, double> stats { get; set; }
    }

    // DTO for gold information
    public class Gold
    {
        public int baseValue { get; set; }
        public int total { get; set; }
        public int sell { get; set; }
        public bool purchasable { get; set; }
    }
}
