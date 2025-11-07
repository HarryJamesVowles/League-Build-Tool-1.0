namespace LeagueBuildTool.Core.Configuration
{
    /// <summary>
    /// Configuration settings for external APIs
    /// </summary>
    public class ApiConfiguration
    {
        /// <summary>
        /// Riot Games API key
        /// </summary>
        public string RiotApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Region for API requests (e.g., na1, euw1)
        /// </summary>
        public string Region { get; set; } = "na1";

        /// <summary>
        /// Current game version (e.g., "13.18.1")
        /// </summary>
        public string GameVersion { get; set; } = "13.18.1";
    }
}