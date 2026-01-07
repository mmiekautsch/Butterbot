using Discord;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Extensions;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.DependencyInjection;

namespace Butterbot2
{
    public class SoundService(IServiceProvider services)
    {
        private readonly DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        private readonly IAudioService audioService = services.GetRequiredService<IAudioService>();

        private static readonly Random random = new();
        private static readonly ulong butterChannel = 1208189932770959400;
        private LavalinkPlayer? player;

        private ulong GuildId => client.Guilds.First().Id;
        public string CurrentlyPlaying { get; private set; } = string.Empty;
        public bool IsPlaying => player?.State == PlayerState.Playing;

        private async Task<LavalinkPlayer?> GetCurrentPlayerAsync()
        {
            return await audioService.Players.GetPlayerAsync<LavalinkPlayer>(GuildId);
        }

        private static string GetRandomSound()
        {
            string[] sounds = Directory.GetFiles("C:\\Users\\dasmo\\Documents\\Butterbot\\sounds", "*.mp3");
            return sounds[random.Next(sounds.Length)];
        }

        private async Task<LavalinkPlayer> GetOrJoinPlayerAsync(ulong voiceChannelId)
        {
            var existingPlayer = await GetCurrentPlayerAsync();

            if (existingPlayer is not null)
            {
                player = existingPlayer;
                if (existingPlayer.VoiceChannelId != voiceChannelId)
                {
                    // Verschiebe Player zum neuen Channel
                    await existingPlayer.DisconnectAsync();
                    return await JoinChannelAsync(voiceChannelId);
                }

                return existingPlayer;
            }

            // Kein Player existiert, erstelle neuen
            return await JoinChannelAsync(voiceChannelId);
        }

        private async Task<LavalinkPlayer> JoinChannelAsync(ulong voiceChannelId)
        {
            player = await audioService.Players.JoinAsync(
                GuildId,
                voiceChannelId,
                playerFactory: PlayerFactory.Default,
                options: new LavalinkPlayerOptions(),
                cancellationToken: default);
            
            return player;
        }

        public async Task PlayRandomSound(ulong voiceChannelId)
        {
            var soundPath = GetRandomSound();
            var fileName = Path.GetFileName(soundPath);

            var track = await audioService.Tracks.LoadTrackAsync(soundPath, TrackSearchMode.None);

            if (track is null)
            {
                throw new InvalidOperationException($"Track konnte nicht geladen werden: {soundPath}");
            }

            var currentPlayer = await GetOrJoinPlayerAsync(voiceChannelId);

            CurrentlyPlaying = fileName;
            await (client.GetChannel(butterChannel) as IMessageChannel)!.SendMessageAsync($"Butter präsentiert: ```{CurrentlyPlaying}```");
            await currentPlayer.PlayAsync(track);
        }

        public async Task PlayChannelJoinSound(ulong voiceChannelId)
        {
            var joinSoundPath = Path.Combine(Directory.GetCurrentDirectory(), "res", "channel_join.mp3");

            var track = await audioService.Tracks.LoadTrackAsync(joinSoundPath, TrackSearchMode.None);

            if (track is null)
            {
                throw new InvalidOperationException("Channel join sound konnte nicht geladen werden");
            }

            var currentPlayer = await GetOrJoinPlayerAsync(voiceChannelId);

            CurrentlyPlaying = "channel_join.mp3";
            await currentPlayer.PlayAsync(track);
        }

        public async Task StopSoundAsync()
        {
            if (player is not null)
            {
                await player.StopAsync();
                CurrentlyPlaying = string.Empty;
            }
        }

        public async Task DisconnectAsync()
        {
            if (player is not null)
            {
                await player.DisconnectAsync();
                player = null;
                CurrentlyPlaying = string.Empty;
            }
        }
    }
}
