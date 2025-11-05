namespace LeagueBuildTool.Core;

public class Item
{
    // Name of the item. e.g., "Infinity Edge", "Rabadon's Deathcap".
    public string Name { get; set; }

    // Cost of the item in gold.
    public int Cost { get; set; }

    // Notes about buffs, lifesteal, or other special effects can be added later.
    public string Description { get; set; }

    // Constructor to initialize an Item object with specified values
    public Item(string name, int cost, string description)
    {
        Name = name;
        Cost = cost;
        Description = description;
    }

    // Optional parameterless constructor with default values
    public Item()
    {
        Name = string.Empty;
        Cost = 0;
        Description = string.Empty;
    }

    // Override ToString to display item nicely
    public override string ToString()
    {
        return $"{Name} (Cost: {Cost} gold) - {Description}";
    }
}
