using System;
using System.Collections.Generic;
using System.IO;
using MEC;
using Smod2;
using Smod2.Attributes;
using Smod2.Config;
using UnityEngine;

namespace TextChat
{
	[PluginDetails(author = "Joker119",
		description = "Text chat in console",
		id = "joker119.textchat",
		configPrefix = "tc",
		name = "TextChat",
		version = "1.0.4",
		SmodMajor = 3,
		SmodMinor = 5,
		SmodRevision = 1)]
	
	public class TextChat: Plugin
	{
		public Methods Functions { get; private set; }
		public List<int> Cooldown = new List<int>();
		public Dictionary<string, int> Blocked = new Dictionary<string, int>();
		public Dictionary<string, List<string>> LocalMuted = new Dictionary<string, List<string>>();
		public List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();
		public Transform IntercomArea = null;

		[ConfigOption] public string[] BlacklistedWords = new string[] { };
		[ConfigOption] public bool TutCanseeScp = true;
		[ConfigOption] public bool TutCanseeMtf = true;
		[ConfigOption] public bool TutCanseeCi = true;
		[ConfigOption] public bool TutCanseeSpec = true;
		[ConfigOption] public bool CiCanseeScp = true;
		[ConfigOption] public bool CiCanseeMtf = true;
		[ConfigOption] public bool CiCanseeTut = true;
		[ConfigOption] public bool CiCanseeSpec = true;
		[ConfigOption] public bool MtfCanseeScp = true;
		[ConfigOption] public bool MtfCanseeTut = true;
		[ConfigOption] public bool MtfCanseeCi = true;
		[ConfigOption] public bool MtfCanseeSpec = true;
		[ConfigOption] public bool SpecCanseeScp = true;
		[ConfigOption] public bool SpecCanseeMtf = true;
		[ConfigOption] public bool SpecCanseeCi = true;
		[ConfigOption] public bool SpecCanseeTut = true;
		[ConfigOption] public bool ScpCanseeTut = true;
		[ConfigOption] public bool ScpCanseeMtf = true;
		[ConfigOption] public bool ScpCanseeCi = true;
		[ConfigOption] public bool ScpCanseeSpec = true;
		[ConfigOption] public string BlockedPath = $"{Path.GetDirectoryName(Environment.CurrentDirectory)}/TextChat/blocked.txt";
		[ConfigOption] public string LocalMutePath = $"{Path.GetDirectoryName(Environment.CurrentDirectory)}/TextChat/muted.txt";
		[ConfigOption] public bool AreaChat = false;
		[ConfigOption] public float AreaSize = 60f;
		[ConfigOption] public bool AdminBypass = true;
		[ConfigOption] public string[] AdminBadges = new string[] {};
		[ConfigOption] public float CooldownTime = 1.5f;
		[ConfigOption] public bool UseRadioRange = false;
		[ConfigOption] public bool IntercomSendAll = true;
		

		public override void Register()
		{
			Info($"BlockedPath: {BlockedPath}\nLocalPath: {LocalMutePath}");
			AddEventHandlers(new EventHandlers(this));
			AddCommands(new []{"chat"}, new Commands(this));
			Functions = new Methods(this);
		}

		public override void OnEnable()
		{
			Info($"{Details.name} - {Details.version} enabled.");
		}

		public override void OnDisable()
		{
			Info($"{Details.name} - {Details.version} disabled.");
		}
	}
}