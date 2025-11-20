using System.Collections.Generic;

namespace LeagueBuildTool.Core.Data.RiotApi
{
    public class SpectatorGameInfo
    {
        public long GameId { get; set; }
        public string GameMode { get; set; }
        public string GameType { get; set; }
        public long GameQueueConfigId { get; set; }
        public List<CurrentGameParticipant> Participants { get; set; }
    }

    public class CurrentGameParticipant
    {
        public long ChampionId { get; set; }
        public long TeamId { get; set; }
        public string SummonerName { get; set; }
        public string SummonerId { get; set; }
        public long Spell1Id { get; set; }
        public long Spell2Id { get; set; }
        public Perks Perks { get; set; }
    }

    public class Perks
    {
        public List<long> PerkIds { get; set; }
        public long PerkStyle { get; set; }
        public long PerkSubStyle { get; set; }
    }
