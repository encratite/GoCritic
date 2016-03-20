using System;
using System.IO;
using System.Linq;
using DemoInfo;

namespace GoCritic
{
	class DemoParser
	{
		private Match _Match = new Match();

		private bool _MatchStarted = false;

		public Match Parse(FileInfo fileInfo)
		{
			_Match.DemoName = fileInfo.Name;
			using (var stream = new FileStream(fileInfo.FullName, FileMode.Open))
			{
				var parser = new DemoInfo.DemoParser(stream);
				parser.MatchStarted += OnMatchStarted;
				parser.PlayerKilled += OnPlayerKilled;
				parser.PlayerTeam += OnPlayerTeam;
				parser.RoundEnd += OnRoundEnd;
				parser.ParseHeader();
				_Match.Map = parser.Map;
				parser.ParseToEnd();
				RemoveSpectators();
			}
			return _Match;
		}

		#region Event handlers

		private void OnMatchStarted(object sender, MatchStartedEventArgs arguments)
		{
			_MatchStarted = true;
		}

		private void OnPlayerKilled(object sender, PlayerKilledEventArgs arguments)
		{
			if (!_MatchStarted || arguments.Killer == null || arguments.Victim == null)
				return;
			var killerStats = GetPlayerStats(arguments.Killer);
			var victimStats = GetPlayerStats(arguments.Victim);
			killerStats.Kills++;
			victimStats.Deaths++;
		}

		private void OnPlayerTeam(object sender, PlayerTeamEventArgs arguments)
		{
			var player = arguments.Swapped;
			if (player == null)
				return;
			var stats = GetPlayerStats(player);
			var oldTeam = GetTeam(arguments.OldTeam);
			oldTeam.Players.Remove(stats);
			var newTeam = GetTeam(arguments.NewTeam);
			newTeam.Players.Add(stats);
		}

		private void OnRoundEnd(object sender, RoundEndedEventArgs arguments)
		{
			if (arguments.Reason == RoundEndReason.GameStart)
				return;
			var team = GetTeam(arguments.Winner);
			team.Score++;
		}

		#endregion

		private PlayerStats GetPlayerStats(Player player)
		{
			var stats = _Match.Teams.SelectMany(t => t.Players).FirstOrDefault(s => s.SteamId == player.SteamID);
			if (stats == null)
				stats = AddPlayer(player, player.Team);
			return stats;
		}

		private Team GetTeam(DemoInfo.Team team)
		{
			var teams = _Match.Teams;
			var output = teams.FirstOrDefault(t => t.TeamEnum == team);
			if (output == null)
			{
				output = new Team { TeamEnum = team };
				teams.Add(output);
			}
			return output;
		}

		private PlayerStats AddPlayer(Player player, DemoInfo.Team teamEnum)
		{
			var team = GetTeam(teamEnum);
			var stats = new PlayerStats(player);
			team.Players.Add(stats);
			return stats;
		}

		private void RemoveSpectators()
		{
			_Match.Teams = _Match.Teams.Where(t => t.TeamEnum != DemoInfo.Team.Spectate).ToList();
		}
	}
}
