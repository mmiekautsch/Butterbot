using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Fügen Sie ggf. den „erforderlichen“ Modifizierer hinzu, oder deklarieren Sie den Modifizierer als NULL-Werte zulassend.

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
            .BuildServiceProvider();
    }
}