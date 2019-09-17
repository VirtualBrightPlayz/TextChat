using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MEC;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;

namespace TextChat
{
	public class EventHandlers : IEventHandlerWaitingForPlayers, IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerCallCommand, IEventHandlerPlayerJoin
	{
		private readonly TextChat plugin;
		public EventHandlers(TextChat plugin) => this.plugin = plugin;
		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			//Ensure directory exists
			if (!Directory.Exists(plugin.BlockedPath.Replace("/blocked.txt", "")))
			{
				plugin.Info("TextChat directory missing, creating..");
				Directory.CreateDirectory(plugin.BlockedPath.Replace("/blocked.txt", ""));
			}
			//Ensure files exist
			if (!File.Exists(plugin.BlockedPath))
			{
				plugin.Info("Blocked users file not found, creating..");
				File.Create(plugin.BlockedPath);
			}

			if (!File.Exists(plugin.LocalMutePath))
			{
				plugin.Info("Muted users file not found, creating..");
				File.Create(plugin.LocalMutePath);
			}
			
			plugin.Cooldown.Clear();

			//Setup blocked user parsing
			string[] blockedReadArray = File.ReadAllLines(plugin.BlockedPath);
			foreach (string s in blockedReadArray)
			{
				string[] blockedParse = s.Split(new[] {":"}, StringSplitOptions.None);
				if (!int.TryParse(blockedParse[1], out int result))
				{
					plugin.Error($"Invalid duration counter for {blockedParse[0]}");
					continue;
				}

				if (!plugin.Blocked.ContainsKey(blockedParse[0]) && result != 0)
					plugin.Blocked.Add(blockedParse[0], result);
				
				if (result == 0)
					if (plugin.Blocked.ContainsKey(blockedParse[0]))
						plugin.Blocked.Remove(blockedParse[0]);
			}
			
			//setup locally muted user parsing
			string[] mutedReadArray = File.ReadAllLines(plugin.LocalMutePath);
			foreach (string s in mutedReadArray)
			{
				string[] mutedParse = s.Split(new[] {":"}, StringSplitOptions.None);
				if (!plugin.LocalMuted.ContainsKey(mutedParse[0]))
					plugin.LocalMuted.Add(mutedParse[0], mutedParse[1].Split(new []{"."}, StringSplitOptions.None).ToList());
			}
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			foreach (string blocked in plugin.Blocked.Keys)
				plugin.Blocked[blocked]--;
		}

		public void OnRoundEnd(RoundEndEvent ev)
		{

			foreach (CoroutineHandle handle in plugin.Coroutines)
				Timing.KillCoroutines(handle);
			
			string[] blockedWriteArray = {};
			foreach (string blocked in plugin.Blocked.Keys.Where(blocked => plugin.Blocked[blocked] != 0))
				blockedWriteArray.Append($"{blocked}:{plugin.Blocked[blocked]}");
			
			File.WriteAllLines(plugin.BlockedPath, blockedWriteArray);

			string[] mutedWriteArray = { };
			foreach (string key in plugin.LocalMuted.Keys)
			{
				string muted = plugin.LocalMuted[key].Aggregate("", (current, s) => current + $"{s}.");
				mutedWriteArray.Append($"{key}:{muted}");
			}

			File.WriteAllLines(plugin.LocalMutePath, mutedWriteArray);
		}

		public void OnCallCommand(PlayerCallCommandEvent ev)
		{
			if (!ev.Command.StartsWith(".chat") || !ev.Command.StartsWith(".mute") || !ev.Command.StartsWith(".unmute"))
				return;

			if (ev.Command.StartsWith(".mute"))
			{
				string[] args = ev.Command.Split(new[] {" "}, StringSplitOptions.None);
				if (args.Length < 2)
				{
					ev.ReturnMessage = "You must supply a player name of who to mute.";
					return;
				}

				List<Player> players = plugin.Server.GetPlayers(args[1]);
				if (players == null || players.Count == 0)
				{
					ev.ReturnMessage = "Player not found.";
					return;
				}

				Player player = players.OrderBy(ply => ply.Name.Length).First();
				if (plugin.LocalMuted[ev.Player.SteamId].Contains(player.SteamId))
				{
					ev.ReturnMessage = "That player is already muted.";
					return;
				}
				
				plugin.LocalMuted[ev.Player.SteamId].Add(player.SteamId);
				ev.ReturnMessage = $"{player.Name} has been muted. You will no longer see messages from this person.";
			}
			
			if (ev.Command.StartsWith(".unmute"))
			{
				string[] args = ev.Command.Split(new[] {" "}, StringSplitOptions.None);
				if (args.Length < 2)
				{
					ev.ReturnMessage = "You must supply a player name of who to mute.";
					return;
				}

				List<Player> players = plugin.Server.GetPlayers(args[1]);
				if (players == null || players.Count == 0)
				{
					ev.ReturnMessage = "Player not found.";
					return;
				}

				Player player = players.OrderBy(ply => ply.Name.Length).First();
				if (!plugin.LocalMuted[ev.Player.SteamId].Contains(player.SteamId))
				{
					ev.ReturnMessage = "That player is not muted.";
					return;
				}
				
				plugin.LocalMuted[ev.Player.SteamId].Remove(player.SteamId);
				ev.ReturnMessage = $"{player.Name} has been unmuted. You will now see messages from this person.";
			}

			if (!Methods.IntercomOverride(ev.Player))
			{
				if (plugin.Cooldown.Contains(ev.Player.PlayerId))
				{
					ev.ReturnMessage =
						$"Your message was not sent, please send messages no less than {plugin.CooldownTime} seconds apart.";
					return;
				}

				if (!plugin.Functions.CanSend(ev.Player))
				{
					ev.ReturnMessage = "You cannot send messages because you have been muted by staff.";
					return;
				}
				
				if (Methods.CheckIntercomRange(ev.Player) && !Intercom.host.speaking && plugin.IntercomSendAll)
					Methods.SetIntercomSpeaker(ev.Player);
			}

			if (plugin.Functions.BlacklistCheck(ev.Command))
			{
				ev.ReturnMessage = "Your message was blocked because it contained a blacklisted word.";
				return;
			}
			
			foreach (Player player in plugin.Server.GetPlayers())
				if (!plugin.LocalMuted[player.SteamId].Contains(ev.Player.SteamId))
					if (plugin.Functions.CanSee(player, ev.Player))
						if (plugin.AreaChat && plugin.Functions.InRange(player, ev.Player))
							plugin.Functions.SendMessage(ev.Player, player, ev.Command.Replace(".chat",""));
						else if (!plugin.AreaChat)
							plugin.Functions.SendMessage(ev.Player, player, ev.Command.Replace(".chat",""));
			
			plugin.Cooldown.Add(ev.Player.PlayerId);
			plugin.Coroutines.Add(Timing.RunCoroutine(plugin.Functions.RemoveCooldown(ev.Player)));
			ev.ReturnMessage = "";
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			if (!plugin.LocalMuted.ContainsKey(ev.Player.SteamId))
				plugin.LocalMuted.Add(ev.Player.SteamId, new List<string>());
		}
	}
}