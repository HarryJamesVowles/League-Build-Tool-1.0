using System.Collections.Generic;

namespace LeagueBuildTool.Core.RiotChampionData;

// DTO to deserialize Riot JSON for a single champion
public class RiotChampion
{
    public string? id { get; set; }
    public string? name { get; set; }
    public string? title { get; set; }
    public string[]? tags { get; set; }
    public Dictionary<string, double>? stats { get; set; }
}
