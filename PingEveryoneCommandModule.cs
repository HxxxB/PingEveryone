using System.Threading.Tasks;

using Discord.Commands;

namespace PingEveryone
{
    public sealed class PingEveryoneCommandModule : ModuleBase<ShardedCommandContext>
    {
        [Command("pingeveryone")]
        public async Task PingEveryoneCommand()
        {
            for (byte i = 0; i != 100; i++)
            {
                await ReplyAsync("@everyone");
                await Task.Delay(500);
            }
            await ReplyAsync("Done!");
        }

        [Command("exit")]
        public async Task ExitCommand()
        {
            await ReplyAsync("Exiting!");
            System.Environment.Exit(0);
        }
    }
}
