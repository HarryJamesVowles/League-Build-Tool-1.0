using System.Collections.Generic;
using LeagueBuildTool.Core.RiotItemData;

namespace LeagueBuildTool.Core;

/// <summary>
/// Represents the root structure of the Riot Games Data Dragon item data JSON response.
/// This class maps directly to the JSON structure returned by the Riot Games API
/// when fetching item data.
/// </summary>
public class RiotItemDataRoot
{
    /// <summary>
    /// Gets or sets the type of data being returned.
    /// Typically contains "item" for item data responses.
    /// </summary>
    public string type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the Data Dragon API being used.
    /// Format is typically "X.Y.Z" (e.g., "13.24.1").
    /// </summary>
    public string version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dictionary containing all item data.
    /// The key is the item ID (as a string), and the value is the corresponding RiotItem object
    /// containing all the item's details.
    /// </summary>
    public Dictionary<string, RiotItem> data { get; set; } = new Dictionary<string, RiotItem>();
}
