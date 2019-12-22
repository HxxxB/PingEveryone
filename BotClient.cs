using System;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PingEveryone
{
    public sealed class BotClient
    {
        private const string @Token = "NjI3OTkxODc3MDUyMDA2NDAw.Xf62Xg.Zsmj_zikZfbaVVDTX5Xy9bHZJAk";

        [MTAThread]
        private static async Task Main()
        {
            await new BotClient().RunBotClient();
        }

        private BotClient()
        {
            GC.KeepAlive(this);
            GC.SuppressFinalize(this);
            GC.AddMemoryPressure(24576);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .Build();
        }

        public DiscordShardedClient Client { get; set; }
        public CommandService CommandOperation { get; set; }
        public ServiceProvider Services { get; set; }
        public IConfigurationRoot Configuration { get; }

        private bool IsClientInitialized { get; set; } = false;

        public async Task RunBotClient()
        {
            if (IsClientInitialized == true) return;
            IsClientInitialized = true;
            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 500,
                TotalShards = 2
            });

            CommandOperation = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false
            });

            Services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(CommandOperation)
                .AddSingleton(Configuration)
                .AddSingleton<CommandService>()
                .BuildServiceProvider();

            Client.ShardReady += arg =>
            {
                Console.WriteLine($"Shard {arg.ShardId} is connected and ready!");
                return Task.CompletedTask;
            };

            Client.Log += arg =>
            {
                Console.WriteLine(arg.ToString());
                return Task.CompletedTask;
            };

            Client.MessageReceived += async arg => {
                if (!(arg is SocketUserMessage message) || message.Author.IsBot) return;
                int argumentPos = 0;
                if (message.HasStringPrefix(@">>", ref argumentPos) || message.HasMentionPrefix(Client.CurrentUser, ref argumentPos))
                {
                    ShardedCommandContext context = new ShardedCommandContext(Client, message);
                    IResult result = await CommandOperation.ExecuteAsync(context, argumentPos, Services);
                    if (!result.IsSuccess)
                    {
                        Console.WriteLine(result.ErrorReason);
                        await message.Channel.SendMessageAsync(result.ErrorReason);
                    }
                }
            };

            // Register commands
            await CommandOperation.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

            await Client.LoginAsync(TokenType.Bot, @Token, true);
            await Client.StartAsync();
            await Client.SetGameAsync("everyone legit being pinged", type: ActivityType.Watching);
            await Task.Delay(System.Threading.Timeout.Infinite);
        }
    }
}
