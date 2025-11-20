using System;
using System.Collections.Generic;
using System.Linq;

namespace LeagueBuildTool.Core
{
    /// <summary>
    /// Manages a collection of champions and provides methods for adding and retrieving champions
    /// based on various criteria. This class serves as the central point for champion management
    /// in the League Build Tool.
    /// </summary>
    public class ChampionManager
    {
        /// <summary>
        /// Gets or sets the list of all champions managed by this instance.
        /// </summary>
        public List<Champion> Champions { get; set; }

        /// <summary>
        /// Initializes a new instance of the ChampionManager class.
        /// Creates an empty list to store champions.
        /// </summary>
        public ChampionManager()
        {
            Champions = new List<Champion>();
        }

        /// <summary>
        /// Adds a new champion to the manager's collection.
        /// </summary>
        /// <param name="champion">The champion to add to the collection.</param>
        public void AddChampion(Champion champion)
        {
            Champions.Add(champion);
        }

        /// <summary>
        /// Retrieves a champion by their name, using a case-insensitive search.
        /// </summary>
        /// <param name="name">The name of the champion to find.</param>
        /// <returns>The champion with the specified name, or null if no champion is found.</returns>
        public Champion? GetChampionByName(string name)
        {
            return Champions.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Retrieves all champions that are assigned to a specific lane.
        /// </summary>
        /// <param name="lane">The lane to filter champions by (e.g., "Mid", "Top", "Jungle").</param>
        /// <returns>A list of champions assigned to the specified lane.</returns>
        public List<Champion> GetChampionsByLane(string lane)
        {
            return Champions.Where(c => c.Lane.Equals(lane, StringComparison.OrdinalIgnoreCase)).ToList();
        }

    }
}