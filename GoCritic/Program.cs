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
				Console.WriteLine("This is a CS:GO demo analysis tool to figure out your strongest maps");
				Console.WriteLine("and what premades you perform the best with.");
				Console.WriteLine(string.Empty);
				Console.WriteLine("Commands supported:");
				Console.WriteLine(string.Empty);
				Console.WriteLine("  update               Update the demo cache. This may take several minutes.");
				Console.WriteLine("                       Run this command the first time you use this tool.");
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
