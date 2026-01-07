using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace Butterbot2
{
    public class Program
    {
        private static Process? lavalinkProcess;

        public static async Task Main(string[] args)
        {
            await EnsureLavalinkRunningAsync();
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(new DiscordSocketConfig
                    {
                        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
                    })
                    .AddSingleton(new CommandServiceConfig
                    {
                        DefaultRunMode = RunMode.Async,
                        LogLevel = LogSeverity.Debug,
                        CaseSensitiveCommands = false,
                    })
                    .AddSingleton<DiscordSocketClient>()
                    .AddSingleton<CommandService>()
                    .AddSingleton<CommandHandlingService>()
                    .AddSingleton<HttpClient>()
                    .AddSingleton<CatService>()
                    .AddSingleton<UserService>()
                    .AddSingleton<SoundService>()
                    .AddSingleton<TimerService>()
                    .AddLavalink()
                    .ConfigureLavalink(options =>
                    {
                        options.BaseAddress = new Uri("http://localhost:2333/");
                        options.Passphrase = "youshallnotpass";
                        options.ReadyTimeout = TimeSpan.FromSeconds(10);
                    })
                    .AddHostedService<ClientHandler>();
                })
                .Build();

            // if lavalink is a child process, setup graceful shutdown
            if (lavalinkProcess is not null)
            {
                AppDomain.CurrentDomain.ProcessExit += (s, e) => lavalinkProcess?.Kill();
                Console.CancelKeyPress += (s, e) => lavalinkProcess?.Kill();
            }
            await host.RunAsync();
        }

        private static async Task EnsureLavalinkRunningAsync()
        {
            if (await IsLavalinkRunningAsync())
            {
                return;
            }

            Console.WriteLine("Starte Lavalink Server...");

            var lavalinkPath = Path.Combine(Directory.GetCurrentDirectory(), "Lavalink", "Lavalink.jar");

            if (!File.Exists(lavalinkPath))
            {
                throw new FileNotFoundException($"Lavalink.jar nicht gefunden: {lavalinkPath}");
            }

            lavalinkProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = $"-jar \"{lavalinkPath}\"",
                    WorkingDirectory = Path.GetDirectoryName(lavalinkPath),
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            lavalinkProcess.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine($"[Lavalink] {e.Data}");
            };

            lavalinkProcess.Start();
            lavalinkProcess.BeginOutputReadLine();

            // Warte bis Lavalink bereit ist
            var timeout = DateTime.Now.AddSeconds(30);
            while (DateTime.Now < timeout)
            {
                if (await IsLavalinkRunningAsync())
                {
                    Console.WriteLine("Lavalink Server running.");
                    return;
                }
                await Task.Delay(500);
            }

            throw new TimeoutException("Lavalink konnte nicht innerhalb von 30 Sekunden gestartet werden");
        }

        private static async Task<bool> IsLavalinkRunningAsync()
        {
            try
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                var response = await httpClient.GetAsync("http://localhost:2333/version");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}