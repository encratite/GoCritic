using DemoInfo;

namespace GoCritic
{
	class PlayerMatchStats
	{
		public string Name { get; set; }
		public long SteamId { get; set; }

		public int Kills { get; set; } = 0;
		public int Deaths { get; set; } = 0;

		public PlayerMatchStats()
		{
		}

		public PlayerMatchStats(Player player)
		{
			Name = player.Name;
			SteamId = player.SteamID;
		}
	}
}
