using System.Threading.Tasks;

namespace LeagueBuildTool.Core.Services.BuildProviders
{
    /// <summary>
    /// Interface for build recommendation providers (U.GG, OP.GG, etc.)
    /// </summary>
    public interface IBuildProvider
    {
        /// <summary>
        /// Gets the source of this build provider
        /// </summary>
        BuildRecommendation.BuildSource Source { get; }

        /// <summary>
        /// Gets the current patch version this provider is using
        /// </summary>
        /// <returns>The patch version string (e.g., "13.18.1")</returns>
        Task<string> GetCurrentPatchVersionAsync();

        /// <summary>
        /// Gets build recommendations for a specific champion and role
        /// </summary>
        /// <param name="championName">Name of the champion</param>
        /// <param name="role">Role/position (can be null for all roles)</param>
        /// <returns>List of build recommendations</returns>
        Task<List<BuildRecommendation.BuildRecommendation>> GetBuildRecommendationsAsync(string championName, string? role = null);

        /// <summary>
        /// Gets build recommendations for a specific champion against a specific opponent
        /// </summary>
        /// <param name="championName">Name of the champion</param>
        /// <param name="opponentName">Name of the opponent champion</param>
        /// <param name="role">Role/position (can be null for all roles)</param>
        /// <returns>List of build recommendations</returns>
        Task<List<BuildRecommendation.BuildRecommendation>> GetCounterBuildRecommendationsAsync(string championName, string opponentName, string? role = null);
    }
}