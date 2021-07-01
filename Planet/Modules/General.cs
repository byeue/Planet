using Discord;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Planet.Modules
{
    public class ExampleModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ExampleModule> _logger;

        public ExampleModule(ILogger<ExampleModule> logger)
            => _logger = logger;

        [Command("ping")]
        public async Task PingAsync()
        {
            await ReplyAsync("pong");
        }

        [Command("purge")] //testing only
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PurgeAsync(int amount)
        {
            var channel = Context.Channel as SocketTextChannel;

            var messages = await channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await channel.DeleteMessagesAsync(messages);

            var message = await channel.SendMessageAsync($"{messages.Count()} messages deleted.");
            await Task.Delay(2500);
            await message.DeleteAsync();

        }

    }
}