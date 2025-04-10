using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.

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
        [Command("reconnect")]
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
        private static readonly ulong butterChannel = 1208189932770959400;
        private readonly SoundService soundService = services.GetRequiredService<SoundService>();
        private readonly DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        private readonly UserService userService = services.GetRequiredService<UserService>();

        [Command("butter")]
        [Alias("kommher", "bielebiele")]
        public async Task RequestJoinAsync()
        {
            IVoiceChannel? userChannel = (Context.User as IGuildUser)?.VoiceChannel;

            if (userChannel == null)
            {
                await ReplyAsync("Wir treffen uns in \"einfach Quasseln\", ohne treten. Sag nochmal !butter wenn du bereit bist.");
                return;
            }

            //if (userChannel )
            //{
            //    await Context.Channel.SendFileAsync("butterdog.jpg");
            //    return;
            //}

            soundService.AudioClient = await userChannel.ConnectAsync();
            await soundService.PlayChannelJoinSound();
        }

        [CheckUserInfo]
        [Command("cat")]
        public async Task SendCat()
        {
            var catService = services.GetRequiredService<CatService>();
            var stream = await catService.GetCatPictureAsync();
            var image = new FileAttachment(stream, "cat.jpg");
            await Context.Channel.SendFileAsync(image);
        }

        [CheckUserInfo]
        [Command("machwas")]
        public async Task PlaySound()
        {
            if (soundService.AudioClient == null)
            {
                await ReplyAsync("Ich bin nicht in einem Voice-Channel.");
                return;
            }
            await soundService.PlayRandomSound();
            await (client.GetChannel(butterChannel) as IMessageChannel).SendMessageAsync($"Butter präsentiert: ```{soundService.CurrentlyPlaying}```");
        }

        [CheckUserInfo]
        [Command("otz")]
        public async Task PlayOtz(int userNum)
        {
            if (userNum > 1 || userNum < 20)
            {
                await ReplyAsync("Du musst ne zahl zwischen 1 und 20 sagen.");
                return;
            }

            int botNum = new Random().Next(1, 21);
            userService.Users.TryGetValue(Context.User.Id, out UserService.Info info);
            if (userNum == botNum)
            {
                info.LastSoundCommand = DateTime.MinValue;
                if (DateTime.Now - info.LastSoundCommand < userService.SoundCommandCooldown)
                {
                    await ReplyAsync("Gewinne Gewinne Gewinne! Du hattest nicht mal cooldown, Macher");
                }
                else
                {
                    await ReplyAsync("Gewinne Gewinne Gewinne! Dein !machwas cooldown wurde zurückgesetzt.");
                }
            }
            else
            {
                info.FailedOtzAttempts++;
                info.LastFailedOtzAttempt = DateTime.Now;
                await ReplyAsync($"Es war {botNum} lol. Womp Womp :(");
            }
        }
    }
}