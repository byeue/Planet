using Discord;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json.Linq;

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

        [Command("apitest")]
        [Alias("reddit")]
        public async Task apiTest(string subreddit = null)
        {
            await ReplyAsync("testing reddit api..");

            //reddit
            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");
            if (!result.StartsWith("["))
            {
                await ReplyAsync("This subreddit doesn't exist.", false, null);
                return;
            }
            /*JArray arr = JArray.Parse(result);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

            var builder = new EmbedBuilder()
                .WithImageUrl(post["url"].ToString())
                .WithTitle(post["title"].ToString())
                .WithUrl("https://reddit.com" + post["permalink"].ToString())
                .WithFooter($"\U0001F5E9 {post["num_comments"]} \u2191 {post["ups"]}");

            var embed = builder.Build();
            await ReplyAsync(null, false, embed);*/
            await ReplyAsync($"```{result.Substring(0, 1900)}```", false, null);
        }
    }
}