# ITEM 5: Centralized Configuration Management

## Overview
Implement a centralized configuration system to manage all application settings, API keys, environment variables, and secrets in a secure, maintainable way.

## Why This Is Important

### Current Problems:
1. **Hardcoded Values Scattered Everywhere**
   - API version "13.18.1" hardcoded in multiple fetcher classes
   - Base URLs duplicated across services
   - No way to change settings without recompiling code

2. **Security Risks**
   - API keys would need to be in source code (dangerous!)
   - Secrets exposed in version control
   - No separation between dev/staging/production settings

3. **Maintenance Nightmare**
   - Changing a version means editing multiple files
   - Regional differences require code changes
   - Testing different configurations is difficult

4. **No Environment Flexibility**
   - Can't easily switch between regions (NA, EUW, KR, etc.)
   - Can't have different settings for local dev vs production
   - No way to configure rate limits or retry behavior

## What Needs To Be Done

### 1. Create Configuration Classes
```
LeagueBuildTool.Core/Configuration/
├── ApiConfiguration.cs          - API keys, endpoints, versions
├── RegionConfiguration.cs       - Region-specific settings
├── AppConfiguration.cs          - General app settings
└── RateLimitConfiguration.cs    - Rate limiting rules
```

### 2. Configuration Values to Extract

**From RiotChampionFetcher.cs:**
- `version = "13.18.1"` → ConfigSection
- `baseUrl` → ConfigSection
- `championListUrl` → ConfigSection
- Semaphore limit (8) → ConfigSection

**From RiotItemFetcher.cs:**
- Item data URL → ConfigSection
- Version string → ConfigSection

**Future additions:**
- Riot API key (for live game/summoner data)
- Region endpoints (na1.api.riotgames.com, etc.)
- Rate limits (20 requests per second, etc.)
- Cache durations
- Retry policies

### 3. Configuration Sources (Priority Order)
1. **Environment Variables** (highest priority - for secrets)
2. **appsettings.json** (general settings)
3. **appsettings.{Environment}.json** (environment-specific)
4. **User Secrets** (local dev only - never committed)
5. **Default Values** (fallback)

### 4. Implementation Pattern

**Create Configuration Model:**
```csharp
public class RiotApiConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string DataDragonVersion { get; set; } = "13.18.1";
    public string BaseUrl { get; set; } = "https://ddragon.leagueoflegends.com";
    public string Region { get; set; } = "na1";
    public int MaxConcurrentRequests { get; set; } = 8;
    public int RateLimitPerSecond { get; set; } = 20;
}
```

**Load in Startup:**
```csharp
// In Program.cs or dependency injection setup
var config = builder.Configuration
    .GetSection("RiotApi")
    .Get<RiotApiConfiguration>();
```

**Inject Into Services:**
```csharp
public class RiotChampionFetcher
{
    private readonly RiotApiConfiguration _config;
    
    public RiotChampionFetcher(RiotApiConfiguration config)
    {
        _config = config;
    }
    
    private string GetChampionUrl() => 
        $"{_config.BaseUrl}/cdn/{_config.DataDragonVersion}/data/en_US/champion.json";
}
```

### 5. Security Best Practices

**For API Keys:**
```bash
# Never in code
dotnet user-secrets init
dotnet user-secrets set "RiotApi:ApiKey" "RGAPI-your-key-here"
```

**For Production:**
- Use Azure Key Vault / AWS Secrets Manager
- Or environment variables in deployment
- Never commit secrets to git

### 6. Example appsettings.json
```json
{
  "RiotApi": {
    "DataDragonVersion": "13.18.1",
    "BaseUrl": "https://ddragon.leagueoflegends.com",
    "Region": "na1",
    "MaxConcurrentRequests": 8,
    "RateLimitPerSecond": 20,
    "EnableCaching": true,
    "CacheDurationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "LeagueBuildTool": "Debug"
    }
  }
}
```

## Benefits After Implementation

✅ **Security**: API keys never in source code
✅ **Flexibility**: Change settings without recompiling
✅ **Testing**: Easy to use test/mock configurations
✅ **Environments**: Different settings for dev/staging/prod
✅ **Maintenance**: One place to update versions and URLs
✅ **Scalability**: Easy to add new configuration options
✅ **Best Practices**: Industry-standard configuration pattern

## Implementation Steps

1. Create Configuration classes in `Configuration/` folder
2. Create `appsettings.json` in App project
3. Add configuration binding in Program.cs
4. Refactor fetcher services to use injected config
5. Set up user secrets for local development
6. Document configuration options in README
7. Add validation for required configuration values

## Estimated Effort
- **Time**: 2-3 hours
- **Complexity**: Medium
- **Impact**: High
- **Priority**: High (should be done before adding more features)

## Related Items
- Connects to Item 6 (logging - also configured here)
- Enables Item 1 (better data retrieval with configurable endpoints)
- Required before implementing real API key usage
