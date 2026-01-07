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

        private readonly Timer ChannelInteractionTimer = new(300000); // 5 Minutes
        private readonly Timer SoundInteractionTimer = new(30000); // 30 Seconds

        private Random random = new(Seed: 69); // nice

        public TimerService(IServiceProvider _services)
        {
            services = _services;
            client = services.GetRequiredService<DiscordSocketClient>();
            
            ChannelInteractionTimer.Elapsed += ChannelInteractionTimer_Elapsed;
            ChannelInteractionTimer.AutoReset = true;
            
            SoundInteractionTimer.Elapsed += SoundInteractionTimer_Elapsed;
            SoundInteractionTimer.AutoReset = true;
        }

        // will keep for later use maybe
        public void CreateTimer(double interval_ms, ElapsedEventHandler handler, bool autoReset = false)
        {
            Timer timer = new(interval_ms);
            timer.Elapsed += handler;
            timer.AutoReset = autoReset;
            timer.Enabled = true;
            Timers.Add(timer);
        }

        private void JoinRandomChannel(IEnumerable<IVoiceChannel> channels)
        {
            var channelsList = channels.ToList();
            var channelToJoin = channelsList[random.Next(channelsList.Count)];
            channelToJoin.ConnectAsync().Wait();
        }

        private void LeaveCurrentChannel()
        {
            var botChannel = (client.CurrentUser as IGuildUser)?.VoiceChannel;
            botChannel?.DisconnectAsync().Wait();
            // maybe need to disconnect from sound service too here idk
        }

        private void ChannelInteractionTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            // get current channel id of bot
            var botChannel = (client.CurrentUser as IGuildUser)?.VoiceChannel;

            // check if bot is in a channel alone :(
            bool botIsAlone = botChannel != null && (botChannel as SocketVoiceChannel)!.ConnectedUsers.All(u => u.Id == client.CurrentUser.Id);

            // get list of voice channels
            var joinableChannels = client.Guilds.First().VoiceChannels
                .Where(c =>
                    c.Id != botChannel?.Id && // that the bot is not connected to
                    c.ConnectedUsers.Count > 0 && // with connected users
                    !c.ConnectedUsers.All(u => u.IsBot) // who are not all bots
                    );

            if (botChannel != null)
            {
                if (botIsAlone)
                {
                    if (joinableChannels.Any())
                    {
                        JoinRandomChannel(joinableChannels);
                    }
                    else
                    {
                        LeaveCurrentChannel();
                    }
                }
                else
                {
                    if (joinableChannels.Any())
                    {
                        if (random.Next(100) < 10)
                        {
                            JoinRandomChannel(joinableChannels);
                        }
                    }
                    else
                    {
                        if (random.Next(100) < 5)
                        {
                            LeaveCurrentChannel();
                        }
                    }
                }
            }
            else
            {
                if (joinableChannels.Any())
                {
                    if (random.Next(100) < 60)
                    {
                        JoinRandomChannel(joinableChannels);
                    }
                }
            }
        }

        private void SoundInteractionTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var botChannel = (client.CurrentUser as IGuildUser)?.VoiceChannel;
            // TODO
        }

        public void StartChannelInteractionTimer()
        {
            ChannelInteractionTimer.Enabled = true;
        }

        public void StartSoundInteractionTimer()
        {
            SoundInteractionTimer.Enabled = true;
        }

        public void StopChannelInteractionTimer()
        {
            ChannelInteractionTimer.Enabled = false;
        }

        public void StopSoundInteractionTimer()
        {
            SoundInteractionTimer.Enabled = false;
        }
    }
}
