using System;
using System.Collections.Generic;
using System.Linq;

namespace LeagueBuildTool.Core.Utils;

/// <summary>
/// Utility to normalize Riot stat keys into canonical project stat keys and derive high-level tags.
/// </summary>
public static class StatMapper
{
    // mapping heuristics from riot keys to canonical keys
    private static readonly (string[] patterns, string canonical)[] KeyMap = new[]
    {
        (new[] { "flathppoolmod", "hp", "hp" }, "Health"),
        (new[] { "attackdamage", "flatphysicaldmgmod", "flatphysicaldamage" , "physicaldamage" }, "AttackDamage"),
        (new[] { "abilitypower", "ap", "spelldamage" }, "AbilityPower"),
        (new[] { "armor" }, "Armor"),
        (new[] { "spellblock", "magicresist", "mr" }, "MagicResist"),
        (new[] { "attackspeed", "attackspeed" }, "AttackSpeed"),
        (new[] { "flatmovementspeedmod", "movespeed", "movement" }, "MoveSpeed"),
        (new[] { "hpregen", "flathpregenmod" }, "HealthRegen"),
        (new[] { "mp", "mana" }, "Mana"),
        (new[] { "mpregen", "manaregen" }, "ManaRegen"),
        (new[] { "flatcritchancemod", "crit" }, "CritChance"),
        (new[] { "flatcritdamagemod" , "critdamage" }, "CritDamage")
    };

    // common resource detection patterns mapped to resource name
    private static readonly (string[] patterns, string resource)[] ResourceMap = new[]
    {
        (new[] { "mp", "mana" }, "Mana"),
        (new[] { "energy" }, "Energy"),
        (new[] { "fury", "rage" }, "Fury"),
        (new[] { "heat" }, "Heat"),
        (new[] { "chi" }, "Chi"),
        (new[] { "blood" , "bloodwell" }, "BloodWell")
    };

    /// <summary>
    /// Normalize a Riot stat key into a canonical project stat key.
    /// If no mapping is found, returns the original key.
    /// </summary>
    public static string NormalizeStatKey(string riotKey)
    {
        if (string.IsNullOrWhiteSpace(riotKey)) return riotKey;
        var rk = riotKey.ToLowerInvariant();
        foreach (var (patterns, canonical) in KeyMap)
        {
            foreach (var p in patterns)
            {
                if (rk.Contains(p)) return canonical;
            }
        }
        // no mapping found — return original key for traceability
        return riotKey;
    }

    /// <summary>
    /// Derive human-friendly tags from a dictionary of normalized stats.
    /// Tags include AD/AP/Tank/MS/Crit/AS etc.
    /// </summary>
    public static List<string> DeriveTagsFromStats(IDictionary<string, double> stats)
    {
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (stats == null) return tags.ToList();

        // AD vs AP: which one has larger magnitude
        double ad = GetStatValue(stats, "AttackDamage");
        double ap = GetStatValue(stats, "AbilityPower");
        if (ad > 0 || stats.Keys.Any(k => k.IndexOf("attack", StringComparison.OrdinalIgnoreCase) >= 0)) tags.Add("AD");
        if (ap > 0 || stats.Keys.Any(k => k.IndexOf("abilitypower", StringComparison.OrdinalIgnoreCase) >= 0) || stats.Keys.Any(k => k.IndexOf("ap", StringComparison.OrdinalIgnoreCase) >= 0)) tags.Add("AP");

        // Tank if large health/armor/mr
        if (GetStatValue(stats, "Health") >= 100 || GetStatValue(stats, "Armor") >= 30 || GetStatValue(stats, "MagicResist") >= 30) tags.Add("Tank");

        if (GetStatValue(stats, "MoveSpeed") > 0) tags.Add("MS");
        if (GetStatValue(stats, "AttackSpeed") > 0) tags.Add("AS");
        if (GetStatValue(stats, "CritChance") > 0) tags.Add("Crit");

    // resource tag inference: if Mana exists, tag as Mana
    if (GetStatValue(stats, "Mana") > 0) tags.Add("Resource:Mana");

        return tags.ToList();
    }

    /// <summary>
    /// Detects resource types from raw riot stat keys or raw key names.
    /// Returns tags like "Resource:Mana", "Resource:Energy", etc.
    /// </summary>
    public static List<string> DetectResourcesFromRawKeys(IEnumerable<string> rawKeys)
    {
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (rawKeys == null) return tags.ToList();
        foreach (var rk in rawKeys)
        {
            if (string.IsNullOrWhiteSpace(rk)) continue;
            var lower = rk.ToLowerInvariant();
            foreach (var (patterns, resource) in ResourceMap)
            {
                foreach (var p in patterns)
                {
                    if (lower.Contains(p)) tags.Add($"Resource:{resource}");
                }
            }
        }

        return tags.ToList();
    }

    private static double GetStatValue(IDictionary<string, double> stats, string key)
    {
        if (stats.TryGetValue(key, out var v)) return v;
        // sometimes un-normalized keys may exist; try case-insensitive match
        var match = stats.FirstOrDefault(kv => string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase));
        return match.Value;
    }
}
