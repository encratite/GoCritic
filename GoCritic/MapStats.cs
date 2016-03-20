namespace GoCritic
{
	class MapStats
	{
		public string Name { get; private set; }

		public int Wins { get; set; } = 0;
		public int Losses { get; set; } = 0;

		public int RoundsWon { get; set; } = 0;
		public int RoundsLost { get; set; } = 0;

		public int Kills { get; set; } = 0;
		public int Deaths { get; set; } = 0;

		public MapStats(string name)
		{
			Name = name;
		}
	}
}
