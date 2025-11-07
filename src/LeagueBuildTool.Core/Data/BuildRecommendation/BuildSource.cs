namespace LeagueBuildTool.Core.Data.BuildRecommendation
{
    /// <summary>
    /// Represents the source of a build recommendation
    /// </summary>
    public enum BuildSource
    {
        /// <summary>
        /// Build recommendation from U.GG
        /// </summary>
        UGG,

        /// <summary>
        /// Build recommendation from OP.GG
        /// </summary>
        OPGG,

        /// <summary>
        /// Build recommendation from Riot's recommended items
        /// </summary>
        RiotRecommended
    }
}