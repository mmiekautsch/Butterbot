using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace Butterbot2
{
    // ClientHandler übernimmt die Initialisierung und das Stoppen des Bots
    public class ClientHandler(IServiceProvider services) : IHostedService
    {
        private readonly CommandHandlingService commandHandler = services.GetRequiredService<CommandHandlingService>();
        private readonly DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        private readonly TimerService timerService = services.GetRequiredService<TimerService>();

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            client.Log += Log;
            client.Ready += OnReady;
        
            await commandHandler.InstallCommandsAsync();
            await client.LoginAsync(TokenType.Bot, File.ReadAllText("token.txt"));
            await client.StartAsync();
        
            timerService.StartChannelInteractionTimer();
            timerService.StartSoundInteractionTimer();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            timerService.StopChannelInteractionTimer();
            timerService.StopSoundInteractionTimer();
        
            await client.StopAsync();
            await client.LogoutAsync();
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
