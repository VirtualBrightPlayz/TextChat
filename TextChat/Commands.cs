
using EXILED;
using EXILED.Extensions;

namespace TextChat
{
	public class Commands
	{
		private readonly TextChat plugin;
		public Commands(TextChat plugin) => this.plugin = plugin;
		
		public void RemoteAdminCommandEvent(ref RACommandEvent ev)
		{
			string[] args = ev.Command.Split(' ');
			ReferenceHub sender = ev.Sender.SenderId == "SERVER CONSOLE" || ev.Sender.SenderId == "GAME CONSOLE" ? Player.GetPlayer(PlayerManager.localPlayer) : Player.GetPlayer(ev.Sender.SenderId);
			switch (args[0].ToLower())
			{
				case "block":
					if (args.Length < 3)
					{
						ev.Sender.RaReply("TextChat#You must supply a player name or ID and a number of rounds. use -1 for permanent.", true, true, string.Empty);
						ev.Allow = false;
						return;
					}
					if (!int.TryParse(args[1], out int id))
					{
						ev.Sender.RaReply("TextChat#Invalid PlayerID specified.", true, true, string.Empty);
						ev.Allow = false;
						return;
					}
					if (!int.TryParse(args[2], out int count))
					{
						ev.Sender.RaReply("TextChat#Invalid round count argument, must be a number!", true, true, string.Empty);
						ev.Allow = false;
						return;
					}

					ReferenceHub target = Player.GetPlayer(id);

					if (target == null)
					{
						ev.Sender.RaReply("TextChat#Player not found.", true, true, string.Empty);
						ev.Allow = false;
						return;
					}
					else
					{
						plugin.Functions.AddBlockedUser(target.characterClassManager.UserId, count);
						ev.Allow = false;
						return;
					}
				case "unblock":
					if (!int.TryParse(args[1], out int id2))
					{
						ev.Sender.RaReply("TextChat#Invalid PlayerID specified.", true, true, string.Empty);
						ev.Allow = false;
						return;
					}

					ReferenceHub target2 = Player.GetPlayer(id2);
					if (target2 == null)
					{
						ev.Sender.RaReply("TextChat#Player not found.", true, true, string.Empty);
						ev.Allow = false;
						return;
					}
					else
					{
						plugin.Functions.RemoveBlockedUser(target2.characterClassManager.UserId);
						ev.Allow = false;
						return;
					}
			}
		}
	}
}