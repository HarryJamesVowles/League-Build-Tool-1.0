using System.Threading.Tasks;
using LeagueBuildTool.Core.Data.BuildRecommendation;

namespace LeagueBuildTool.Core.Services.BuildProviders
{
    public class LiveGameBuildProvider : IBuildProvider
    {
        private readonly LiveGameFetcher _liveGameFetcher;

        public LiveGameBuildProvider(LiveGameFetcher liveGameFetcher)
        {
            _liveGameFetcher = liveGameFetcher;
        }

        public BuildSource Source => BuildSource.LiveGame;

        public Task<string> GetCurrentPatchVersionAsync()
        {
            // This provider does not use a specific patch version
            return Task.FromResult("N/A");
        }

        public Task<List<BuildRecommendation>> GetBuildRecommendationsAsync(string championName, string? role = null)
        {
            // This provider is for live games, so we need a summoner name
            throw new System.NotImplementedException();
        }

        public Task<List<BuildRecommendation>> GetCounterBuildRecommendationsAsync(string championName, string opponentName, string? role = null)
        {
            // This provider is for live games, so we need a summoner name
            throw new System.NotImplementedException();
        }

        public Task<List<BuildRecommendation>> GetLiveGameBuildRecommendationsAsync(string region, string summonerName)
        {
            // Implementation for live game build recommendations
            throw new System.NotImplementedException();
        }
    }
}
