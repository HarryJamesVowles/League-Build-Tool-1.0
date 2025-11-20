using LeagueBuildTool.Core.Configuration;
using LeagueBuildTool.Core.Data.RiotApi;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace LeagueBuildTool.Core.Services
{
    /// <summary>
    /// Service for fetching live game data from the Riot API.
    /// </summary>
    public class LiveGameFetcher
    {
        private readonly HttpClient _client;
        private readonly ApiConfiguration _config;

        // Summoner API endpoint to get a player's basic info (including their ID) by name
        private const string SUMMONER_BY_NAME_URL = "https://{0}.api.riotgames.com/lol/summoner/v4/summoners/by-name/{1}";

        // Spectator API endpoint to get live game data for a given player
        private const string SPECTATOR_ACTIVE_GAME_URL = "https://{0}.api.riotgames.com/lol/spectator/v4/active-games/by-summoner/{1}";

        public LiveGameFetcher(ApiConfiguration config)
        {
            _client = new HttpClient();
            _config = config;
            _client.DefaultRequestHeaders.Add("X-Riot-Token", config.RiotApiKey);
        }

        public async Task<RiotSummoner> GetSummonerByNameAsync(string region, string summonerName)
        {
            var url = string.Format(SUMMONER_BY_NAME_URL, region, summonerName);
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RiotSummoner>(content);
        }

        public async Task<SpectatorGameInfo> GetLiveGameAsync(string region, string summonerId)
        {
            var url = string.Format(SPECTATOR_ACTIVE_GAME_URL, region, summonerId);
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SpectatorGameInfo>(content);
        }
    }
}
