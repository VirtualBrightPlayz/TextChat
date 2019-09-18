using Smod2.API;
using Smod2.Commands;

namespace TextChat
{
	public class Commands : ICommandHandler
	{
		private readonly TextChat plugin;
		public Commands(TextChat plugin) => this.plugin = plugin;
		
		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (args.Length < 2)
				return new[] {GetUsage()};
			switch (args[0].ToLower())
			{
				case "block":
				{
					if (args.Length < 3)
						return new[]
							{"You must supply a player name or ID and a number of rounds. use -1 for permanent."};
					if (!int.TryParse(args[1], out int id))
						return new[] {"Invalid PlayerID specified."};
					if (!int.TryParse(args[2], out int count))
						return new[] {"Invalid round count argument, must be a number!"};

					Player target = plugin.Server.GetPlayer(id);
					
					return target == null ? new[] {"Player not found."} : new []{plugin.Functions.AddBlockedUser(target.SteamId, count)};
				}
				case "unblock":
				{
					if (!int.TryParse(args[1], out int id))
						return new[] {"Invalid PlayerID specified."};

					Player target = plugin.Server.GetPlayer(id);
					
					return target == null ? new[] {"Player not found."} : new []{plugin.Functions.RemoveBlockedUser(target.SteamId)};
				}
			}

			return new[] {GetUsage()};
		}

		public string GetUsage() =>
			"TextChat Commands\n" +
			"tchat block (PlayerID) (number of rounds) - Blocks the user from sending chat messages for (number) amount of rounds. Use -1 to block permanently.\n" +
			"tchat unblock (PlayerID) - Unblocks the user regardless of how many rounds are remaining in their counter.";

		public string GetCommandDescription()
		{
			return "";
		}
	}
}