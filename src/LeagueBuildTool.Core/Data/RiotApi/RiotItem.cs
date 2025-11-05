using System.Collections.Generic;

namespace LeagueBuildTool.Core.RiotItemData;

// DTO to deserialize Riot JSON for a single item
public class RiotItem
{
    public string? name { get; set; }         // item name
    public string? plaintext { get; set; }   // short description
    public Gold? gold { get; set; }          // gold info
    public Dictionary<string, double>? stats { get; set; } // item stats like AD, AP, etc.
}