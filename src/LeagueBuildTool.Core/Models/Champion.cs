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
    /// Canonical base stats for the champion. Keys represent stat names and values represent the
    /// numeric base value for that stat. This dictionary is guaranteed to contain entries for
    /// the canonical stat keys defined in <see cref="RequiredBaseStatKeys"/> (defaults to 0 when missing).
    /// Examples of keys: "Health", "AttackDamage", "AbilityPower", "Armor", "MagicResist", "AttackSpeed".
    /// </summary>
    public Dictionary<string, double> BaseStats { get; set; }

    /// <summary>
    /// Tags that describe the champion's playstyle or primary attributes (e.g., "Assassin", "Mage", "AD").
    /// These are populated from Riot's champion data <c>tags</c> field when available.
    /// </summary>
    public List<string> Tags { get; set; }

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
        // Initialize base stats with canonical keys so callers can rely on their presence.
        BaseStats = new Dictionary<string, double>();
        foreach (var key in RequiredBaseStatKeys)
        {
            BaseStats[key] = 0.0;
        }
        Tags = new List<string>();
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

    /// <summary>
    /// Canonical list of base stat keys every Champion should have in <see cref="BaseStats"/>.
    /// Add to this list if additional, required base stats are needed project-wide.
    /// </summary>
    public static readonly string[] RequiredBaseStatKeys = new[]
    {
        "Health",
        "AttackDamage",
        "AbilityPower",
        "Armor",
        "MagicResist",
        "AttackSpeed",
        "MoveSpeed",
        "HealthRegen",
        "Mana",
        "ManaRegen"
    };

}
