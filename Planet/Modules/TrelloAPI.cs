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
using Planet.Utilities;
using Infrastructure;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Planet.Modules
{
    public class TrelloAPI : ModuleBase<SocketCommandContext>
    {
        private readonly Utilities.Trello _trello;
        private readonly Servers _servers;

        private EmbedAuthorBuilder trelloAuthor = new EmbedAuthorBuilder()
                .WithName("Planet Trello")
                .WithIconUrl("https://cdn3.iconfinder.com/data/icons/popular-services-brands-vol-2/512/trello-512.png");

        public TrelloAPI(Utilities.Trello trello, Servers servers)
        {
            _trello = trello;
            _servers = servers;
        }

        private async Task<string> CheckToken()
        {
            var token = await _trello.GetTokenAsync(Context.User) ?? "";
            if (token == null)
            {
                await ReplyAsync("Please add your Trello token with +trellotoken.", false, null);
                return "";
            }
            return token;
        }

        private async Task<string> CheckBoardId()
        {
            var boardId = await _trello.GetDefaultBoardIdAsync(Context.User) ?? "";

            if (boardId == null)
            {
                await ReplyAsync("Please set a default board using +trellosetdefault OR specify a board name using +trellolists *boardName*.", false, null);
                return "";
            }
            return boardId;
        }

        private async Task<JArray> APIRequest(string url)
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync(url);

            if (!result.StartsWith("["))
            {
                await ReplyAsync("ERROR sending request.", false, null);
                return new JArray { };
            }

            JArray arr = JArray.Parse(result);
            return arr;
        }

        /*  [SET-UP]
        ---------------------------------------------------------------------*/

        [Command("trellotoken")]
        public async Task AddTrelloToken(string token)
        {
            await _trello.AddTokenAsync(Context.User, token);
            await ReplyAsync($"{Context.User.Username}'s Trello token was updated.");
        }

        [Command("trelloSetBoard")]
        public async Task TrelloSetDefault([Remainder]string boardName)
        {
            var token = await CheckToken();

            // get boards in user account
            JArray arr = await APIRequest($"https://api.trello.com/1/members/me/boards?key={Program.trelloKey}&token={token}");

            var boardId = arr
                .Where(x => x["name"].ToString().ToLower() == boardName.ToLower())
                .Select(x => x["id"].ToString())
                .FirstOrDefault();

            await _trello.SetDefaultBoardAsync(Context.User, boardId);

            await ReplyAsync($"Your default board has been set to {boardName}.", false, null);
        }

        /*  [GET/VIEWING]
        ---------------------------------------------------------------------*/

        [Command("trelloLists")]
        public async Task TrelloLists([Remainder]string boardName = null)
        {
            var token = await CheckToken();
            var boardId = await CheckBoardId();

            // get lists in boards
            JArray arr = await APIRequest($"https://api.trello.com/1/boards/{boardId}/lists?key={Program.trelloKey}&token={token}");

            // TO-DO: put in embed and add each

            var builder = new EmbedBuilder()
                .WithTitle("Lists in Board")
                .WithAuthor(trelloAuthor);

            for (int i = 0; i < arr.Count; i++)
            {
                builder.AddField(arr[i]["name"].ToString(), "x cards", false);
            }

            // TO-DO: get number of cards in a list

            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
        }

        [Command("trelloList")]
        public async Task TrelloList([Remainder]string listName)
        {

            var token = await CheckToken();
            var boardId = await CheckBoardId();

            // get lists in board 
            JArray arr = await APIRequest($"https://api.trello.com/1/boards/{boardId}/lists?key={Program.trelloKey}&token={token}");

            var listId = arr
                .Where(x => x["name"].ToString().ToLower() == listName.ToLower())
                .Select(x => x["id"].ToString())
                .FirstOrDefault();

            // get cards in board
            JArray arr2 = await APIRequest($"https://api.trello.com/1/boards/{boardId}/cards?key={Program.trelloKey}&token={token}");

            var cards = arr2
                .Where(x => x["idList"].ToString() == listId)
                .Select(x => new
                {
                    name = (string)x["name"],
                    label = (string)x["labels"][0]["name"],
                    labelColour = (string)x["labels"][0]["color"]
                })
                .ToArray();

            var builder = new EmbedBuilder()
                .WithTitle($"{arr.Where(x => x["name"].ToString().ToLower() == listName.ToLower()).Select(x => x["name"].ToString()).FirstOrDefault()}")
                .WithAuthor(trelloAuthor);

            // TO-DO: get # of members, image manipulation w member icons as thumbnail/image?

            var emoji = "";

            for (int i = 0; i < arr.Count; i++)
            {
                if (cards[i].labelColour == "lime")
                {
                    emoji = "🥝";
                }
                else if (cards[i].labelColour == "pink")
                {
                    emoji = "🌸";
                }
                else if (cards[i].labelColour == "sky")
                {
                    emoji = "🌐";
                }

                builder.AddField($"*{cards[i].name}*", $"{emoji} [{cards[i].label}]", false);
            }

            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
        }

        [Command("trelloCard")]
        public async Task TrelloCard([Remainder]string cardName)
        {
            var token = await CheckToken();
            var boardId = await CheckBoardId();

            // get cards in board
            JArray arr = await APIRequest($"https://api.trello.com/1/boards/{boardId}/cards?key={Program.trelloKey}&token={token}");

            var card = arr
                .Where(x => x["name"].ToString().ToLower() == cardName.ToLower())
                .Select(x => new
                {
                    desc = (string)x["desc"],
                    label = (string)x["labels"][0]["name"] ?? "",
                    labelColour = (string)x["labels"][0]["color"] ?? "",
                    members = (string)x["idMembers"][0],
                    checklistId = (string)x["idChecklists"][0]
                })
                .ToArray();

            var emoji = "";
            if (card[0].labelColour == "lime")
            {
                emoji = "🥝";
            }
            else if (card[0].labelColour == "pink")
            {
                emoji = "🌸";
            }
            else if (card[0].labelColour == "sky")
            {
                emoji = "🌐";
            }

            var builder = new EmbedBuilder()
                .WithTitle($"{arr.Where(x => x["name"].ToString().ToLower() == cardName.ToLower()).Select(x => x["name"].ToString()).FirstOrDefault()}")
                .WithAuthor(trelloAuthor)
                .AddField("*Description*", card[0].desc, false)
                .AddField("*Label*", $"{emoji} {card[0].label}", false);

            // TO-DO: get checklist
            // TO-DO: get # of members, image manipulation w member icons as thumbnail/image?

            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
        }
    }
}