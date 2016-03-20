using DemoInfo;

namespace GoCritic
{
	class PlayerStats
	{
		public string Name { get; set; }
		public long SteamId { get; set; }

		public int Kills { get; set; } = 0;
		public int Deaths { get; set; } = 0;

		public PlayerStats()
		{
		}

		public PlayerStats(Player player)
		{
			Name = player.Name;
			SteamId = player.SteamID;
		}
	}
}
