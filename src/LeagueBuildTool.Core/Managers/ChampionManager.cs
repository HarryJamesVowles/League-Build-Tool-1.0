using System;
using System.Collections.Generic;
using System.Linq;

namespace LeagueBuildTool.Core
{
    public class ChampionManager
    {
       // List to hold all champions.
        public List<Champion> Champions { get; set; }

        // Constructor to initialize the ChampionManager with an empty list of champions.
        public ChampionManager()
        {
            Champions = new List<Champion>();
        }
        // Method to add a new champion to the manager.
        public void AddChampion(Champion champion)
        {
            Champions.Add(champion);
        }

        // Method to retrieve a champion by name (case-insensitive).
        public Champion GetChampionByName(string name)
        {
            return Champions.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        // Method to list all champions for a specific lane.
        public List<Champion> GetChampionsByLane(string lane)
        {
            return Champions.Where(c => c.Lane.Equals(lane, StringComparison.OrdinalIgnoreCase)).ToList();
        }

    }
}