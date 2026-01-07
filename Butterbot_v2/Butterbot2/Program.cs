using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Butterbot2;

public class Program
{
    public static async Task Main()
    {
        await using var services = ConfigureServices();
        var handler = new ClientHandler(services);
        await Task.Delay(-1);
    }

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            })
            .AddSingleton(new CommandServiceConfig {
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Debug,
                CaseSensitiveCommands = false,
            })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlingService>()
            .AddSingleton<HttpClient>()
            .AddSingleton<CatService>()
            .AddSingleton<UserService>()
            .AddSingleton<SoundService>()
            .AddSingleton<TimerService>()
            .AddLavalink()
            .BuildServiceProvider();
    }
}