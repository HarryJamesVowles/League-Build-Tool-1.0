using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeagueBuildTool.Core.Data.BuildRecommendation;
using LeagueBuildTool.Core.Data.RiotApi;

namespace LeagueBuildTool.Core.Services.BuildProviders
{
    /// <summary>
    /// Provides build recommendations for live games based on real-time team composition analysis.
    /// Analyzes enemy and ally team compositions to recommend defensive and offensive items.
    /// </summary>
    public class LiveGameBuildProvider : IBuildProvider
    {
        private readonly LiveGameFetcher _liveGameFetcher;
        private readonly RiotChampionFetcher _championFetcher;
        private readonly RiotItemFetcher _itemFetcher;
        private readonly Configuration.RiotApiConfiguration _config;

        public LiveGameBuildProvider(
            LiveGameFetcher liveGameFetcher,
            RiotChampionFetcher championFetcher,
            RiotItemFetcher itemFetcher,
            Configuration.RiotApiConfiguration config)
        {
            _liveGameFetcher = liveGameFetcher;
            _championFetcher = championFetcher;
            _itemFetcher = itemFetcher;
            _config = config;
        }

        public BuildSource Source => BuildSource.LiveGame;

        public Task<string> GetCurrentPatchVersionAsync()
        {
            // Return the configured patch version
            return Task.FromResult(_config.DataDragonVersion);
        }

        /// <summary>
        /// Not applicable for live game provider - summoner name is required.
        /// Use GetLiveGameBuildRecommendationsAsync instead.
        /// </summary>
        public Task<List<BuildRecommendation>> GetBuildRecommendationsAsync(string championName, string? role = null)
        {
            throw new InvalidOperationException(
                "LiveGameBuildProvider requires a summoner name. Use GetLiveGameBuildRecommendationsAsync instead.");
        }

        /// <summary>
        /// Not applicable for live game provider - summoner name is required.
        /// Use GetLiveGameBuildRecommendationsAsync instead.
        /// </summary>
        public Task<List<BuildRecommendation>> GetCounterBuildRecommendationsAsync(string championName, string opponentName, string? role = null)
        {
            throw new InvalidOperationException(
                "LiveGameBuildProvider requires a summoner name. Use GetLiveGameBuildRecommendationsAsync instead.");
        }

        /// <summary>
        /// Gets build recommendations for a live game by analyzing team composition.
        /// Fetches the active game data for the specified summoner and analyzes both teams
        /// to recommend items based on threats and win conditions.
        /// </summary>
        /// <param name="region">The region code (e.g., "na1", "euw1")</param>
        /// <param name="summonerName">The summoner to fetch live game data for</param>
        /// <returns>List of build recommendations based on live game analysis</returns>
        public async Task<List<BuildRecommendation>> GetLiveGameBuildRecommendationsAsync(string region, string summonerName)
        {
            var recommendations = new List<BuildRecommendation>();

            try
            {
                // Step 1: Get summoner info and their live game
                var summoner = await _liveGameFetcher.GetSummonerByNameAsync(region, summonerName);
                if (summoner == null)
                    return recommendations; // Summoner not found

                var liveGame = await _liveGameFetcher.GetLiveGameAsync(region, summoner.Id);
                if (liveGame == null)
                    return recommendations; // No active game

                // Step 2: Get all champions and items
                var allChampions = await _championFetcher.GetAllChampionsAsync();
                var allItems = await _itemFetcher.GetAllItemsAsync();

                // Step 3: Analyze team compositions
                var playerTeamId = GetPlayerTeamId(liveGame, summoner.Id);
                var playerChampion = GetPlayerChampion(liveGame, summoner.Id, allChampions);
                var enemyTeam = GetTeamChampions(liveGame, playerTeamId, false, allChampions);
                var allyTeam = GetTeamChampions(liveGame, playerTeamId, true, allChampions);

                if (playerChampion == null)
                    return recommendations; // Could not identify player champion

                // Step 4: Generate recommendations based on team analysis
                var recommendation = AnalyzeTeamAndRecommendBuild(
                    playerChampion,
                    enemyTeam,
                    allyTeam,
                    allItems,
                    liveGame);

                if (recommendation != null)
                    recommendations.Add(recommendation);
            }
            catch (Exception ex)
            {
                // Log error and return empty recommendations rather than throwing
                System.Diagnostics.Debug.WriteLine($"Error analyzing live game for {summonerName}: {ex.Message}");
            }

            return recommendations;
        }

        /// <summary>
        /// Determines which team the player is on based on their summoner ID.
        /// </summary>
        private long GetPlayerTeamId(SpectatorGameInfo game, string summonerId)
        {
            var player = game.Participants?.FirstOrDefault(p => p.SummonerId == summonerId);
            return player?.TeamId ?? 100;
        }

        /// <summary>
        /// Gets the player's champion object.
        /// </summary>
        private Champion? GetPlayerChampion(SpectatorGameInfo game, string summonerId, List<Champion> allChampions)
        {
            var participant = game.Participants?.FirstOrDefault(p => p.SummonerId == summonerId);
            if (participant == null)
                return null;

            // Get champion from champion list (we'll match by champion ID)
            // For now, return the first champion we can find as a placeholder
            // In production, you'd have a champion ID to name mapping
            return allChampions.FirstOrDefault() ?? new Champion { Name = "Unknown" };
        }

        /// <summary>
        /// Gets all champions on a specific team.
        /// </summary>
        private List<Champion> GetTeamChampions(SpectatorGameInfo game, long playerTeamId, bool isAllyTeam, List<Champion> allChampions)
        {
            var teamId = isAllyTeam ? playerTeamId : (playerTeamId == 100 ? 200 : 100);
            var teamMembers = game.Participants?.Where(p => p.TeamId == teamId).ToList() ?? new List<CurrentGameParticipant>();

            // Map participants to champions (placeholder - returns generic champions)
            return teamMembers.Select(m => new Champion { Name = m.SummonerName ?? "Unknown" }).ToList();
        }

        /// <summary>
        /// Analyzes team composition and generates build recommendations.
        /// Considers AP/AD ratios, crowd control, and threat analysis.
        /// </summary>
        private BuildRecommendation? AnalyzeTeamAndRecommendBuild(
            Champion playerChampion,
            List<Champion> enemyTeam,
            List<Champion> allyTeam,
            List<Item> allItems,
            SpectatorGameInfo game)
        {
            var recommendation = new BuildRecommendation
            {
                ChampionName = playerChampion.Name,
                Role = playerChampion.Lane,
                Source = BuildSource.LiveGame,
                PatchVersion = _config.DataDragonVersion,
                LastUpdated = DateTime.UtcNow,
                Notes = "Recommendations based on live game analysis"
            };

            // Analyze enemy team threat profile
            var enemyApCount = CountApThreats(enemyTeam);
            var enemyAdCount = CountAdThreats(enemyTeam);
            var enemyHasCc = HasCrowdControl(enemyTeam);
            var enemyHasHeal = HasHealing(enemyTeam);

            // Build recommendations based on threats
            var defensiveItems = RecommendDefensiveItems(enemyApCount, enemyAdCount, enemyHasCc, allItems);
            var offensiveItems = RecommendOffensiveItems(playerChampion, allItems);
            var situationalItems = RecommendSituationalItems(enemyHasHeal, allItems);

            recommendation.CoreItems = defensiveItems.Take(3).ToList();
            recommendation.StartingItems = new List<string> { "Doran's Blade" }; // Placeholder
            recommendation.SituationalItems = situationalItems.Select(item => new SituationalItem
            {
                ItemName = item,
                Condition = "Based on enemy team composition"
            }).ToList();

            return recommendation;
        }

        /// <summary>
        /// Counts the number of AP threats on a team.
        /// </summary>
        private int CountApThreats(List<Champion> team)
        {
            return team.Count(c => c.Tags.Contains("AP") || c.Tags.Contains("Mage"));
        }

        /// <summary>
        /// Counts the number of AD threats on a team.
        /// </summary>
        private int CountAdThreats(List<Champion> team)
        {
            return team.Count(c => c.Tags.Contains("AD") || c.Tags.Contains("Marksman") || c.Tags.Contains("Assassin"));
        }

        /// <summary>
        /// Checks if team has crowd control champions.
        /// </summary>
        private bool HasCrowdControl(List<Champion> team)
        {
            return team.Any(c => c.Tags.Contains("Support") || c.Tags.Contains("Tank"));
        }

        /// <summary>
        /// Checks if team has healing/sustain.
        /// </summary>
        private bool HasHealing(List<Champion> team)
        {
            return team.Any(c => c.Tags.Contains("Support"));
        }

        /// <summary>
        /// Recommends defensive items based on enemy team threats.
        /// </summary>
        private List<string> RecommendDefensiveItems(int apThreatCount, int adThreatCount, bool hasCc, List<Item> allItems)
        {
            var items = new List<string>();

            // Recommend MR items if significant AP threats
            if (apThreatCount >= 2)
            {
                items.Add("Kaenic Rookern");
                items.Add("Mercury's Treads");
                items.Add("Abyssal Mask");
            }

            // Recommend Armor items if significant AD threats
            if (adThreatCount >= 2)
            {
                items.Add("Plated Steelcaps");
                items.Add("Hollow Radiance");
                items.Add("Dead Man's Plate");
            }

            // Recommend CC mitigation
            if (hasCc)
            {
                items.Add("Mercury's Treads");
            }

            return items.Distinct().ToList();
        }

        /// <summary>
        /// Recommends offensive items based on champion type.
        /// </summary>
        private List<string> RecommendOffensiveItems(Champion champion, List<Item> allItems)
        {
            var items = new List<string>();

            // Basic offensive recommendations based on champion tags
            if (champion.Tags.Contains("AD"))
            {
                items.Add("Infinity Edge");
                items.Add("Trinity Force");
            }
            else if (champion.Tags.Contains("AP"))
            {
                items.Add("Rabadon's Deathcap");
                items.Add("Zhonya's Hourglass");
            }
            else
            {
                items.Add("Trinity Force");
                items.Add("Black Cleaver");
            }

            return items;
        }

        /// <summary>
        /// Recommends situational items based on specific conditions.
        /// </summary>
        private List<string> RecommendSituationalItems(bool enemyHasHeal, List<Item> allItems)
        {
            var items = new List<string>();

            // Recommend grievous wounds if enemy has healing
            if (enemyHasHeal)
            {
                items.Add("Mortal Reminder");
                items.Add("Executioner's Calling");
            }

            return items;
        }
    }
}
