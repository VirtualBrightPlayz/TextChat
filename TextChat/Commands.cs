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
            if (args.Length < 3)
                return new[] {GetUsage()};
            switch (args[1].ToLower())
            {
                case "block":
                {
                    if (args.Length < 4)
                        return new[]
                            {"You must supply a player name or ID and a number of rounds. use -1 for permanent."};
                    if (!int.TryParse(args[3], out int id))
                        return new[] {"Invalid PlayerID specified."};
                    if (!int.TryParse(args[4], out int count))
                        return new[] {"Invalid round count argument, must be a number!"};

                    Player target = plugin.Server.GetPlayer(id);
                    
                    return target == null ? new[] {"Player not found."} : new []{plugin.Functions.AddBlockedUser(target.SteamId, count)};
                }
                case "unblock":
                {
                    if (!int.TryParse(args[3], out int id))
                        return new[] {"Invalid PlayerID specified."};

                    Player target = plugin.Server.GetPlayer(id);
                    
                    return target == null ? new[] {"Player not found."} : new []{plugin.Functions.RemoveBlockedUser(target.SteamId)};
                }
            }

            return new[] {GetUsage()};
        }

        public string GetUsage()
        {
            return "TextChat Commands\n" +
                   "tchat block (PlayerID) (number of rounds) - Blocks the user from sending chat messages for (number) amount of rounds. Use -1 to block permanently.\n" +
                   "tchat unblock (PlayerID) - Unblocks the user regardless of how many rounds are remaining in their counter.";
        }

        public string GetCommandDescription()
        {
            return "";
        }
    }
}