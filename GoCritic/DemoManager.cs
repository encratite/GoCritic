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

		public void PrintStats(List<string> steamIDFilters)
		{
			var mapStats = GetMapStats(steamIDFilters);
			foreach (var map in mapStats)
			{
				var originalColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(map.Map);
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
				Console.WriteLine($"Rating: {map.Rating:F3}");
            }
		}

		private long GetSteamID()
		{
			string name = GetRegistryString("LastGameNameUsed");
			var player = _Matches.SelectMany(m => m.Teams).SelectMany(t => t.Players).FirstOrDefault(p => p.Name == name);
			if (player == null)
				throw new ApplicationException("Unable to find any demos with your current player name.");
			return player.SteamID;
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

		private List<MapStats> GetMapStats(List<string> steamIDFilters)
		{
			long steamID = GetSteamID();
			var mapStats = new List<MapStats>();
			var allMaps = new MapStats("[All maps]");
			foreach (var match in _Matches)
			{
				ProcessMatch(match, steamID, steamIDFilters, mapStats);
				ProcessMatch(match, steamID, steamIDFilters, mapStats, allMaps);
			}
			mapStats = mapStats.OrderByDescending(m => m.Games).ToList();
			mapStats.Add(allMaps);
			return mapStats;
		}

		private void ProcessMatch(Match match, long steamID, List<string> steamIDFilters, List<MapStats> mapStats, MapStats statsOverride = null)
		{
			if (match.Teams.Count != 2)
				return;
			var player = match.Teams.SelectMany(t => t.Players).FirstOrDefault(p => p.SteamID == steamID);
			if (player == null)
				return;
			var team1 = match.Teams[0];
			var team2 = match.Teams[1];
			bool isOnTeam1 = team1.Players.Contains(player);
			var playerTeam = isOnTeam1 ? team1 : team2;
			var enemyTeam = isOnTeam1 ? team2 : team1;
			var inclusiveSet = new HashSet<long>();
			var exclusiveSet = new HashSet<long>();
			foreach (string filter in steamIDFilters)
			{
				if (filter[0] != '!')
					inclusiveSet.Add(long.Parse(filter));
				else
					exclusiveSet.Add(long.Parse(filter.Substring(1)));
			}
			var teamSet = new HashSet<long>(playerTeam.Players.Select(p => p.SteamID));
			if (!inclusiveSet.IsSubsetOf(teamSet) || exclusiveSet.Intersect(teamSet).Any())
				return;
			var stats = statsOverride ?? mapStats.FirstOrDefault(s => s.Map == match.Map);
			if (stats == null)
			{
				stats = new MapStats(match.Map);
				mapStats.Add(stats);
			}
			if (playerTeam.Score > enemyTeam.Score)
				stats.Wins++;
			else if (playerTeam.Score < enemyTeam.Score)
				stats.Losses++;
			else
				stats.Draws++;
			stats.Kills += player.Kills;
			stats.Deaths += player.Deaths;
			for (int i = 0; i < player.MultiKills.Length; i++)
				stats.MultiKills[i] += player.MultiKills[i];
			stats.RoundsWon += playerTeam.Score;
			stats.RoundsLost += enemyTeam.Score;
		}
	}
}
