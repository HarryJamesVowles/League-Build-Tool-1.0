namespace LeagueBuildTool.Core;

public class Item
{
    // Name of the item. e.g., "Infinity Edge", "Rabadon's Deathcap".
    public string Name { get; set; }

    // Cost of the item in gold.
    public int Cost { get; set; }

    // Notes about buffs, lifesteal, or other special effects can be added later.
    public string Description { get; set; }

    // Constructor to initialize an Item object with default values.
    public Item()
    {
        Name = name;
        Cost = cost;
        Description = description;
    }

    public ovverride string ToString()
    {
        return $"{Name} (Cost: {Cost} gold) - {Description}";
    }
}