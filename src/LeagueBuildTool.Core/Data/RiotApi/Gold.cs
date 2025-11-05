namespace LeagueBuildTool.Core.RiotItemData;

// DTO for gold information
public class Gold
{
    public int baseValue { get; set; }       // base cost
    public int total { get; set; }           // total cost
    public int sell { get; set; }            // sell price
    public bool purchasable { get; set; }    // whether item can be bought
}