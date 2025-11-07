namespace LeagueBuildTool.Core;

/// <summary>
/// Represents a League of Legends item with its basic properties.
/// This class serves as our internal representation of items, independent of Riot's data format.
/// </summary>
public class Item
{
    /// <summary>
    /// The name of the item as it appears in the game.
    /// Examples: "Infinity Edge", "Rabadon's Deathcap", "Trinity Force"
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The total cost of the item in gold.
    /// This represents the full purchase price, not considering build path or sell value.
    /// </summary>
    public int Cost { get; set; }

    /// <summary>
    /// A brief description of the item's effects and purpose.
    /// This is typically the plain text description from the game, excluding exact stat values.
    /// Future versions will include detailed stat modifiers and unique passive/active effects.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// A dictionary of stat keys to their numeric modifier values provided by this item.
    /// Keys follow Riot's JSON stat names where possible (e.g. "FlatHPPoolMod", "FlatPhysicalDamageMod").
    /// </summary>
    public Dictionary<string, double> Stats { get; set; }

    /// <summary>
    /// A list of tags describing which stats or playstyles this item affects (e.g. "AD", "AP", "Tank").
    /// Derived from the <see cref="Stats"/> keys when available.
    /// </summary>
    public List<string> Tags { get; set; }

    /// <summary>
    /// Initializes a new Item with specified properties.
    /// </summary>
    /// <param name="name">The name of the item</param>
    /// <param name="cost">The cost in gold</param>
    /// <param name="description">A brief description of the item's effects</param>
    public Item(string name, int cost, string description)
    {
        Name = name;
        Cost = cost;
        Description = description;
        Stats = new Dictionary<string, double>();
        Tags = new List<string>();
    }

    /// <summary>
    /// Initializes a new Item with default values.
    /// Provided for serialization and framework compatibility.
    /// </summary>
    public Item()
    {
        Name = string.Empty;
        Cost = 0;
        Description = string.Empty;
        Stats = new Dictionary<string, double>();
        Tags = new List<string>();
    }

    /// <summary>
    /// Returns a string representation of the item, including its name, cost, and description.
    /// </summary>
    /// <returns>A formatted string containing the item's details</returns>
    public override string ToString()
    {
        return $"{Name} (Cost: {Cost} gold) - {Description}";
    }
}
