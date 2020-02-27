using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EXILED;
using EXILED.Extensions;
using MEC;

namespace TextChat
{
	public class EventHandlers
	{
		private readonly TextChat plugin;
		public EventHandlers(TextChat plugin) => this.plugin = plugin;
		public void OnWaitingForPlayers()
		{
			try
			{
				//Ensure directory exists
				if (!Directory.Exists(TextChat.Config.GetString("tc_blocked_path", $"{TextChat.pluginDir}/blocked.txt").Replace("/blocked.txt", "")))
				{
					Log.Info("TextChat directory missing, creating..");
					Directory.CreateDirectory(TextChat.Config.GetString("tc_blocked_path", $"{TextChat.pluginDir}/blocked.txt").Replace("/blocked.txt", ""));
				}

				//Ensure files exist
				if (!File.Exists(TextChat.Config.GetString("tc_blocked_path", $"{TextChat.pluginDir}/blocked.txt")))
				{
					Log.Info("Blocked users file not found, creating..");
					File.Create(TextChat.Config.GetString("tc_blocked_path", $"{TextChat.pluginDir}/blocked.txt"));
				}

				if (!File.Exists(TextChat.Config.GetString("tc_local_mute_path", $"{TextChat.pluginDir}/muted.txt")))
				{
					Log.Info("Muted users file not found, creating..");
					File.Create(TextChat.Config.GetString("tc_local_mute_path", $"{TextChat.pluginDir}/muted.txt"));
				}
			}
			catch (Exception)
			{
				// ignored
			}

			plugin.Cooldown.Clear();
			plugin.Blocked.Clear();
			plugin.LocalMuted.Clear();

			//Setup blocked user parsing
			string[] blockedReadArray = File.ReadAllLines(TextChat.Config.GetString("tc_blocked_path", $"{TextChat.pluginDir}/blocked.txt"));
			foreach (string s in blockedReadArray)
			{
				string[] blockedParse = s.Split(new[] {":"}, StringSplitOptions.None);
				if (!int.TryParse(blockedParse[1], out int result))
				{
					Log.Error($"Invalid duration counter for {blockedParse[0]}");
					continue;
				}

				if (!plugin.Blocked.ContainsKey(blockedParse[0]) && result != 0)
					plugin.Blocked.Add(blockedParse[0], result);
				
				if (result == 0)
					if (plugin.Blocked.ContainsKey(blockedParse[0]))
						plugin.Blocked.Remove(blockedParse[0]);
			}
			
			//setup locally muted user parsing
			string[] mutedReadArray = File.ReadAllLines(TextChat.Config.GetString("tc_local_mute_path", $"{TextChat.pluginDir}/muted.txt"));
			foreach (string s in mutedReadArray)
			{
				string[] mutedParse = s.Split(new[] {":"}, StringSplitOptions.None);
				if (!plugin.LocalMuted.ContainsKey(mutedParse[0]))
					plugin.LocalMuted.Add(mutedParse[0], mutedParse[1].Split(new []{"."}, StringSplitOptions.None).ToList());
			}
		}

		public void OnRoundStart()
		{
			foreach (string blocked in plugin.Blocked.Keys)
				plugin.Blocked[blocked]--;
		}

		public void OnRoundEnd()
		{
			plugin.IntercomArea = null;
			foreach (CoroutineHandle handle in plugin.Coroutines)
				Timing.KillCoroutines(handle);
			
			List<string>blockedWritelist = plugin.Blocked.Keys.Where(blocked => plugin.Blocked[blocked] != 0).Select(blocked => $"{blocked}:{plugin.Blocked[blocked]}").ToList();
			File.WriteAllLines(TextChat.Config.GetString("tc_blocked_path", $"{TextChat.pluginDir}/blocked.txt"), blockedWritelist);
			
			List<string> mutedWriteList = new List<string>();
			foreach (string key in plugin.LocalMuted.Keys)
			{
				string muted = plugin.LocalMuted[key].Aggregate("", (current, s) => current + $"{s}.");
				if (muted == ".")
					continue;
				mutedWriteList.Add($"{key}:{muted}");
			}
			
			File.WriteAllLines(TextChat.Config.GetString("tc_local_mute_path", $"{TextChat.pluginDir}/muted.txt"), mutedWriteList);
		}

		public void OnCallCommand(ConsoleCommandEvent ev)
		{
			if (!ev.Command.StartsWith("chat") && !ev.Command.StartsWith("mute") && !ev.Command.StartsWith("unmute")) 
				return;

			if (ev.Command.StartsWith("mute"))
			{
				string[] args = ev.Command.Split(new[] {" "}, StringSplitOptions.None);
				if (args.Length < 2)
				{
					ev.ReturnMessage = "You must supply a player name of who to mute.";
					return;
				}

				ReferenceHub playert = Player.GetPlayer(args[1]);
				if (playert == null)
				{
					ev.ReturnMessage = "Player not found.";
					return;
				}

				if (plugin.LocalMuted[ev.Player.characterClassManager.UserId].Contains(playert.characterClassManager.UserId))
				{
					ev.ReturnMessage = "That player is already muted.";
					return;
				}
				
				plugin.LocalMuted[ev.Player.characterClassManager.UserId].Add(playert.characterClassManager.UserId);
				ev.ReturnMessage = $"{playert.nicknameSync.MyNick} has been muted. You will no longer see messages from this person.";
				return;
			}
			
			if (ev.Command.StartsWith("unmute"))
			{
				string[] args = ev.Command.Split(new[] {" "}, StringSplitOptions.None);
				if (args.Length < 2)
				{
					ev.ReturnMessage = "You must supply a player name of who to mute.";
					return;
				}

				ReferenceHub player = Player.GetPlayer(args[1]);
				if (player == null)
				{
					ev.ReturnMessage = "Player not found.";
					return;
				}

				if (!plugin.LocalMuted[ev.Player.characterClassManager.UserId].Contains(player.characterClassManager.UserId))
				{
					ev.ReturnMessage = "That player is not muted.";
					return;
				}
				
				plugin.LocalMuted[ev.Player.characterClassManager.UserId].Remove(player.characterClassManager.UserId);
				ev.ReturnMessage = $"{player.nicknameSync.MyNick} has been unmuted. You will now see messages from this person.";
				return;
			}

			if (!Methods.IntercomOverride(ev.Player))
			{
				if (plugin.Cooldown.Contains(ev.Player.queryProcessor.PlayerId))
				{
					ev.ReturnMessage =
						$"Your message was not sent, please send messages no less than {TextChat.Config.GetFloat("tc_cooldown_time", 1.5f)} seconds apart.";
					return;
				}

				if (!plugin.Functions.CanSend(ev.Player))
				{
					ev.ReturnMessage = "You cannot send messages because you have been muted by staff.";
					PlayerManager.localPlayer.GetComponent<Intercom>().Networkspeaker = ev.Player.gameObject;
					return;
				}

				if (plugin.Functions.CheckIntercomRange(ev.Player) && !Intercom.host.speaking && TextChat.Config.GetBool("tc_intercom_send_all", true)) 
					Methods.SetIntercomSpeaker(ev.Player);
			}

			if (plugin.Functions.BlacklistCheck(ev.Command))
			{
				ev.ReturnMessage = "Your message was blocked because it contained a blacklisted word.";
				return;
			}
			
			foreach (ReferenceHub player in Player.GetHubs())
				if (!plugin.LocalMuted[player.characterClassManager.UserId].Contains(ev.Player.characterClassManager.UserId))
					if (plugin.Functions.CanSee(player, ev.Player))
						if (TextChat.Config.GetBool("tc_area_chat", false) && plugin.Functions.InRange(player, ev.Player))
							plugin.Functions.SendMessage(ev.Player, player, ev.Command.Replace("chat ",""));
						else if (!TextChat.Config.GetBool("tc_area_chat", false))
							plugin.Functions.SendMessage(ev.Player, player, ev.Command.Replace("chat ",""));
			
			plugin.Cooldown.Add(ev.Player.queryProcessor.PlayerId);
			plugin.Coroutines.Add(Timing.RunCoroutine(plugin.Functions.RemoveCooldown(ev.Player)));
			ev.ReturnMessage = "";
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			if (!plugin.LocalMuted.ContainsKey(ev.Player.characterClassManager.UserId))
				plugin.LocalMuted.Add(ev.Player.characterClassManager.UserId, new List<string>());
		}
	}
}