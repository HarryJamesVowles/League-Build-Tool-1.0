using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LeagueBuildTool.Core.RiotChampionData;
using LeagueBuildTool.Core.Data;
using LeagueBuildTool.Core.Configuration;

namespace LeagueBuildTool.Core
{
    /// <summary>
    /// Service responsible for fetching champion list data from Riot's Data Dragon and
    /// converting it into our internal Champion representation.
    /// </summary>
    public class RiotChampionFetcher
    {
        private readonly RiotApiConfiguration _config;
        private readonly HttpClient _client;
        private readonly SemaphoreSlim _detailFetchSemaphore;
        private RiotChampionDataRoot? _cachedRoot;
        private readonly Dictionary<string, ChampionDetail> _cachedDetails = new();

        public RiotChampionFetcher(RiotApiConfiguration config, HttpClient client)
        {
            _config = config;
            _client = client;
            _detailFetchSemaphore = new SemaphoreSlim(_config.MaxConcurrentRequests);
        }

        private async Task<RiotChampionDataRoot> GetRiotDataAsync()
        {
            if (_cachedRoot != null) return _cachedRoot;
            string json = await _client.GetStringAsync(_config.GetChampionListUrl());
            _cachedRoot = JsonConvert.DeserializeObject<RiotChampionDataRoot>(json);
            return _cachedRoot ?? throw new JsonException("Failed to deserialize Riot champion data");
        }

        /// <summary>
        /// Fetches all champions and converts them into our internal Champion objects.
        /// Ensures each Champion has the canonical BaseStats keys present.
        /// </summary>
        /// <returns>List of Champion objects</returns>
        public async Task<List<Champion>> GetAllChampionsAsync()
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
                        await _detailFetchSemaphore.WaitAsync();
                        var champId = entry.id ?? entry.name ?? string.Empty;
                        if (!string.IsNullOrEmpty(champId))
                        {
                            var detailUrl = _config.GetChampionDetailUrl(champId);
                            var detailJson = await _client.GetStringAsync(detailUrl);
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
                        _detailFetchSemaphore.Release();
                    }

                    return c;
                }).ToList();

                var results = await Task.WhenAll(tasks);
                champions.AddRange(results);
            }

            return champions;
        }

        /// <summary>
        /// Fetches basic champion data for all champions (similar to champion select screen).
        /// This method is optimized for quickly loading basic champion information without detailed data.
        /// </summary>
        /// <returns>Dictionary of champion name to Champion object with basic stats</returns>
        public async Task<Dictionary<string, Champion>> GetBasicChampionDataAsync()
        {
            var root = await GetRiotDataAsync();
            var champions = new Dictionary<string, Champion>(StringComparer.OrdinalIgnoreCase);

            if (root?.data != null)
            {
                foreach (var entry in root.data.Values)
                {
                    var champion = new Champion
                    {
                        Name = entry.name ?? string.Empty,
                        Lane = string.Empty
                    };

                    if (entry.tags != null)
                    {
                        champion.Tags = entry.tags.ToList();
                    }

                    if (entry.stats != null)
                    {
                        var normalized = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                        foreach (var kv in entry.stats)
                        {
                            var normKey = LeagueBuildTool.Core.Utils.StatMapper.NormalizeStatKey(kv.Key);
                            if (normalized.ContainsKey(normKey)) normalized[normKey] += kv.Value;
                            else normalized[normKey] = kv.Value;
                        }

                        foreach (var key in Champion.RequiredBaseStatKeys)
                        {
                            champion.BaseStats[key] = normalized.ContainsKey(key) ? normalized[key] : 0.0;
                        }
                    }

                    champions[champion.Name] = champion;
                }
            }

            return champions;
        }

        /// <summary>
        /// Loads detailed champion data for specific champions (like after champion select).
        /// This method fetches additional information like abilities, resource types, and lore.
        /// </summary>
        /// <param name="championNames">List of champion names to load details for (max 10 for a typical game)</param>
        /// <returns>Dictionary of champion name to their detailed information</returns>
        public async Task<Dictionary<string, ChampionDetail>> LoadChampionDetailsAsync(IEnumerable<string> championNames)
        {
            var root = await GetRiotDataAsync();
            var details = new Dictionary<string, ChampionDetail>();
            
            // Convert to HashSet for O(1) lookup and to remove duplicates
            var championsToLoad = new HashSet<string>(championNames, StringComparer.OrdinalIgnoreCase);
            
            // Limit to 10 champions (5v5 game)
            if (championsToLoad.Count > 10)
            {
                throw new ArgumentException("Cannot load details for more than 10 champions at once", nameof(championNames));
            }

            var tasks = championsToLoad.Select(async championName =>
            {
                // Check cache first
                if (_cachedDetails.TryGetValue(championName, out var cachedDetail))
                {
                    return (championName, cachedDetail);
                }

                try
                {
                    await _detailFetchSemaphore.WaitAsync();
                    
                    // Find the champion ID from our cached root data
                    var champId = root?.data?.Values
                        .FirstOrDefault(c => string.Equals(c.name, championName, StringComparison.OrdinalIgnoreCase))?.id;

                    if (string.IsNullOrEmpty(champId))
                    {
                        throw new KeyNotFoundException($"Champion '{championName}' not found in basic data");
                    }

                    var detailUrl = _config.GetChampionDetailUrl(champId);
                    var detailJson = await _client.GetStringAsync(detailUrl);
                    var jobj = JObject.Parse(detailJson);
                    var champData = jobj["data"]?[champId];

                    if (champData == null)
                    {
                        throw new JsonException($"Failed to parse detail data for champion '{championName}'");
                    }

                    var detail = new ChampionDetail
                    {
                        ParType = champData["partype"]?.ToString() ?? string.Empty,
                        Title = champData["title"]?.ToString() ?? string.Empty,
                        Lore = champData["lore"]?.ToString() ?? string.Empty
                    };

                    // Parse abilities
                    var spells = champData["spells"] as JArray;
                    if (spells != null)
                    {
                        foreach (var spell in spells)
                        {
                            detail.Abilities.Add(new ChampionAbility
                            {
                                Name = spell["name"]?.ToString() ?? string.Empty,
                                Description = spell["description"]?.ToString() ?? string.Empty,
                                Tooltip = spell["tooltip"]?.ToString() ?? string.Empty
                            });
                        }
                    }

                    // Cache the result
                    _cachedDetails[championName] = detail;
                    return (championName, detail);
                }
                finally
                {
                    _detailFetchSemaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);
            foreach (var (name, detail) in results)
            {
                details[name] = detail;
            }

            return details;
        }
    }
}
