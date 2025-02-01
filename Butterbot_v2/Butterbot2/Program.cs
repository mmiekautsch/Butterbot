using Discord;
using Discord.WebSocket;
using System.Diagnostics;

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Fügen Sie ggf. den „erforderlichen“ Modifizierer hinzu, oder deklarieren Sie den Modifizierer als NULL-Werte zulassend.

namespace Butterbot2;

public class Program
{
    private static DiscordSocketClient client;

    public static async Task Main()
    {
        client = new();

        client.Log += Log;

        await client.LoginAsync(TokenType.Bot, File.ReadAllText("token.txt"));

        await client.StartAsync();
        await Task.Delay(-1);
    }

    private static Task Log(LogMessage msg)
    {
        Trace.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}