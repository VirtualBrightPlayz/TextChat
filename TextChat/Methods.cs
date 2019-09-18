using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MEC;
using Smod2;
using Smod2.API;
using UnityEngine;

namespace TextChat
{
	public class Methods
	{
		private readonly TextChat plugin;
		public Methods(TextChat plugin) => this.plugin = plugin;

		public string RemoveBlockedUser(string steamid)
		{
			if (!plugin.Blocked.ContainsKey(steamid))
				return "User is not blocked.";

			plugin.Blocked.Remove(steamid);
			return "User is no longer blocked.";
		}

		public string AddBlockedUser(string steamid, int count)
		{
			if (plugin.Blocked.ContainsKey(steamid))
				return "User is already blocked.";
			
			plugin.Blocked.Add(steamid, count);
			string duration = count == -1 ? "permanently" : $"for {count} rounds.";
			return $"User blocked {duration}.";
		}

		private bool HasAdminBypass(Player player) => plugin.AdminBadges.Length == 0
			? ((GameObject) player.GetGameObject()).GetComponent<ServerRoles>().RemoteAdmin
			: plugin.AdminBadges.Any(s => player.GetUserGroup().Name == s);

		public void SendMessage(Player source, Player target, string message)
		{
			target.SendConsoleMessage($"[{DateTime.Now}] {source.Name}: {message}");
		}

		public bool CanSend(Player source) => !plugin.Blocked.ContainsKey(source.SteamId) || plugin.Cooldown.Contains(source.PlayerId);
		public bool BlacklistCheck(string message) => plugin.BlacklistedWords.Any(message.Contains);

		public IEnumerator<float> RemoveCooldown(Player player)
		{
			yield return Timing.WaitForSeconds(plugin.CooldownTime);

			plugin.Cooldown.Remove(player.PlayerId);
		}

		public bool InRange(Player target, Player source) =>
			Vector.Distance(target.GetPosition(), source.GetPosition()) < plugin.AreaSize ||
			(source.TeamRole.Team == Smod2.API.Team.SCP && target.TeamRole.Team == Smod2.API.Team.SCP);

		//TODO: public bool RadioRangeCheck(Player target, Player source)

		public bool CheckIntercomRange(Player source) => Vector3.Distance(intercomArea.position, source.GameObject().transform.position) <= Intercom.host.triggerDistance;

		private Transform intercomArea
		{
			get
			{
				if (plugin.IntercomArea == null)
					plugin.IntercomArea = typeof(Intercom).GetField("area", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(Intercom.host) as Transform;

				if (plugin.IntercomArea == null) 
					throw new MissingFieldException("Field for intercom not found.");

				return plugin.IntercomArea;
			}
		}

		public static void SetIntercomSpeaker(Player source) => Intercom.host.RequestTransmission(source.GameObject());

		public static bool IntercomOverride(Player source) => Intercom.host.speaker == source.GameObject();

		public bool CanSee(Player target, Player source)
		{
			if (IntercomOverride(source))
				return true;
			if (plugin.AdminBypass && HasAdminBypass(target))
				return true;

			if (source.TeamRole.Team == target.TeamRole.Team)
				return true;

			Smod2.API.Team tarTeam = target.TeamRole.Team;
			
			switch (source.TeamRole.Team)
			{
				case Smod2.API.Team.SCP:
				{
					switch (tarTeam)
					{
						case Smod2.API.Team.NINETAILFOX when plugin.MtfCanseeScp:
						case Smod2.API.Team.SCIENTIST when plugin.MtfCanseeScp:
						case Smod2.API.Team.SPECTATOR when plugin.SpecCanseeScp: 
						case Smod2.API.Team.CHAOS_INSURGENCY when plugin.CiCanseeScp:    
						case Smod2.API.Team.CLASSD when plugin.CiCanseeScp:
						case Smod2.API.Team.TUTORIAL when plugin.TutCanseeScp:
							return true;
						default:
							return false;
					}
				}
				case Smod2.API.Team.CLASSD:
				case Smod2.API.Team.CHAOS_INSURGENCY:
				{
					switch (tarTeam)
					{
						case Smod2.API.Team.SCP when plugin.ScpCanseeCi:
						case Smod2.API.Team.NINETAILFOX when plugin.MtfCanseeCi:
						case Smod2.API.Team.SCIENTIST when plugin.MtfCanseeCi:
						case Smod2.API.Team.SPECTATOR when plugin.SpecCanseeCi:
						case Smod2.API.Team.TUTORIAL when plugin.TutCanseeCi:
							return true;
						default:
							return false;
					}
				}
				case Smod2.API.Team.SCIENTIST:
				case Smod2.API.Team.NINETAILFOX:
				{
					switch (tarTeam)
					{
						case Smod2.API.Team.SCP when plugin.ScpCanseeMtf:
						case Smod2.API.Team.CHAOS_INSURGENCY when plugin.CiCanseeMtf:
						case Smod2.API.Team.CLASSD when plugin.CiCanseeMtf:
						case Smod2.API.Team.SPECTATOR when plugin.SpecCanseeMtf:
						case Smod2.API.Team.TUTORIAL when plugin.TutCanseeMtf:
							return true;
						default:
							return false;
					}
				}
				case Smod2.API.Team.TUTORIAL:
				{
					switch (tarTeam)
					{
						case Smod2.API.Team.SCP when plugin.ScpCanseeTut:
						case Smod2.API.Team.NINETAILFOX when plugin.MtfCanseeTut:
						case Smod2.API.Team.CHAOS_INSURGENCY when plugin.CiCanseeTut:
						case Smod2.API.Team.SCIENTIST when plugin.MtfCanseeTut:
						case Smod2.API.Team.CLASSD when plugin.CiCanseeTut:
						case Smod2.API.Team.SPECTATOR when plugin.SpecCanseeTut:
							return true;
						default:
							return false;
					}
				}
				case Smod2.API.Team.SPECTATOR:
				{
					switch(tarTeam)
					{
						case Smod2.API.Team.SCP when plugin.ScpCanseeSpec:
						case Smod2.API.Team.NINETAILFOX when plugin.MtfCanseeSpec:
						case Smod2.API.Team.CHAOS_INSURGENCY when plugin.CiCanseeSpec:
						case Smod2.API.Team.SCIENTIST when plugin.MtfCanseeSpec:
						case Smod2.API.Team.CLASSD when plugin.CiCanseeSpec:
						case Smod2.API.Team.TUTORIAL when plugin.TutCanseeSpec:
							return true;
						default:
							return false;
					}
				}
				default:
					return false;
			}
		}
	}
}