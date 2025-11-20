using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LeagueBuildTool.Core;
using LeagueBuildTool.Core.Configuration;
using LeagueBuildTool.Core.Services;

// Entry point for League Build Tool with dependency injection and configuration

// Build host with configuration and services
var host = Host.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration((context, config) =>
	{
		// Add configuration sources in order of priority
		config.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
			.AddEnvironmentVariables()
			.AddUserSecrets<Program>(optional: true);
	})
	.ConfigureServices((context, services) =>
	{
		// Bind RiotApi configuration section
		var riotConfig = context.Configuration.GetSection("RiotApi").Get<RiotApiConfiguration>() 
			?? new RiotApiConfiguration();
		
		// Register configuration as singleton
		services.AddSingleton(riotConfig);
		
		// Register HttpClient
		services.AddHttpClient<RiotChampionFetcher>();
		services.AddHttpClient<RiotItemFetcher>();
	})
	.Build();

// Run the sample inspection
await RunSampleAsync(host.Services);

async Task RunSampleAsync(IServiceProvider services)
{
	try
	{
		// Get services from DI container
		var championFetcher = services.GetRequiredService<RiotChampionFetcher>();
		var itemFetcher = services.GetRequiredService<RiotItemFetcher>();
		
		// Fetch sample champions from Riot API
		Console.WriteLine("Fetching sample champions...");
		var champs = await championFetcher.GetAllChampionsAsync();
		Console.WriteLine($"Total champions fetched: {champs.Count}");

		// Aggregate stats and tags for champions
		var champStatCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		var champTagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		foreach (var c in champs)
		{
			// Count non-zero stats
			foreach (var kv in c.BaseStats)
			{
				if (kv.Value != 0)
					champStatCounts[kv.Key] = champStatCounts.GetValueOrDefault(kv.Key, 0) + 1;
			}
			// Count tags
			foreach (var t in c.Tags ?? new System.Collections.Generic.List<string>())
			{
				champTagCounts[t] = champTagCounts.GetValueOrDefault(t, 0) + 1;
			}
		}

		// Print sample champion details
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

		// Print aggregated stat counts
		Console.WriteLine("\nChampion stat keys (non-zero counts):");
		foreach (var kv in champStatCounts.OrderByDescending(k => k.Value).Take(20))
		{
			Console.WriteLine($"  {kv.Key}: {kv.Value}");
		}

		// Print tag distribution
		Console.WriteLine("\nChampion tag distribution (top):");
		foreach (var kv in champTagCounts.OrderByDescending(k => k.Value).Take(20))
		{
			Console.WriteLine($"  {kv.Key}: {kv.Value}");
		}

		// Fetch sample items from Riot API
		Console.WriteLine("\nFetching sample items...");
		var items = await itemFetcher.GetAllItemsAsync();
		Console.WriteLine($"Total items fetched: {items.Count}");
		foreach (var it in items.Take(5))
		{
			Console.WriteLine($"--- Item: {it.Name} (Cost: {it.Cost})");
			Console.WriteLine($"Tags: {(it.Tags?.Count > 0 ? string.Join(", ", it.Tags) : "(none)")}");
			// Print item stats sample
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
		// Print error if sample fetch fails
		Console.WriteLine("Error while fetching samples: " + ex.Message);
	}
}

