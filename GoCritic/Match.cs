using System.Collections.Generic;

namespace GoCritic
{
	class Match
	{
		public string DemoName { get; set; }
		public string Map { get; set; }

		public List<Team> Teams { get; set; } = new List<Team>();
	}
}
