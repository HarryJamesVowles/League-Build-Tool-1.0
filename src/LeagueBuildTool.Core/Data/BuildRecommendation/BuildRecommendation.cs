using System;

namespace LeagueBuildTool.Core.Data.BuildRecommendation
{
    /// <summary>
    /// Represents a build recommendation from any supported source with metadata
    /// </summary>
    public class BuildRecommendation
    {
        /// <summary>
        /// The champion this build is for
        /// </summary>
        public string ChampionName { get; set; } = string.Empty;

        /// <summary>
        /// The role/position this build is for
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// The source of this build recommendation
        /// </summary>
        public BuildSource Source { get; set; }

        /// <summary>
        /// The League of Legends patch version this build is from
        /// </summary>
        public string PatchVersion { get; set; } = string.Empty;

        /// <summary>
        /// When this build data was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Win rate for this build (if available)
        /// </summary>
        public double? WinRate { get; set; }

        /// <summary>
        /// Pick rate for this build (if available)
        /// </summary>
        public double? PickRate { get; set; }

        /// <summary>
        /// Sample size for the statistics (if available)
        /// </summary>
        public int? SampleSize { get; set; }

        /// <summary>
        /// The recommended starting items in order
        /// </summary>
        public List<string> StartingItems { get; set; } = new();

        /// <summary>
        /// The recommended core items in order
        /// </summary>
        public List<string> CoreItems { get; set; } = new();

        /// <summary>
        /// Situational item recommendations with conditions
        /// </summary>
        public List<SituationalItem> SituationalItems { get; set; } = new();

        /// <summary>
        /// The skill order recommendation (e.g., Q > W > E)
        /// </summary>
        public string? SkillOrder { get; set; }

        /// <summary>
        /// The summoner spells recommended for this build
        /// </summary>
        public List<string> SummonerSpells { get; set; } = new();

        /// <summary>
        /// Any additional notes or context for this build
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// The rank tier this build is recommended for (if specified)
        /// </summary>
        public string? TargetRank { get; set; }

        /// <summary>
        /// Region this data is from (if region-specific)
        /// </summary>
        public string? Region { get; set; }
    }
}