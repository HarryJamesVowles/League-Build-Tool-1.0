// This defines a namespace to organise related classes.
namespace LeagueBuildTool.Core;

//
public class Champion
{
    // Properties define data each Champion object will hold.
    
    // Name of the champion. e.g., "Ahri", "Garen".
    public string Name { get; set; }

    // Lane of the champion. e.g., "Mid", "Top", "Jungle".
    public string Lane { get; set; }

    // Health points of the champion.
    public int Health { get; set; }

    // Attack damage of the champion.
    public int AttackDamage { get; set; }

    // Ability power of the champion.
    public int AbilityPower { get; set; }

    //Current gold available.
    public int Gold { get; set; }

    // List of items in champions current build.
    public List<Item> Build { get; set; }

    // Constructor to initialize a Champion object with default values.
    public Champion()
    {
        // defult values for a new champion
        Name = string.Empty;
        Lane = string.Empty;
        Health = 0;
        AttackDamage = 0;
        AbilityPower = 0;
        Gold = 0;
        Build = new List<Item>();
    }

    // Method to add an item to the champion's build.
    public void AddItemToBuild(Item item)
    {
        Build.Add(item);
    }

}
