using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

namespace Butterbot2
{
    public class CommandHandler(DiscordSocketClient _client, CommandService _commandService)
    {
        private readonly DiscordSocketClient client = _client;
        private readonly CommandService commandService = _commandService;

        public string cmdPrefix = "!";

        public async Task InstallCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            if (message is not SocketUserMessage msg) return;

            int argPos = 0;

            if (!msg.HasStringPrefix(cmdPrefix, ref argPos) ||
                msg.HasMentionPrefix(client.CurrentUser, ref argPos) ||
                msg.Author.IsBot) 
                return;

            SocketCommandContext context = new(client, msg);

            await commandService.ExecuteAsync(context, argPos, null);
        }
    }
}
