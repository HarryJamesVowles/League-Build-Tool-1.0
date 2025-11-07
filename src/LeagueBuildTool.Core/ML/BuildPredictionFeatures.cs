namespace LeagueBuildTool.Core.ML
{
    /// <summary>
    /// Contains features used for build recommendation ML model
    /// </summary>
    public class BuildPredictionFeatures
    {
        /// <summary>
        /// Champion for which we're predicting the build
        /// </summary>
        public string ChampionName { get; set; } = string.Empty;

        /// <summary>
        /// Role being played
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Enemy team composition
        /// </summary>
        public List<string> EnemyTeam { get; set; } = new();

        /// <summary>
        /// Ally team composition
        /// </summary>
        public List<string> AllyTeam { get; set; } = new();

        /// <summary>
        /// Enemy damage composition (AP/AD ratio)
        /// </summary>
        public double EnemyDamageRatio { get; set; }

        /// <summary>
        /// Team's damage composition (AP/AD ratio)
        /// </summary>
        public double TeamDamageRatio { get; set; }

        /// <summary>
        /// Items already built
        /// </summary>
        public List<string> CurrentItems { get; set; } = new();

        /// <summary>
        /// Current gold available
        /// </summary>
        public int CurrentGold { get; set; }

        /// <summary>
        /// Game time in minutes
        /// </summary>
        public double GameTime { get; set; }

        /// <summary>
        /// Gold difference compared to opponent (-ve if behind)
        /// </summary>
        public int GoldDifference { get; set; }

        /// <summary>
        /// Team's objective control score
        /// </summary>
        public double ObjectiveControl { get; set; }
    }
}