using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LeagueBuildTool.Core;
using LeagueBuildTool.Core.Configuration;
using LeagueBuildTool.Core.Services;
using Microsoft.Extensions.Configuration;

// Small sample runner used to inspect a few champions and items from Riot Data Dragon.
// This is intentionally lightweight and meant only for local inspection to help
// design normalization rules for stats and tags.
await RunSampleAsync();

async Task RunSampleAsync()
{
	try
	{
		Console.WriteLine("Fetching sample champions...");
		var champs = await RiotChampionFetcher.GetAllChampionsAsync();
		Console.WriteLine($"Total champions fetched: {champs.Count}");
		// aggregated stats for champions
		var champStatCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		var champTagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		foreach (var c in champs)
		{
			foreach (var kv in c.BaseStats)
			{
				if (kv.Value != 0) champStatCounts[kv.Key] = champStatCounts.GetValueOrDefault(kv.Key, 0) + 1;
			}
			foreach (var t in c.Tags ?? new System.Collections.Generic.List<string>())
			{
				champTagCounts[t] = champTagCounts.GetValueOrDefault(t, 0) + 1;
			}
		}

		foreach (var c in champs.Take(5))
		{
			Console.WriteLine($"--- Champion: {c.Name}");
			Console.WriteLine($"Tags: {(c.Tags?.Count > 0 ? string.Join(", ", c.Tags) : "(none)")}");
			Console.WriteLine("BaseStats sample:");
			foreach (var key in c.BaseStats.Keys.Take(6))
			{
				Console.WriteLine($"  {key}: {c.BaseStats[key]}");
			}
			Console.WriteLine();
		}

		Console.WriteLine("\nChampion stat keys (non-zero counts):");
		foreach (var kv in champStatCounts.OrderByDescending(k => k.Value).Take(20))
		{
			Console.WriteLine($"  {kv.Key}: {kv.Value}");
		}

		Console.WriteLine("\nChampion tag distribution (top):");
		foreach (var kv in champTagCounts.OrderByDescending(k => k.Value).Take(20))
		{
			Console.WriteLine($"  {kv.Key}: {kv.Value}");
		}

		Console.WriteLine("\nFetching sample items...");
		var items = await RiotItemFetcher.GetAllItemsAsync();
		Console.WriteLine($"Total items fetched: {items.Count}");
		foreach (var it in items.Take(5))
		{
			Console.WriteLine($"--- Item: {it.Name} (Cost: {it.Cost})");
			Console.WriteLine($"Tags: {(it.Tags?.Count > 0 ? string.Join(", ", it.Tags) : "(none)")}");
			if (it.Stats != null && it.Stats.Count > 0)
			{
				Console.WriteLine("Stats sample:");
				foreach (var kv in it.Stats.Take(6))
				{
					Console.WriteLine($"  {kv.Key}: {kv.Value}");
				}
			}
			else
			{
				Console.WriteLine("Stats: (none)");
			}
			Console.WriteLine();
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine("Error while fetching samples: " + ex.Message);
	}
}

