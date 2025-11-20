using System.Threading.Tasks;
using LeagueBuildTool.Core.Data;

namespace LeagueBuildTool.Core.Services.BuildProviders
{
    public class LiveGameBuildProvider : IBuildProvider
    {
        private readonly LiveGameFetcher _liveGameFetcher;

        public LiveGameBuildProvider(LiveGameFetcher liveGameFetcher)
        {
            _liveGameFetcher = liveGameFetcher;
        }

        public BuildRecommendation.BuildSource Source => BuildRecommendation.BuildSource.LiveGame;

        public Task<string> GetCurrentPatchVersionAsync()
        {
            // This provider does not use a specific patch version
            return Task.FromResult("N/A");
        }

        public Task<List<BuildRecommendation.BuildRecommendation>> GetBuildRecommendationsAsync(string championName, string? role = null)
        {
            // This provider is for live games, so we need a summoner name
            throw new System.NotImplementedException();
        }

        public Task<List<BuildRecommendation.BuildRecommendation>> GetCounterBuildRecommendationsAsync(string championName, string opponentName, string? role = null)
        {
            // This provider is for live games, so we need a summoner name
            throw new System.NotImplementedException();
        }
    }
}
