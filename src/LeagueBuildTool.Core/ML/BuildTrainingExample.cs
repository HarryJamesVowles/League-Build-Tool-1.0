namespace LeagueBuildTool.Core.ML
{
    /// <summary>
    /// Represents a training example for the build recommendation model
    /// </summary>
    public class BuildTrainingExample
    {
        /// <summary>
        /// Input features for the model
        /// </summary>
        public BuildPredictionFeatures Features { get; set; } = new();

        /// <summary>
        /// The items that were built in this example
        /// </summary>
        public List<string> BuiltItems { get; set; } = new();

        /// <summary>
        /// Whether this build resulted in a win
        /// </summary>
        public bool WasWin { get; set; }

        /// <summary>
        /// Player's rank in this game
        /// </summary>
        public string Rank { get; set; } = string.Empty;

        /// <summary>
        /// Patch version this game was played on
        /// </summary>
        public string PatchVersion { get; set; } = string.Empty;

        /// <summary>
        /// Match ID for reference
        /// </summary>
        public string MatchId { get; set; } = string.Empty;

        /// <summary>
        /// Performance metrics (KDA, damage dealt, etc.)
        /// </summary>
        public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    }
}