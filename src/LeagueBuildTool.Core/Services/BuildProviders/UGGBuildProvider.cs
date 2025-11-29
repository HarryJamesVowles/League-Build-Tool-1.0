using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LeagueBuildTool.Core.Data.BuildRecommendation;
using Newtonsoft.Json.Linq;

namespace LeagueBuildTool.Core.Services.BuildProviders
{
    /// <summary>
    /// Provides build recommendations from U.GG by scraping their website.
    /// U.GG provides statistics-based builds with win rates and pick rates.
    /// </summary>
    public class UGGBuildProvider : IBuildProvider
    {
        private readonly HttpClient _httpClient;
        private readonly Configuration.RiotApiConfiguration _config;

        // U.GG base URL - can be configured per region
        private const string BASE_URL = "https://u.gg/lol/champion";
        private const string API_URL = "https://api.u.gg/v1/web";

        // User-Agent to avoid being blocked by scraping detection
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";

        // Cache to avoid excessive scraping (6 hours)
        private readonly Dictionary<string, CachedBuild> _buildCache = new();
        private const int CACHE_DURATION_MINUTES = 360;

        public UGGBuildProvider(HttpClient httpClient, Configuration.RiotApiConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;

            // Configure HttpClient for web scraping
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            }
        }

        public BuildSource Source => BuildSource.UGG;

        /// <summary>
        /// Gets the current patch version from U.GG
        /// </summary>
        public async Task<string> GetCurrentPatchVersionAsync()
        {
            try
            {
                // Attempt to extract patch from U.GG homepage
                var response = await _httpClient.GetAsync("https://u.gg/lol/patch");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // Extract patch version from page (format: "Patch 13.18.1" or similar)
                    var match = Regex.Match(content, @"Patch\s+([\d.]+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching patch version from U.GG: {ex.Message}");
            }

            // Fallback to configured version
            return _config.DataDragonVersion;
        }

        /// <summary>
        /// Gets build recommendations for a specific champion and role from U.GG
        /// </summary>
        public async Task<List<BuildRecommendation>> GetBuildRecommendationsAsync(string championName, string? role = null)
        {
            try
            {
                // Check cache first
                var cacheKey = $"{championName}_{role}";
                if (_buildCache.TryGetValue(cacheKey, out var cached) && 
                    (DateTime.UtcNow - cached.CachedAt).TotalMinutes < CACHE_DURATION_MINUTES)
                {
                    return cached.Builds;
                }

                // Normalize champion name for URL (lowercase, no spaces)
                var urlChampion = championName.ToLower().Replace(" ", "");

                // Build U.GG URL with role if specified
                var url = role != null 
                    ? $"{BASE_URL}/{urlChampion}/{role.ToLower()}"
                    : $"{BASE_URL}/{urlChampion}";

                // Fetch the page
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new List<BuildRecommendation>();

                var content = await response.Content.ReadAsStringAsync();

                // Parse build data from the page
                var builds = ParseUGGBuildData(content, championName, role);

                // Cache the results
                _buildCache[cacheKey] = new CachedBuild
                {
                    Builds = builds,
                    CachedAt = DateTime.UtcNow
                };

                return builds;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching builds from U.GG for {championName}: {ex.Message}");
                return new List<BuildRecommendation>();
            }
        }

        /// <summary>
        /// Gets counter build recommendations from U.GG
        /// </summary>
        public async Task<List<BuildRecommendation>> GetCounterBuildRecommendationsAsync(string championName, string opponentName, string? role = null)
        {
            try
            {
                // Get the opponent champion's strong builds
                var baseBuilds = await GetBuildRecommendationsAsync(opponentName, role);
                if (!baseBuilds.Any())
                    return new List<BuildRecommendation>();

                // Get our champion's builds
                var ourBuilds = await GetBuildRecommendationsAsync(championName, role);
                if (!ourBuilds.Any())
                    return new List<BuildRecommendation>();

                // Create counter recommendations by adding defensive items
                var counterBuilds = new List<BuildRecommendation>();

                foreach (var ourBuild in ourBuilds)
                {
                    var counterBuild = new BuildRecommendation
                    {
                        ChampionName = championName,
                        Role = role ?? ourBuild.Role,
                        Source = BuildSource.UGG,
                        PatchVersion = ourBuild.PatchVersion,
                        LastUpdated = DateTime.UtcNow,
                        WinRate = ourBuild.WinRate,
                        PickRate = ourBuild.PickRate,
                        Notes = $"Counter build vs {opponentName} - Based on {opponentName}'s strongest items"
                    };

                    // Copy core items and add counter items
                    counterBuild.CoreItems = ourBuild.CoreItems.ToList();
                    counterBuild.StartingItems = ourBuild.StartingItems.ToList();

                    // Add defensive counter items
                    var opponentBuild = baseBuilds.FirstOrDefault();
                    if (opponentBuild != null)
                    {
                        // Analyze opponent's build for threat type
                        var hasAP = opponentBuild.CoreItems.Any(i =>
                            i.Contains("Deathcap") || i.Contains("Void") || i.Contains("Hourglass"));
                        var hasAD = opponentBuild.CoreItems.Any(i =>
                            i.Contains("Infinity") || i.Contains("Trinity") || i.Contains("Eclipse"));

                        var counterItems = new List<SituationalItem>();
                        if (hasAP)
                        {
                            counterItems.Add(new SituationalItem 
                            { 
                                ItemName = "Kaenic Rookern", 
                                Condition = "Against AP damage from opponent" 
                            });
                        }
                        if (hasAD)
                        {
                            counterItems.Add(new SituationalItem 
                            { 
                                ItemName = "Randuin's Omen", 
                                Condition = "Against AD damage from opponent" 
                            });
                        }

                        counterBuild.SituationalItems = counterItems;
                    }

                    counterBuilds.Add(counterBuild);
                }

                return counterBuilds;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching counter builds: {ex.Message}");
                return new List<BuildRecommendation>();
            }
        }

        /// <summary>
        /// Not applicable for U.GG provider - use GetBuildRecommendationsAsync instead
        /// </summary>
        public Task<List<BuildRecommendation>> GetLiveGameBuildRecommendationsAsync(string region, string summonerName)
        {
            throw new InvalidOperationException(
                "UGGBuildProvider does not support live game recommendations. Use GetBuildRecommendationsAsync with the champion and role instead.");
        }

        /// <summary>
        /// Parses build data from U.GG HTML content
        /// </summary>
        private List<BuildRecommendation> ParseUGGBuildData(string htmlContent, string championName, string? role)
        {
            var recommendations = new List<BuildRecommendation>();

            try
            {
                // Extract JSON data embedded in the page
                // U.GG typically includes build data in script tags or as JSON
                var jsonMatch = Regex.Match(htmlContent, @"<script[^>]*>.*?window\.__INITIAL_STATE__\s*=\s*({.*?})</script>", RegexOptions.Singleline);

                if (!jsonMatch.Success)
                {
                    // Try alternative pattern
                    jsonMatch = Regex.Match(htmlContent, @"<script type=""application/json""[^>]*>(.*?)</script>", RegexOptions.Singleline);
                }

                if (!jsonMatch.Success)
                {
                    // Fallback: Create a basic recommendation based on available data
                    return CreateBasicRecommendation(championName, role);
                }

                var jsonStr = jsonMatch.Groups[1].Value;
                var jsonData = JObject.Parse(jsonStr);

                // Parse the JSON to extract build information
                var buildRec = new BuildRecommendation
                {
                    ChampionName = championName,
                    Role = role ?? (ExtractRoleFromJson(jsonData) ?? "Mid"),
                    Source = BuildSource.UGG,
                    PatchVersion = ExtractPatchFromJson(jsonData) ?? _config.DataDragonVersion,
                    LastUpdated = DateTime.UtcNow
                };

                // Extract items from JSON
                buildRec.StartingItems = ExtractStartingItems(jsonData);
                buildRec.CoreItems = ExtractCoreItems(jsonData);
                buildRec.SituationalItems = ExtractSituationalItems(jsonData);
                buildRec.SkillOrder = ExtractSkillOrder(jsonData);
                buildRec.SummonerSpells = ExtractSummonerSpells(jsonData);

                // Extract statistics
                buildRec.WinRate = ExtractWinRate(jsonData);
                buildRec.PickRate = ExtractPickRate(jsonData);
                buildRec.SampleSize = ExtractSampleSize(jsonData);

                buildRec.Notes = "Build data from U.GG - Updated regularly based on current meta";

                if (buildRec.CoreItems.Any())
                {
                    recommendations.Add(buildRec);
                }
                else
                {
                    // If parsing failed, return basic recommendation
                    return CreateBasicRecommendation(championName, role);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing U.GG data: {ex.Message}");
                // Return basic recommendation on parse failure
                return CreateBasicRecommendation(championName, role);
            }

            return recommendations;
        }

        /// <summary>
        /// Creates a basic recommendation when scraping fails
        /// Contains placeholder data that indicates U.GG was checked but data couldn't be extracted
        /// </summary>
        private List<BuildRecommendation> CreateBasicRecommendation(string championName, string? role)
        {
            // Return a basic recommendation indicating we couldn't scrape detailed data
            // but U.GG should have current builds
            var rec = new BuildRecommendation
            {
                ChampionName = championName,
                Role = role ?? "Mid",
                Source = BuildSource.UGG,
                PatchVersion = _config.DataDragonVersion,
                LastUpdated = DateTime.UtcNow,
                Notes = "U.GG data available but detailed scraping requires update. Visit u.gg for current builds.",
                CoreItems = new List<string>
                {
                    "Trinity Force", // Placeholder
                    "Black Cleaver"  // Placeholder
                },
                StartingItems = new List<string> { "Doran's Blade" }
            };

            return new List<BuildRecommendation> { rec };
        }

        #region JSON Extraction Helpers

        private string? ExtractRoleFromJson(JObject json)
        {
            try
            {
                return json["champion"]?["role"]?.ToString() ?? json["role"]?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string? ExtractPatchFromJson(JObject json)
        {
            try
            {
                return json["patch"]?.ToString() ?? json["patchVersion"]?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private List<string> ExtractStartingItems(JObject json)
        {
            var items = new List<string>();
            try
            {
                var startingItems = json["build"]?["startingItems"] as JArray ?? json["startingItems"] as JArray;
                if (startingItems != null)
                {
                    items.AddRange(startingItems.Select(i => i.ToString() ?? ""));
                }
            }
            catch { }
            return items;
        }

        private List<string> ExtractCoreItems(JObject json)
        {
            var items = new List<string>();
            try
            {
                var coreItems = json["build"]?["coreItems"] as JArray ?? 
                               json["items"]?["core"] as JArray ?? 
                               json["coreItems"] as JArray;
                if (coreItems != null)
                {
                    items.AddRange(coreItems.Take(6).Select(i => i.ToString() ?? ""));
                }
            }
            catch { }
            return items.Where(i => !string.IsNullOrEmpty(i)).ToList();
        }

        private List<SituationalItem> ExtractSituationalItems(JObject json)
        {
            var items = new List<SituationalItem>();
            try
            {
                var situational = json["build"]?["situationalItems"] as JArray ?? 
                                 json["items"]?["situational"] as JArray ?? 
                                 json["situationalItems"] as JArray;
                if (situational != null)
                {
                    foreach (var item in situational)
                    {
                        var itemName = item["name"]?.ToString() ?? item.ToString();
                        var condition = item["condition"]?.ToString() ?? "Situational";
                        if (!string.IsNullOrEmpty(itemName))
                        {
                            items.Add(new SituationalItem { ItemName = itemName, Condition = condition });
                        }
                    }
                }
            }
            catch { }
            return items;
        }

        private string? ExtractSkillOrder(JObject json)
        {
            try
            {
                return json["build"]?["skillOrder"]?.ToString() ?? 
                       json["skillOrder"]?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private List<string> ExtractSummonerSpells(JObject json)
        {
            var spells = new List<string>();
            try
            {
                var summonerSpells = json["build"]?["summonerSpells"] as JArray ?? 
                                    json["summonerSpells"] as JArray;
                if (summonerSpells != null)
                {
                    spells.AddRange(summonerSpells.Select(s => s.ToString() ?? ""));
                }
            }
            catch { }
            return spells.Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        private double? ExtractWinRate(JObject json)
        {
            try
            {
                var wr = json["stats"]?["winRate"] ?? json["winRate"];
                if (wr != null && double.TryParse(wr.ToString(), out var winRate))
                {
                    return winRate;
                }
            }
            catch { }
            return null;
        }

        private double? ExtractPickRate(JObject json)
        {
            try
            {
                var pr = json["stats"]?["pickRate"] ?? json["pickRate"];
                if (pr != null && double.TryParse(pr.ToString(), out var pickRate))
                {
                    return pickRate;
                }
            }
            catch { }
            return null;
        }

        private int? ExtractSampleSize(JObject json)
        {
            try
            {
                var ss = json["stats"]?["gamesAnalyzed"] ?? 
                        json["stats"]?["sampleSize"] ?? 
                        json["sampleSize"];
                if (ss != null && int.TryParse(ss.ToString(), out var sampleSize))
                {
                    return sampleSize;
                }
            }
            catch { }
            return null;
        }

        #endregion

        /// <summary>
        /// Simple cache entry for build data
        /// </summary>
        private class CachedBuild
        {
            public List<BuildRecommendation> Builds { get; set; } = new();
            public DateTime CachedAt { get; set; }
        }
    }
}
