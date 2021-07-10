using Discord;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Infrastructure;

namespace Planet.Modules
{
    public class ExampleModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ExampleModule> _logger;
        private readonly Servers _servers;

        public ExampleModule(ILogger<ExampleModule> logger, Servers servers)
        {
            _logger = logger;
            _servers = servers;
        }

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

        [Command("prefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Prefix(string prefix = null)
        {
            if (prefix == null)
            {
                var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "+";
                await ReplyAsync($"The current prefix is `{guildPrefix}`", false, null);
                return;
            }

            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"The prefix has been changed to `{prefix}`");
        }

    }
}