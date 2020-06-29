using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hints;
using MEC;
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

		private bool HasAdminBypass(ReferenceHub player)
		{
			string groupName = ServerStatic.GetPermissionsHandler().GetUserGroup(player.characterClassManager.UserId) != null ? ServerStatic.GetPermissionsHandler()._groups.FirstOrDefault(g => g.Value == player.serverRoles.Group).Key : "";
			return TextChat.Config.GetStringList("tc_admin_badges").Count == 0
					? player.serverRoles.RemoteAdmin
					: TextChat.Config.GetStringList("tc_admin_badges").Any(s => groupName == s);
		}

		public void SendMessage(ReferenceHub source, ReferenceHub target, string message)
		{
			string data = TextChat.Config.GetString("tc_hint_msg_data", "<size=100%><color=blue>%name%: </color>%message%</size><br><size=50%><color=yellow>Open the console (~) for more</color></size>");
			string no = "[Hidden]";
			if (plugin.Hints.ContainsKey(target.characterClassManager.UserId))
			{
				if (plugin.Hints[target.characterClassManager.UserId])
				{
					data = data.Replace("%message%", $"{message.Replace("/>", "").Substring(0, Mathf.Min(TextChat.Config.GetInt("tc_max_chars", 60), message.Length))}");
				}
				else
				{
					data = data.Replace("%message%", $"{TextChat.Config.GetString("tc_hint_no", no)}");
				}
			}
			else
			{
				if (TextChat.Config.GetBool("tc_hint_msg_default", true))
				{
					data = data.Replace("%message%", $"{message.Replace("/>", "").Substring(0, Mathf.Min(TextChat.Config.GetInt("tc_max_chars", 60), message.Length))}");
				}
				else
				{
					data = data.Replace("%message%", $"{TextChat.Config.GetString("tc_hint_no", no)}");
				}
			}
			if (TextChat.Config.GetBool("tc_hint_enable", true))
			{
				target.hints.Show(new TextHint(data.Replace("%name%", $"{source.nicknameSync.MyNick}"), new HintParameter[] { new StringHintParameter("") }, new HintEffect[]
				{
				HintEffectPresets.TrailingPulseAlpha(0.5f, 1f, 0.5f, 2f, 0f, 3)
				}, 5f));
			}
			target.characterClassManager.TargetConsolePrint(target.characterClassManager.connectionToClient, $"[{DateTime.Now}] {source.nicknameSync.MyNick}: {message.Replace("/>", "").Substring(0, Mathf.Min(TextChat.Config.GetInt("tc_max_chars", 60), message.Length))}", "green");
		}

		public bool CanSend(ReferenceHub source) => !plugin.Blocked.ContainsKey(source.characterClassManager.UserId) || plugin.Cooldown.Contains(source.queryProcessor.PlayerId);
		public bool BlacklistCheck(string message) => TextChat.Config.GetStringList("tc_blacklisted_words").Any(message.Contains);

		public IEnumerator<float> RemoveCooldown(ReferenceHub player)
		{
			yield return Timing.WaitForSeconds(TextChat.Config.GetFloat("tc_cooldown_time", 1.5f));

			plugin.Cooldown.Remove(player.queryProcessor.PlayerId);
		}

		public bool InRange(ReferenceHub target, ReferenceHub source) =>
			Vector3.Distance(target.transform.position, source.transform.position) < TextChat.Config.GetFloat("tc_area_size", 60f) ||
			(source.characterClassManager.Classes.SafeGet(source.characterClassManager.CurClass).team == Team.SCP && target.characterClassManager.Classes.SafeGet(target.characterClassManager.CurClass).team == Team.SCP);

		//TODO: public bool RadioRangeCheck(Player target, Player source)

		public bool CheckIntercomRange(ReferenceHub source) => Vector3.Distance(intercomArea.position, source.transform.position) <= Intercom.host.triggerDistance;

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

		public static void SetIntercomSpeaker(ReferenceHub source) => Intercom.host.RequestTransmission(source.gameObject);

		public static bool IntercomOverride(ReferenceHub source) => Intercom.host.speaker == source.gameObject;

		public bool CanSee(ReferenceHub target, ReferenceHub source)
		{
			Team targetTeam = target.characterClassManager.Classes.SafeGet(target.characterClassManager.CurClass).team;
			Team sourceTeam = source.characterClassManager.Classes.SafeGet(source.characterClassManager.CurClass).team;
			if (IntercomOverride(source))
				return true;
			if (TextChat.Config.GetBool("tc_admin_bypass", true) && HasAdminBypass(target))
				return true;

			if (sourceTeam == targetTeam)
				return true;

			
			switch (sourceTeam)
			{
				case Team.SCP:
					{
						switch (targetTeam)
						{
							case Team.MTF when TextChat.Config.GetBool("tc_mtf_cansee_scp", true):
							case Team.RSC when TextChat.Config.GetBool("tc_mtf_cansee_scp", true):
							case Team.RIP when TextChat.Config.GetBool("tc_rip_cansee_scp", true):
							case Team.CHI when TextChat.Config.GetBool("tc_chi_cansee_scp", true):
							case Team.CDP when TextChat.Config.GetBool("tc_chi_cansee_scp", true):
							case Team.TUT when TextChat.Config.GetBool("tc_tut_cansee_scp", true):
								return true;
							default:
								return false;
						}
					}
				case Team.CDP:
				case Team.CHI:
					{
						switch (targetTeam)
						{
							case Team.SCP when TextChat.Config.GetBool("tc_scp_cansee_chi", true):
							case Team.MTF when TextChat.Config.GetBool("tc_mtf_cansee_chi", true):
							case Team.RSC when TextChat.Config.GetBool("tc_mtf_cansee_chi", true):
							case Team.RIP when TextChat.Config.GetBool("tc_rip_cansee_chi", true):
							case Team.TUT when TextChat.Config.GetBool("tc_tut_cansee_chi", true):
								return true;
							default:
								return false;
						}
					}
				case Team.RSC:
				case Team.MTF:
					{
						switch (targetTeam)
						{
							case Team.SCP when TextChat.Config.GetBool("tc_scp_cansee_mtf", true):
							case Team.CHI when TextChat.Config.GetBool("tc_chi_cansee_mtf", true):
							case Team.CDP when TextChat.Config.GetBool("tc_chi_cansee_mtf", true):
							case Team.RIP when TextChat.Config.GetBool("tc_rip_cansee_mtf", true):
							case Team.TUT when TextChat.Config.GetBool("tc_tut_cansee_mtf", true):
								return true;
							default:
								return false;
						}
					}
				case Team.TUT:
					{
						switch (targetTeam)
						{
							case Team.SCP when TextChat.Config.GetBool("tc_scp_cansee_tut", true):
							case Team.MTF when TextChat.Config.GetBool("tc_mtf_cansee_tut", true):
							case Team.CHI when TextChat.Config.GetBool("tc_chi_cansee_tut", true):
							case Team.RSC when TextChat.Config.GetBool("tc_mtf_cansee_tut", true):
							case Team.CDP when TextChat.Config.GetBool("tc_chi_cansee_tut", true):
							case Team.RIP when TextChat.Config.GetBool("tc_rip_cansee_tut", true):
								return true;
							default:
								return false;
						}
					}
				case Team.RIP:
					{
						switch (targetTeam)
						{
							case Team.SCP when TextChat.Config.GetBool("tc_scp_cansee_rip", true):
							case Team.MTF when TextChat.Config.GetBool("tc_mtf_cansee_rip", true):
							case Team.CHI when TextChat.Config.GetBool("tc_chi_cansee_rip", true):
							case Team.RSC when TextChat.Config.GetBool("tc_mtf_cansee_rip", true):
							case Team.CDP when TextChat.Config.GetBool("tc_chi_cansee_rip", true):
							case Team.TUT when TextChat.Config.GetBool("tc_tut_cansee_rip", true):
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