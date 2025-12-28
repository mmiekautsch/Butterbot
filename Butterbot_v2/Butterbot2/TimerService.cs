using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Butterbot2
{
    public class TimerService
    {
        private readonly DiscordSocketClient client;
        private readonly IServiceProvider services;

        private readonly List<Timer> Timers = [];
        
        public TimerService(IServiceProvider _services)
        {
            services = _services;
            client = services.GetRequiredService<DiscordSocketClient>();
        }

        public void CreateTimer(double interval_ms, ElapsedEventHandler handler, bool autoReset = false)
        {
            Timer timer = new(interval_ms);
            timer.Elapsed += handler;
            timer.AutoReset = autoReset;
            timer.Enabled = true;
            Timers.Add(timer);
        }

        // maybe need a way to remove timers later?
    }
}
