using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Reflection;

namespace Butterbot2
{
    public class CommandHandlingService
    {
        private readonly IServiceProvider services;
        private readonly DiscordSocketClient client;
        private readonly CommandService commandService;

        public CommandHandlingService(IServiceProvider _services)
        {
            services = _services;
            client = services.GetRequiredService<DiscordSocketClient>();
            commandService = services.GetRequiredService<CommandService>();
        }

        public async Task InstallCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            commandService.Log += LogAsync;
            commandService.CommandExecuted += CommandExecutedAsync;
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (result.IsSuccess) 
                return;

            if (!command.IsSpecified)
            {
                await context.Channel.SendMessageAsync($"{context.Message.Content} kenn ich ni");
                return;
            }

            switch (result.Error)
            {
                case CommandError.BadArgCount:
                    await context.Channel.SendMessageAsync($"{context.User.GlobalName}, ich hoffe du kriegst Husten");
                    break;
                case CommandError.UnmetPrecondition:
                    await context.Channel.SendMessageAsync($"{result.ErrorReason}");
                    break;
                default:
                    await context.Channel.SendMessageAsync("Irgendwas is passiert, Olli bescheid sagen (╯°□°）╯︵ ┻━┻");
                    break;
            }
        }

        private Task LogAsync(LogMessage message)
        {
            Trace.WriteLine(message.ToString());
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            if (message is not SocketUserMessage msg) return;

            int argPos = 0;

            if (!msg.HasCharPrefix('!', ref argPos) ||
                msg.HasMentionPrefix(client.CurrentUser, ref argPos) ||
                msg.Author.IsBot)
                return;

            SocketCommandContext context = new(client, msg);

            await commandService.ExecuteAsync(context, argPos, services);
        }
    }
}
