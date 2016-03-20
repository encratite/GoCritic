namespace GoCritic
{
	class MapStats
	{
		private const decimal AverageKPR = 0.679M;
		private const decimal AverageSPR = 0.317M;
		private const decimal AverageRMK = 1.277M;

		public string Map { get; private set; }

		public int Wins { get; set; } = 0;
		public int Losses { get; set; } = 0;
		public int Draws { get; set; } = 0;

		public int RoundsWon { get; set; } = 0;
		public int RoundsLost { get; set; } = 0;

		public int Kills { get; set; } = 0;
		public int Deaths { get; set; } = 0;

		public int[] MultiKills { get; set; } = new int[PlayerStats.PlayersPerTeam];

		public int Games { get { return Wins + Losses + Draws; } }

		public int Rounds { get { return RoundsWon + RoundsLost; } }

		public decimal? KillsPerRound { get { return (decimal)Kills / Rounds; } }
		public decimal? DeathsPerRound { get { return (decimal)Deaths / Rounds; } }

		public decimal KillRating { get { return (decimal)Kills / Rounds / AverageKPR; } }
		public decimal SurvivalRating { get { return (decimal)(Rounds - Deaths) / Rounds / AverageSPR; } }
		public decimal RoundsWithMultipleKillsRating
		{
			get
			{
				int numerator = MultiKills[0] + 4 * MultiKills[1] + 9 * MultiKills[2] + 16 * MultiKills[3] + 25 * MultiKills[4];
				return numerator / Rounds / AverageRMK;
			}
		}

		public decimal Rating { get { return (KillRating + 0.7M * SurvivalRating + RoundsWithMultipleKillsRating) / 2.7M; } }

		public MapStats(string name)
		{
			Map = name;
		}

		public override string ToString()
		{
			return Map;
		}
	}
}
