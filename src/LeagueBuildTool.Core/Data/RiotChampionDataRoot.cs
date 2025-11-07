using System.Collections.Generic;
using LeagueBuildTool.Core.RiotChampionData;

namespace LeagueBuildTool.Core;

/// <summary>
/// Represents the root structure of the Riot champion list JSON.
/// Maps the Data Dragon champion.json response to a C# object.
/// </summary>
public class RiotChampionDataRoot
{
    public string type { get; set; } = string.Empty;
    public string version { get; set; } = string.Empty;
    public Dictionary<string, RiotChampion> data { get; set; } = new Dictionary<string, RiotChampion>();
}
