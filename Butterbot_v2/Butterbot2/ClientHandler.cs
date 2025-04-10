using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Timers;

namespace Butterbot2
{
    public class ClientHandler
    {
        private static readonly ulong butterChannel = 1208189932770959400;
        private CommandHandlingService commandHandler;
        private readonly DiscordSocketClient client;
        private static IServiceProvider services;

        public ClientHandler(IServiceProvider _services)
        {
            services = _services;
            client = services.GetRequiredService<DiscordSocketClient>();
            commandHandler = services.GetRequiredService<CommandHandlingService>();
            InitializeAsync().Wait();
        }

        public async Task InitializeAsync()
        {
            client.Log += Log;
            client.Ready += OnReady;
            //timer.Elapsed += OnTimerElapsed;
            await commandHandler.InstallCommandsAsync();
            await client.LoginAsync(TokenType.Bot, File.ReadAllText("token.txt"));
            await client.StartAsync();
        }

        private Task Log(LogMessage msg)
        {
            Trace.WriteLine(msg.ToString());
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task OnReady()
        {
            client.SetGameAsync("dumme scheiße ab");
            //(client.GetChannel(butterChannel) as IMessageChannel)?.SendMessageAsync("Butter is back online bitches");
            // connect to butter channel
            IVoiceChannel? channel = client.GetChannel(butterChannel) as IVoiceChannel;

            return Task.CompletedTask;
        }
    }
}
