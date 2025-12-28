using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

#pragma warning disable CS8603 // Mögliche Nullverweisrückgabe.

namespace Butterbot2
{
    public class SoundService
    {
        private readonly DiscordSocketClient client;
        private readonly IServiceProvider services;
        private CancellationTokenSource stopper;
        private static readonly Random random = new();

        public string CurrentlyPlaying { get; private set; } = string.Empty;

        public IAudioClient? AudioClient { get; set; }

        public bool IsPlaying { get; private set; } = false;

        public SoundService(IServiceProvider _services)
        {
            services = _services;
            client = services.GetRequiredService<DiscordSocketClient>();
            stopper = new CancellationTokenSource();
        }

        private Process CreateStream(string path)
        {
            if (stopper.IsCancellationRequested)
            {
                stopper.Dispose();
                stopper = new CancellationTokenSource();
            }
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
        }

        private static string GetRandomSound()
        {
            string[] sounds = Directory.GetFiles("C:\\Users\\dasmo\\Documents\\Butterbot\\sounds", "*.mp3");
            return sounds[random.Next(sounds.Length)];
        }

        public async Task PlayRandomSound()
        {
            if (AudioClient == null) return;

            string path = GetRandomSound();
            CurrentlyPlaying = Path.GetFileNameWithoutExtension(path);

            IsPlaying = true;
            try 
            { 
                using var ffmpeg = CreateStream(path);
                using var output = ffmpeg.StandardOutput.BaseStream;
                using var discord = AudioClient.CreatePCMStream(AudioApplication.Music/*, bufferMillis: 500*/);
                try { await output.CopyToAsync(discord, stopper.Token); }
                finally { await discord.FlushAsync(); }
            }
            catch (TaskCanceledException) { }

            IsPlaying = false;
        }

        public async Task PlayChannelJoinSound()
        {
            if (AudioClient == null) return;

            using var ffmpeg = CreateStream(Directory.GetCurrentDirectory() + "\\res\\channel_join.mp3");
            using var output = ffmpeg.StandardOutput.BaseStream;
            using var discord = AudioClient.CreatePCMStream(AudioApplication.Mixed);
            try { await output.CopyToAsync(discord); }
            finally { await discord.FlushAsync(); }
        }

        public void StopSound()
        {
            if (IsPlaying)
            {
                stopper.Cancel();
                CurrentlyPlaying = string.Empty;
                IsPlaying = false;
            }
        }
    }
}
