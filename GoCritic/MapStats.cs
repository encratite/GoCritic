namespace GoCritic
{
	class MapStats
	{
		public string Name { get; private set; }

		public int Wins { get; set; } = 0;
		public int Losses { get; set; } = 0;
		public int Draws { get; set; } = 0;

		public int RoundsWon { get; set; } = 0;
		public int RoundsLost { get; set; } = 0;

		public int Kills { get; set; } = 0;
		public int Deaths { get; set; } = 0;

		public int Games { get { return Wins + Losses + Draws; } }

		public int Rounds { get { return RoundsWon + RoundsLost; } }

		public decimal? KillsPerRound { get { return GetRatio(Kills, Rounds); } }
		public decimal? DeathsPerRound { get { return GetRatio(Deaths, Rounds); } }

		public MapStats(string name)
		{
			Name = name;
		}

		private decimal? GetRatio(int numerator, int denominator)
		{
			if (denominator == 0)
				return null;
			return (decimal)numerator / denominator;
		}
	}
}
