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
        private readonly DiscordSocketClient client;
        private System.Timers.Timer timer;
        private CommandHandler commandHandler;

        public static IServiceProvider services;

        public ClientHandler(IServiceProvider _services)
        {
            services = _services;
            client = services.GetRequiredService<DiscordSocketClient>();
            timer = new(5000);
            commandHandler = new();
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
            timer.Start();

            return Task.CompletedTask;
        }

        private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            IVoiceChannel? channel = (client.CurrentUser as IGuildUser)?.VoiceChannel;
            // bot not connected
            if (channel == null)
                return;
            // bot in afk channel
            if (channel.Id == 1136646942101880882)
                return;
        }
    }
}
