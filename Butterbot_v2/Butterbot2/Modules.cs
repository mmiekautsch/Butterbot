using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.

namespace Butterbot2
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("say")]
        [Alias("echo")]
        [Summary("Echoes a message.")]
        public async Task SayAsync([Remainder][Summary("The text to echo")] string echo)
            => await ReplyAsync(echo);

        [Command("ping")]
        [Summary("Returns the latency of the bot.")]
        public async Task PingAsync()
            => await ReplyAsync($"Pong! Latency: {Context.Client.Latency}ms");
    }

    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [CheckIfUserIsAdmin]
        [Command("exit")]
        [Alias("shutdown")]
        public async Task ExitAsync()
        {
            Environment.Exit(0);
        }

        [CheckIfUserIsAdmin]
        [Command("reconnect", RunMode = RunMode.Async)]
        public async Task ReconnectAsync()
        {
            IVoiceChannel? botChannel = (Context.Client.CurrentUser as IGuildUser)?.VoiceChannel;
            if (botChannel != null)
            {
                await botChannel.DisconnectAsync();
                await botChannel.ConnectAsync(external: true);
            }
        }
    }

    public class ButterCommand(IServiceProvider services) : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();

        [Command("butter", RunMode = RunMode.Async)]
        [Alias("kommher", "bielebiele")]
        public async Task RequestJoinAsync()
        {
            IVoiceChannel? userChannel = (Context.User as IGuildUser)?.VoiceChannel;
            IVoiceChannel? botChannel = (Context.Client.CurrentUser as IGuildUser)?.VoiceChannel;

            if (userChannel == null)
            {
                await ReplyAsync("Wir treffen uns in \"einfach Quasseln\", ohne treten. Sag nochmal !butter wenn du bereit bist.");
                return;
            }

            if (userChannel == (client.CurrentUser as IGuildUser)?.VoiceChannel)
            {
                await Context.Channel.SendFileAsync("butterdog.jpg");
                return;
            }

            // join users channel
            if (botChannel == null)
            {
                await userChannel.ConnectAsync(external: true);
            }
            else
            {
                
            }
        }
    }
}