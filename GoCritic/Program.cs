using System;
using System.Collections.Generic;
using System.Linq;

namespace GoCritic
{
	class Program
	{
		private static void Main(string[] arguments)
		{
			if (arguments.Length == 0)
			{
				Console.WriteLine("Commands supported:");
				Console.WriteLine(string.Empty);
				Console.WriteLine("  update               Update demo cache. May take a couple of minutes.");
				Console.WriteLine("                       You must run this command the first time you use");
				Console.WriteLine("                       this tool.");
				Console.WriteLine(string.Empty);
				Console.WriteLine("  stats [SteamIDs]     Print map statistics. Specify additional SteamIDs");
				Console.WriteLine("                       to only include games in which you played");
				Console.WriteLine("                       with those particular players.");
				Console.WriteLine("                       Requires the 64-bit integer format of the SteamID");
				Console.WriteLine("                       (e.g. 76xxxxxxxxxxxxxxx). Add an exclamation mark");
				Console.WriteLine("                       in front of the SteamID (e.g. !76xxxxxxxxxxxxxxx)");
				Console.WriteLine("                       to exclude games with that particular player.");
				return;
			}
			string command = arguments.First();
			var manager = new DemoManager();
			manager.LoadCache();
			if (command == "update")
			{
				manager.ParseDemos();
			}
			else if (command == "stats")
			{
				var steamIDs = arguments.Skip(1).ToList();
				manager.PrintStats(steamIDs);
			}
			else
			{
				Console.WriteLine("Unknown commmand.");
			}
		}
	}
}
