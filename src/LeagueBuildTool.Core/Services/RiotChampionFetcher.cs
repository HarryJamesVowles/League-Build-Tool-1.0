using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

            // Limit concurrent per-champion detail requests to avoid hammering the CDN
            private static readonly SemaphoreSlim detailFetchSemaphore = new SemaphoreSlim(8);

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
                // We'll fetch per-champion detail JSON concurrently (bounded) to read authoritative 'partype'.
                var entries = root.data.Values.ToList();
                var tasks = entries.Select(async entry =>
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

                    // Normalize Riot stats to canonical keys
                    if (entry.stats != null)
                    {
                        var normalized = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                        foreach (var kv in entry.stats)
                        {
                            var normKey = LeagueBuildTool.Core.Utils.StatMapper.NormalizeStatKey(kv.Key);
                            if (normalized.ContainsKey(normKey)) normalized[normKey] += kv.Value;
                            else normalized[normKey] = kv.Value;
                        }

                        // ensure required keys exist and fill from normalized dict or 0
                        foreach (var key in Champion.RequiredBaseStatKeys)
                        {
                            c.BaseStats[key] = normalized.ContainsKey(key) ? normalized[key] : 0.0;
                        }

                        // derive tags based on normalized base stats
                        var derived = LeagueBuildTool.Core.Utils.StatMapper.DeriveTagsFromStats(c.BaseStats);
                        foreach (var t in derived)
                        {
                            if (!c.Tags.Contains(t)) c.Tags.Add(t);
                        }

                        // detect resource types from raw riot keys (fallback)
                        var resourceTags = LeagueBuildTool.Core.Utils.StatMapper.DetectResourcesFromRawKeys(entry.stats.Keys);
                        foreach (var rt in resourceTags)
                        {
                            if (!c.Tags.Contains(rt)) c.Tags.Add(rt);
                        }
                    }

                    // Fetch detailed champion JSON to read the authoritative 'partype' field
                    try
                    {
                        await detailFetchSemaphore.WaitAsync();
                        var champId = entry.id ?? entry.name ?? string.Empty;
                        if (!string.IsNullOrEmpty(champId))
                        {
                            // use the same version as the root to ensure compatibility
                            var detailUrl = $"https://ddragon.leagueoflegends.com/cdn/{root.version}/data/en_US/champion/{champId}.json";
                            var detailJson = await client.GetStringAsync(detailUrl);
                            var jobj = JObject.Parse(detailJson);
                            // path: data -> {champId} -> partype
                            var partype = jobj["data"]?[champId]?["partype"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(partype))
                            {
                                var resourceTag = $"Resource:{partype}";
                                if (!c.Tags.Contains(resourceTag)) c.Tags.Add(resourceTag);
                            }
                        }
                    }
                    catch
                    {
                        // ignore per-champion fetch errors; we already have fallbacks
                    }
                    finally
                    {
                        detailFetchSemaphore.Release();
                    }

                    return c;
                }).ToList();

                var results = await Task.WhenAll(tasks);
                champions.AddRange(results);
            }

            return champions;
        }
    }
}
