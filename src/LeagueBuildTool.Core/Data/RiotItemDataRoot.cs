using System.Collections.Generic;
using LeagueBuildTool.Core.RiotItemData;

namespace LeagueBuildTool.Core;

// Class representing the root structure of Riot item data JSON
public class RiotItemDataRoot
    {
        public string type { get; set; } = string.Empty;
        public string version { get; set; } = string.Empty;
        public Dictionary<string, RiotItem> data { get; set; } = new Dictionary<string, RiotItem>();
    }
