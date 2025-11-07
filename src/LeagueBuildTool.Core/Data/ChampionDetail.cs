namespace LeagueBuildTool.Core.Data
{
    /// <summary>
    /// Represents detailed champion information fetched from the Data Dragon API.
    /// Contains additional information not available in the basic champion data.
    /// </summary>
    public class ChampionDetail
    {
        /// <summary>
        /// The champion's resource type (e.g., "Mana", "Energy", "None")
        /// </summary>
        public string ParType { get; set; } = string.Empty;

        /// <summary>
        /// Champion's title (e.g., "the Nine-Tailed Fox" for Ahri)
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Champion's lore description
        /// </summary>
        public string Lore { get; set; } = string.Empty;

        /// <summary>
        /// Champion's abilities information
        /// </summary>
        public List<ChampionAbility> Abilities { get; set; } = new List<ChampionAbility>();
    }

    /// <summary>
    /// Represents a champion's ability information
    /// </summary>
    public class ChampionAbility
    {
        /// <summary>
        /// Name of the ability
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of what the ability does
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Tooltip with detailed ability mechanics
        /// </summary>
        public string Tooltip { get; set; } = string.Empty;
    }
}