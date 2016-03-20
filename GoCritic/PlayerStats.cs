using DemoInfo;

namespace GoCritic
{
	class PlayerStats
	{
		public const int PlayersPerTeam = 5;

		public string Name { get; set; }
		public long SteamId { get; set; }

		public int Kills { get; set; } = 0;
		public int Deaths { get; set; } = 0;

		public int[] MultiKills { get; set; } = new int[PlayersPerTeam];

		private int _CurrentRoundKills = 0;

		public PlayerStats()
		{
		}

		public PlayerStats(Player player)
		{
			Name = player.Name;
			SteamId = player.SteamID;
		}

		public void OnRoundEnd()
		{
			if (_CurrentRoundKills > 0)
			{
				MultiKills[_CurrentRoundKills - 1]++;
			}
			_CurrentRoundKills = 0;
		}

		public void OnRoundKill()
		{
			Kills++;
			if (_CurrentRoundKills < MultiKills.Length)
				_CurrentRoundKills++;
		}
	}
}
