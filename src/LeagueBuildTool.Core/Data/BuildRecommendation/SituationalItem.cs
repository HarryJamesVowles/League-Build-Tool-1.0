namespace LeagueBuildTool.Core.Data.BuildRecommendation
{
    /// <summary>
    /// Represents a situational item recommendation with its conditions
    /// </summary>
    public class SituationalItem
    {
        /// <summary>
        /// The name of the recommended item
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// When to build this item (e.g., "Against AP", "When ahead", etc.)
        /// </summary>
        public string Condition { get; set; } = string.Empty;

        /// <summary>
        /// Priority of this item in the given condition (if specified)
        /// </summary>
        public int? Priority { get; set; }

        /// <summary>
        /// Win rate with this item in the given condition (if available)
        /// </summary>
        public double? ConditionalWinRate { get; set; }
    }
}