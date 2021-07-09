using Discord;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Planet.Modules
{
    public class Planet : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ExampleModule> _logger;

        public Planet(ILogger<ExampleModule> logger)
            => _logger = logger;

        // file organizer - add custom command to file (database)
        [Command("createDB")]
        public async Task CreateDBAsync()
        {
            await Context.Guild.CreateTextChannelAsync("planetdb");
            await ReplyAsync("Channel database was created: #planetdb");
        }

        [Command("embeddocs")] //embed Google Docs link
        public async Task embedItemAsync(string itemURL)
        {
            await ReplyAsync("Add embed description:");
            //var description = await InteractiveBase.

            var builder = new EmbedBuilder()
                .WithThumbnailUrl("https://cdn4.iconfinder.com/data/icons/free-colorful-icons/360/google_docs.png")
                .WithUrl(itemURL)
                .WithFooter("Today's date");
                //.WithTitle(title)
                //.WithDescription(description);

            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
        }

    }
}