using System;
using System.Collections.Generic;
using System.IO;
using EXILED;
using MEC;
using UnityEngine;

namespace TextChat
{

	public class TextChat : Plugin
	{
		public Methods Functions { get; private set; }

		public override string getName
		{
			get
			{
				return "TextChat";
			}
		}

		public List<int> Cooldown = new List<int>();
		public Dictionary<string, int> Blocked = new Dictionary<string, int>();
		public Dictionary<string, bool> Hints = new Dictionary<string, bool>();
		public Dictionary<string, List<string>> LocalMuted = new Dictionary<string, List<string>>();
		public List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();
		public Transform IntercomArea = null;

		public EventHandlers handlers;
		public Commands commands;

		public static string pluginDir;

		public override void OnEnable()
		{
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			pluginDir = Path.Combine(appData, "Plugins", "TextChat");
			handlers = new EventHandlers(this);
			commands = new Commands(this);
			if (!Directory.Exists(pluginDir))
				Directory.CreateDirectory(pluginDir);
			Log.Info($"TextChat - enabled.");
			Log.Info($"BlockedPath: {Config.GetString("tc_blocked_path", $"{TextChat.pluginDir}/blocked.txt")}\nLocalPath: {Config.GetString("tc_local_mute_path", $"{TextChat.pluginDir}/muted.txt")}\nHintsPath: {Config.GetString("tc_local_hints_path", $"{TextChat.pluginDir}/hints.txt")}");
			Events.RemoteAdminCommandEvent += commands.RemoteAdminCommandEvent;
			Events.WaitingForPlayersEvent += handlers.OnWaitingForPlayers;
			Events.RoundStartEvent += handlers.OnRoundStart;
			Events.RoundEndEvent += handlers.OnRoundEnd;
			Events.ConsoleCommandEvent += handlers.OnCallCommand;
			Events.PlayerJoinEvent += handlers.OnPlayerJoin;
			Functions = new Methods(this);
		}

		public override void OnDisable()
		{
			Log.Info($"TextChat - disabled.");
		}

		public override void OnReload()
		{
		}
	}
}