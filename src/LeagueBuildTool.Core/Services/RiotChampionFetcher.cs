using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using LeagueBuildTool.Core.RiotChampionData;

namespace LeagueBuildTool.Core
{
    /// <summary>
    /// Service responsible for fetching champion list data from Riot's Data Dragon and
    /// converting it into our internal Champion representation.
    /// </summary>
    public class RiotChampionFetcher
    {
        private static readonly string url = "https://ddragon.leagueoflegends.com/cdn/13.18.1/data/en_US/champion.json";
        private static RiotChampionDataRoot? cachedRoot;
        private static readonly HttpClient client = new HttpClient();

        private static async Task<RiotChampionDataRoot> GetRiotDataAsync()
        {
            if (cachedRoot != null) return cachedRoot;
            string json = await client.GetStringAsync(url);
            cachedRoot = JsonConvert.DeserializeObject<RiotChampionDataRoot>(json);
            return cachedRoot ?? throw new JsonException("Failed to deserialize Riot champion data");
        }

        /// <summary>
        /// Fetches all champions and converts them into our internal Champion objects.
        /// Ensures each Champion has the canonical BaseStats keys present.
        /// </summary>
        /// <returns>List of Champion objects</returns>
        public static async Task<List<Champion>> GetAllChampionsAsync()
        {
            var root = await GetRiotDataAsync();
            List<Champion> champions = new List<Champion>();

            if (root?.data != null)
            {
                foreach (var entry in root.data.Values)
                {
                    var c = new Champion
                    {
                        Name = entry.name ?? string.Empty,
                        Lane = string.Empty // lane is not provided by Data Dragon; left blank by default
                    };

                    // populate tags from Riot data if available
                    if (entry.tags != null)
                    {
                        c.Tags = entry.tags.ToList();
                    }

                    // Map Riot stats to our canonical BaseStats keys when possible
                    if (entry.stats != null)
                    {
                        // naive mapping: try to match common Riot keys to canonical keys
                        var riotStats = entry.stats;
                        foreach (var key in Champion.RequiredBaseStatKeys)
                        {
                            // default 0
                            c.BaseStats[key] = 0.0;
                        }

                        foreach (var kv in riotStats)
                        {
                            var rkey = kv.Key?.ToLowerInvariant() ?? string.Empty;
                            if (rkey.Contains("hp")) c.BaseStats["Health"] = kv.Value;
                            else if (rkey.Contains("attackdamage") || rkey.Contains("attack") && rkey.Contains("damage")) c.BaseStats["AttackDamage"] = kv.Value;
                            else if (rkey.Contains("ap") || rkey.Contains("abilitypower") || rkey.Contains("spelldamage")) c.BaseStats["AbilityPower"] = kv.Value;
                            else if (rkey.Contains("armor")) c.BaseStats["Armor"] = kv.Value;
                            else if (rkey.Contains("spellblock") || rkey.Contains("mr")) c.BaseStats["MagicResist"] = kv.Value;
                            else if (rkey.Contains("attackspeed")) c.BaseStats["AttackSpeed"] = kv.Value;
                            else if (rkey.Contains("movespeed")) c.BaseStats["MoveSpeed"] = kv.Value;
                            else if (rkey.Contains("hpregen")) c.BaseStats["HealthRegen"] = kv.Value;
                            else if (rkey.Contains("mp")) c.BaseStats["Mana"] = kv.Value;
                            else if (rkey.Contains("mpregen")) c.BaseStats["ManaRegen"] = kv.Value;
                        }
                    }

                    champions.Add(c);
                }
            }

            return champions;
        }
    }
}
