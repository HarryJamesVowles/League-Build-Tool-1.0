namespace LeagueBuildTool.Core.Configuration;

/// <summary>
/// Configuration for Riot API and Data Dragon endpoints.
/// Contains all settings for accessing Riot's public data and authenticated APIs.
/// </summary>
public class RiotApiConfiguration
{
    /// <summary>
    /// Data Dragon version to use for champion and item data.
    /// Example: "13.18.1"
    /// </summary>
    public string DataDragonVersion { get; set; } = "13.18.1";

    /// <summary>
    /// Base URL for Data Dragon CDN (public, no API key required).
    /// Default: "https://ddragon.leagueoflegends.com"
    /// </summary>
    public string DataDragonBaseUrl { get; set; } = "https://ddragon.leagueoflegends.com";

    /// <summary>
    /// Maximum number of concurrent requests when fetching champion details.
    /// Used to prevent overwhelming the CDN with parallel requests.
    /// Default: 8
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 8;

    /// <summary>
    /// Whether to enable caching of API responses.
    /// Default: true
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Duration in minutes to cache API responses.
    /// Default: 60 minutes
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 60;

    /// <summary>
    /// Riot API key for authenticated endpoints (live game, summoner data, etc.).
    /// Optional - leave empty/null if not using authenticated features.
    /// Should be loaded from environment variables or user secrets, never hardcoded.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Region for Riot API authenticated requests.
    /// Examples: "na1", "euw1", "kr", "br1"
    /// Default: "na1"
    /// </summary>
    public string Region { get; set; } = "na1";

    /// <summary>
    /// Rate limit for authenticated API requests (requests per second).
    /// Default: 20 (typical for development API keys)
    /// </summary>
    public int RateLimitPerSecond { get; set; } = 20;

    /// <summary>
    /// Constructs the full URL for the champion list endpoint.
    /// </summary>
    public string GetChampionListUrl() =>
        $"{DataDragonBaseUrl}/cdn/{DataDragonVersion}/data/en_US/champion.json";

    /// <summary>
    /// Constructs the full URL for a specific champion's detail endpoint.
    /// </summary>
    public string GetChampionDetailUrl(string championKey) =>
        $"{DataDragonBaseUrl}/cdn/{DataDragonVersion}/data/en_US/champion/{championKey}.json";

    /// <summary>
    /// Constructs the full URL for the item list endpoint.
    /// </summary>
    public string GetItemListUrl() =>
        $"{DataDragonBaseUrl}/cdn/{DataDragonVersion}/data/en_US/item.json";
}
