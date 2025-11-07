using LeagueBuildTool.Core.ML;
using LeagueBuildTool.Core.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LeagueBuildTool.Core.Services
{
    /// <summary>
    /// Service for collecting high-elo match data for ML training
    /// </summary>
    public class HighEloMatchCollector
    {
        private readonly HttpClient _client;
        private readonly ApiConfiguration _config;
        private const string CHALLENGER_LEAGUE_URL = "https://{0}.api.riotgames.com/lol/league/v4/challengerleagues/by-queue/RANKED_SOLO_5x5";
        private const string MATCHES_BY_PUUID_URL = "https://{0}.api.riotgames.com/lol/match/v5/matches/by-puuid/{1}/ids";
        private const string MATCH_BY_ID_URL = "https://{0}.api.riotgames.com/lol/match/v5/matches/{1}";

        /// <summary>
        /// Ranks considered as high-elo for data collection
        /// </summary>
        public static readonly string[] HighEloRanks = new[] { "CHALLENGER", "GRANDMASTER", "MASTER", "DIAMOND" };

        public HighEloMatchCollector(ApiConfiguration config)
        {
            _client = new HttpClient();
            _config = config;
            _client.DefaultRequestHeaders.Add("X-Riot-Token", config.RiotApiKey);
        }

        /// <summary>
        /// Collects match data for ML training
        /// </summary>
        /// <param name="numberOfMatches">Number of matches to collect</param>
        /// <returns>List of training examples</returns>
        public async Task<List<BuildTrainingExample>> CollectHighEloMatchesAsync(int numberOfMatches)
        {
            var examples = new List<BuildTrainingExample>();
            var challengerPlayers = await GetChallengerPlayersAsync();
            var matchIds = new HashSet<string>();

            foreach (var player in challengerPlayers)
            {
                if (matchIds.Count >= numberOfMatches) break;

                var playerMatches = await GetPlayerMatchesAsync(player.puuid, 10);
                foreach (var matchId in playerMatches)
                {
                    if (matchIds.Count >= numberOfMatches) break;
                    if (matchIds.Contains(matchId)) continue;

                    matchIds.Add(matchId);
                    var matchData = await GetMatchDataAsync(matchId);
                    if (matchData != null)
                    {
                        var trainingExamples = ConvertMatchToTrainingExamples(matchData);
                        examples.AddRange(trainingExamples);
                    }
                }
            }

            return examples;
        }

        private async Task<List<(string summonerId, string puuid)>> GetChallengerPlayersAsync()
        {
            var url = string.Format(CHALLENGER_LEAGUE_URL, _config.Region);
            var response = await _client.GetStringAsync(url);
            var leagueData = JsonConvert.DeserializeObject<dynamic>(response);
            var players = new List<(string, string)>();

            foreach (var entry in leagueData.entries)
            {
                var summonerId = entry.summonerId.ToString();
                var puuid = await GetPuuidBySummonerIdAsync(summonerId);
                if (!string.IsNullOrEmpty(puuid))
                {
                    players.Add((summonerId, puuid));
                }
            }

            return players;
        }

        private async Task<string> GetPuuidBySummonerIdAsync(string summonerId)
        {
            var url = $"https://{_config.Region}.api.riotgames.com/lol/summoner/v4/summoners/{summonerId}";
            var response = await _client.GetStringAsync(url);
            var summonerData = JsonConvert.DeserializeObject<dynamic>(response);
            return summonerData.puuid.ToString();
        }

        private async Task<List<string>> GetPlayerMatchesAsync(string puuid, int count)
        {
            var url = string.Format(MATCHES_BY_PUUID_URL, _config.Region, puuid) + $"?count={count}";
            var response = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<List<string>>(response) ?? new List<string>();
        }

        private async Task<dynamic?> GetMatchDataAsync(string matchId)
        {
            var url = string.Format(MATCH_BY_ID_URL, _config.Region, matchId);
            var response = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<dynamic>(response);
        }

        private List<BuildTrainingExample> ConvertMatchToTrainingExamples(dynamic matchData)
        {
            var examples = new List<BuildTrainingExample>();
            var patch = matchData.info.gameVersion.ToString().Split('.')[0] + "." + matchData.info.gameVersion.ToString().Split('.')[1];

            foreach (var participant in matchData.info.participants)
            {
                var example = new BuildTrainingExample
                {
                    Features = new BuildPredictionFeatures
                    {
                        ChampionName = participant.championName.ToString(),
                        Role = participant.teamPosition.ToString(),
                        CurrentGold = participant.goldEarned,
                        GameTime = matchData.info.gameDuration / 60.0, // Convert to minutes
                    },
                    WasWin = participant.win,
                    Rank = HighEloRanks[0], // Challenger by default since we're collecting high-elo only
                    PatchVersion = patch,
                    MatchId = matchData.metadata.matchId.ToString()
                };

                // Add items that were built
                var itemSlots = new[] { "item0", "item1", "item2", "item3", "item4", "item5", "item6" };
                foreach (var slot in itemSlots)
                {
                    var itemId = (int)participant[slot];
                    if (itemId > 0)
                    {
                        example.BuiltItems.Add(itemId.ToString());
                    }
                }

                // Add performance metrics
                example.PerformanceMetrics = new Dictionary<string, double>
                {
                    { "kills", participant.kills },
                    { "deaths", participant.deaths },
                    { "assists", participant.assists },
                    { "damageDealt", participant.totalDamageDealtToChampions },
                    { "damageTaken", participant.totalDamageTaken },
                    { "visionScore", participant.visionScore }
                };

                examples.Add(example);
            }

            return examples;
        }
    }
}