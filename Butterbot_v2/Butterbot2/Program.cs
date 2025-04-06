using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Fügen Sie ggf. den „erforderlichen“ Modifizierer hinzu, oder deklarieren Sie den Modifizierer als NULL-Werte zulassend.

namespace Butterbot2;

public class Program
{
    private static DiscordSocketClient client;
    public static IServiceProvider serviceProvider;

    public static async Task Main()
    {
        client = new();

        serviceProvider = new ServiceCollection()
            .AddSingleton(client)
            .AddSingleton(new CommandService())
            .BuildServiceProvider();

        var handler = new ClientHandler(serviceProvider);
        await Task.Delay(-1);
    }
}