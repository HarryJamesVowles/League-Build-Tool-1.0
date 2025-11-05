namespace LeagueBuildTool.Core;

/// <summary>
/// Represents a League of Legends champion with their core attributes and build information.
/// This class encapsulates all the relevant data and functionality for managing a champion's stats and items.
/// </summary>
public class Champion
{
    /// <summary>
    /// Gets or sets the name of the champion (e.g., "Ahri", "Garen").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the preferred lane position for the champion (e.g., "Mid", "Top", "Jungle").
    /// </summary>
    public string Lane { get; set; }

    /// <summary>
    /// Gets or sets the champion's health points, representing their maximum HP.
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// Gets or sets the champion's attack damage (AD) value.
    /// </summary>
    public int AttackDamage { get; set; }

    /// <summary>
    /// Gets or sets the champion's ability power (AP) value.
    /// </summary>
    public int AbilityPower { get; set; }

    /// <summary>
    /// Gets or sets the current amount of gold available to the champion.
    /// </summary>
    public int Gold { get; set; }

    /// <summary>
    /// Gets or sets the list of items in the champion's current build.
    /// </summary>
    public List<Item> Build { get; set; }

    /// <summary>
    /// Initializes a new instance of the Champion class with default values.
    /// All numeric properties are set to 0, string properties to empty strings,
    /// and initializes an empty build list.
    /// </summary>
    public Champion()
    {
        Name = string.Empty;
        Lane = string.Empty;
        Health = 0;
        AttackDamage = 0;
        AbilityPower = 0;
        Gold = 0;
        Build = new List<Item>();
    }

    /// <summary>
    /// Adds an item to the champion's current build list.
    /// </summary>
    /// <param name="item">The item to add to the champion's build.</param>
    /// <remarks>
    /// This method appends the item to the end of the build list.
    /// Note that this does not automatically apply the item's stats to the champion.
    /// </remarks>
    public void AddItemToBuild(Item item)
    {
        Build.Add(item);
    }

}
