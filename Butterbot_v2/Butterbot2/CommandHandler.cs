using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Butterbot2
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient client = ClientHandler.services.GetRequiredService<DiscordSocketClient>();
        private readonly CommandService commandService = Program.serviceProvider.GetRequiredService<CommandService>();

        public async Task InstallCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), Program.serviceProvider);
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            if (message is not SocketUserMessage msg) return;

            int argPos = 0;

            var foo = msg.HasCharPrefix('!' , ref argPos);
            var bar = msg.HasMentionPrefix(client.CurrentUser, ref argPos);
            var baz = msg.Author.IsBot;
            var sdfsdf = msg.ToString();
            var sdfsdf2 = msg.ToString();

            if (!msg.HasCharPrefix('!', ref argPos) ||
                msg.HasMentionPrefix(client.CurrentUser, ref argPos) ||
                msg.Author.IsBot)
                return;

            SocketCommandContext context = new(client, msg);

            await commandService.ExecuteAsync(context, argPos, Program.serviceProvider);
        }
    }
}
