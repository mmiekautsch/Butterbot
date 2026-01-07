using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Fügen Sie ggf. den „erforderlichen“ Modifizierer hinzu, oder deklarieren Sie den Modifizierer als NULL-Werte zulassend.

namespace Butterbot2
{
    public class ClientHandler
    {
        private readonly CommandHandlingService commandHandler;
        private readonly DiscordSocketClient client;
        private readonly TimerService timerService;
        private static IServiceProvider services;

        public ClientHandler(IServiceProvider _services)
        {
            services = _services;
            client = services.GetRequiredService<DiscordSocketClient>();
            commandHandler = services.GetRequiredService<CommandHandlingService>();


            timerService = services.GetRequiredService<TimerService>();
            InitializeAsync().Wait();
        }

        public async Task InitializeAsync()
        {
            client.Log += Log;
            client.Ready += OnReady;
            await commandHandler.InstallCommandsAsync();
            await client.LoginAsync(TokenType.Bot, File.ReadAllText("token.txt"));
            await client.StartAsync();
            timerService.StartChannelInteractionTimer();
            timerService.StartSoundInteractionTimer();
        }

        private Task Log(LogMessage msg)
        {
            Trace.WriteLine(msg.ToString());
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task OnReady()
        {
            client.SetGameAsync("spielt dumme scheiße ab");
            //(client.GetChannel(butterChannel) as IMessageChannel)?.SendMessageAsync("Butter is back online bitches");
            return Task.CompletedTask;
        }
    }
}
