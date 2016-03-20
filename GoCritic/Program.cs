namespace GoCritic
{
	class Program
	{
		private static void Main(string[] arguments)
		{
			var manager = new DemoManager();
			manager.LoadCache();
			manager.ParseDemos();
			manager.PrintStats();
		}
	}
}
