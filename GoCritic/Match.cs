using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DemoInfo;

namespace GoCritic
{
	class Match
	{
		public string DemoName { get; set; }

		public DateTime Time { get; set; }

		public string Map { get; set; }

		public List<PlayerMatchStats> PlayerStats { get; set; } = new List<PlayerMatchStats>();

		public void Parse(FileInfo fileInfo)
		{
			DemoName = fileInfo.Name;
			Time = fileInfo.LastWriteTimeUtc;
			using (var stream = new FileStream(fileInfo.FullName, FileMode.Open))
			{
				var parser = new DemoParser(stream);
				parser.PlayerKilled += OnPlayerKilled;
				parser.ParseHeader();
				Map = parser.Map;
				parser.ParseToEnd();
			}
		}

		private void OnPlayerKilled(object sender, PlayerKilledEventArgs events)
		{
			if (events.Killer == null || events.Victim == null)
				return;
			var killerStats = GetPlayerStats(events.Killer);
			var victimStats = GetPlayerStats(events.Victim);
			killerStats.Kills++;
			victimStats.Deaths++;
		}

		private PlayerMatchStats GetPlayerStats(Player player)
		{
			var stats = PlayerStats.FirstOrDefault(s => s.SteamId == player.SteamID);
			if (stats == null)
			{
				stats = new PlayerMatchStats(player);
				PlayerStats.Add(stats);
			}
			return stats;
		}
	}
}
