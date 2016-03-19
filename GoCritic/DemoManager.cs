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
		private const string CacheBackupFile = "CacheBackup.json";

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
			foreach (var file in files)
			{
				if (_Matches.Any(m => m.DemoName == file.Name))
					continue;
				Console.WriteLine($"Parsing demo {file.Name}");
				var match = new Match();
				match.Parse(file);
				_Matches.Add(match);
				SaveCache();
				count++;
			}
            stopwatch.Stop();
			if (count > 0)
				Console.WriteLine($"Parsed {count} new demo(s) in {stopwatch.Elapsed.TotalSeconds:F1} s");
			else
				Console.WriteLine("Detected no new demos");
		}

		private string GetDemoPath()
		{
			using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
			{
				using (var key = baseKey.OpenSubKey(@"Software\Valve\Steam"))
				{
					string steamPath = key.GetValue("SteamPath") as string;
					if (steamPath == null)
						throw new ApplicationException("Unable to find Steam installation.");
					string demoPath = Path.Combine(steamPath, @"SteamApps\common\Counter-Strike Global Offensive\csgo\replays");
					return demoPath;
				}
			}
		}
	}
}
