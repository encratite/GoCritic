using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace GoCritic
{
	class DemoManager
	{
		private const string CacheFile = "Cache.json";

		private List<Match> _Matches = new List<Match>();

		public void LoadCache()
		{
			if (!File.Exists(CacheFile))
				return;
			string json = File.ReadAllText(CacheFile);
			_Matches = JsonConvert.DeserializeObject<List<Match>>(json);
		}

		public void SaveCache()
		{
			var settings = new JsonSerializerSettings
			{
				Formatting = Newtonsoft.Json.Formatting.Indented
			};
			string json = JsonConvert.SerializeObject(_Matches, settings);
			string tempPath = Path.GetTempFileName();
			File.WriteAllText(tempPath, json);
			if (File.Exists(CacheFile))
				File.Delete(CacheFile);
			File.Move(tempPath, CacheFile);
		}

		public void ParseDemos()
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			string demoPath = GetDemoPath();
			var directory = new DirectoryInfo(demoPath);
			var files = directory.GetFiles("*.dem");
			int count = 0;
			var matchingFiles = files.Where(file => !_Matches.Any(m => m.DemoName == file.Name)).ToList();
			foreach (var file in matchingFiles)
			{
				count++;
				Console.WriteLine($"Parsing demo {file.Name} ({count}/{matchingFiles.Count})");
				var parser = new DemoParser();
				var match = parser.Parse(file);
				_Matches.Add(match);
				SaveCache();
			}
            stopwatch.Stop();
			if (count > 0)
				Console.WriteLine($"Parsed {count} new demo(s) in {stopwatch.Elapsed.TotalSeconds:F1} s");
		}

		public void PrintStats()
		{
			var mapStats = GetMapStats();
			foreach (var map in mapStats)
			{
				var originalColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(map.Name);
				Console.ForegroundColor = originalColor;
				Console.Write($"W/D/L: {map.Wins}/{map.Draws}/{map.Losses} (");
				int difference = map.Wins - map.Losses;
				if (difference > 0)
					Console.ForegroundColor = ConsoleColor.Green;
				else if (difference < 0)
					Console.ForegroundColor = ConsoleColor.Red;
				else
					Console.ForegroundColor = originalColor;
				Console.Write(difference.ToString("+#;-#;0"));
                Console.ForegroundColor = originalColor;
				Console.WriteLine(")");
				Console.WriteLine($"Round averages: {map.KillsPerRound:F2} kills, {map.DeathsPerRound:F2} deaths");
            }
		}

		private long GetSteamId()
		{
			string name = GetRegistryString("LastGameNameUsed");
			var player = _Matches.SelectMany(m => m.Teams).SelectMany(t => t.Players).FirstOrDefault(p => p.Name == name);
			if (player == null)
				throw new ApplicationException("Unable to find any demos with your current player name.");
			return player.SteamId;
		}

		private string GetDemoPath()
		{
			string steamPath = GetRegistryString("SteamPath");
			string demoPath = Path.Combine(steamPath, @"SteamApps\common\Counter-Strike Global Offensive\csgo\replays");
			return demoPath;
		}

		private string GetRegistryString(string name)
		{
			using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
			{
				using (var key = baseKey.OpenSubKey(@"Software\Valve\Steam"))
				{
					string value = key.GetValue(name) as string;
					if (value == null)
						throw new ApplicationException("Unable to read registry.");
					return value;
				}
			}
		}

		private List<MapStats> GetMapStats()
		{
			long steamId = GetSteamId();
			var mapStats = new List<MapStats>();
			foreach (var match in _Matches)
			{
				if (match.Teams.Count != 2)
					continue;
				var player = match.Teams.SelectMany(t => t.Players).FirstOrDefault(p => p.SteamId == steamId);
				if (player == null)
					continue;
				var stats = mapStats.FirstOrDefault(s => s.Name == match.Map);
				if (stats == null)
				{
					stats = new MapStats(match.Map);
					mapStats.Add(stats);
				}
				var team1 = match.Teams[0];
				var team2 = match.Teams[1];
				bool isOnTeam1 = team1.Players.Contains(player);
				if (team1.Score == team2.Score)
					stats.Draws++;
				else if (
					team1.Score > team2.Score && isOnTeam1 ||
					team2.Score > team1.Score && !isOnTeam1
				)
					stats.Wins++;
				else
					stats.Losses++;
				stats.Kills += player.Kills;
				stats.Deaths += player.Deaths;
				stats.RoundsWon += (isOnTeam1 ? team1 : team2).Score;
				stats.RoundsLost += (isOnTeam1 ? team2 : team1).Score;
			}
			mapStats = mapStats.OrderByDescending(m => m.Games).ToList();
			return mapStats;
		}
	}
}
