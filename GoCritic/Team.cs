using System.Collections.Generic;

namespace GoCritic
{
	class Team
	{
		public DemoInfo.Team TeamEnum { get; set; }

		public int Score { get; set; } = 0;

		public List<PlayerStats> Players { get; set; } = new List<PlayerStats>();

		public override string ToString()
		{
			return TeamEnum.ToString();
		}
	}
}
