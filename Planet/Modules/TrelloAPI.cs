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
using OAuth;
using System.Net;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Http;
using System;

namespace Planet.Modules
{
    public class TrelloAPI : ModuleBase<SocketCommandContext>
    {
        
        private readonly Utilities.Trello _trello;
        private readonly Servers _servers;

        public TrelloAPI(Utilities.Trello trello, Servers servers)
        {
            _trello = trello;
            _servers = servers;
        }

        private enum colour
        {
            none,
            blue,
            green,
            orange,
            purple,
            red,
            yellow,
            sky,
            lime,
            pink,
            black,
        }

        private EmbedAuthorBuilder trelloAuthor = new EmbedAuthorBuilder()
                .WithName("Planet Trello")
                .WithIconUrl("https://cdn3.iconfinder.com/data/icons/popular-services-brands-vol-2/512/trello-512.png");

        

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

        /*public static async Task Reaction(ISocketMessageChannel arg2, Optional<SocketUserMessage> message, SocketReaction arg3)
        {
        }*/

        private async Task<JArray> APIRequest(string url)
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync(url);

            var arr = new JArray();

            if (result.StartsWith("["))
            {
                arr = JArray.Parse(result);
                return arr;
            }
            else if (result.StartsWith("{"))
            {
                string addBrackets = "[" + result.ToString() + "]";
                arr = JArray.Parse(addBrackets);
                return arr;
            }
            else
            {
                await ReplyAsync("ERROR sending request.", false, null);
                return new JArray { };
            }
            
            
        }

        /*  [SET-UP]
        ---------------------------------------------------------------------*/

        /*[Command("createEmote")]
        public async Task CreateEmote()
        {
            HttpWebRequest aRequest =(HttpWebRequest)WebRequest.Create("https://emojipedia-us.s3.dualstack.us-west-1.amazonaws.com/thumbs/240/apple/285/ringed-planet_1fa90.png");
            HttpWebResponse aResponse = (HttpWebResponse)aRequest.GetResponse();
            var stream = aResponse.GetResponseStream();
            

            await Context.Guild.CreateEmoteAsync("testPlanetEmote", new Image(stream));
            await stream.DisposeAsync();

            

        }*/

        [Command("trellotoken")]
        public async Task AddTrelloToken(string token)
        {
            var planetGuild = Context.Client.GetGuild(864275714353135626);
            await Context.Message.DeleteAsync();
            await _trello.AddTokenAsync(Context.User, token);

            var arr = await APIRequest($"https://api.trello.com/1/tokens/{token}/member?key={Program.trelloKey}&token={token}");

            var memberInfo = arr
                    .Select(x => new
                    {
                        username = (string)x["username"],
                        id = (string)x["id"],
                        avatarHash = (string)x["avatarHash"],
                    })
                    .ToArray();

            var avatarUrl = $"https://trello-members.s3.amazonaws.com/{memberInfo[0].id}/{memberInfo[0].avatarHash}/original.png";

            // TO-DO: store user info into database

            HttpWebRequest aRequest = (HttpWebRequest)WebRequest.Create(avatarUrl);
            HttpWebResponse aResponse = (HttpWebResponse)aRequest.GetResponse();
            var stream = aResponse.GetResponseStream();

            await planetGuild.CreateEmoteAsync($"{memberInfo[0].username}", new Image(stream));
            await stream.DisposeAsync();

            IEmote emote = planetGuild.Emotes.First(e => e.Name == memberInfo[0].username);
            //message.AddReactionAsync(emote);

            var rMsg = await ReplyAsync($"{emote} {Context.User.Username}'s Trello token was updated.");
            //await rMsg.AddReactionAsync(planetGuild.Emotes.Where(x => x.Name == memberInfo[0].username).FirstOrDefault());
        }

        [Command("trelloBoard")]
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

            await ReplyAsync($"Your default board has been set to {arr.Where(x => x["name"].ToString().ToLower() == boardName.ToLower()).Select(x => x["name"].ToString()).FirstOrDefault()}.", false, null);
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

            // get cards in board
            JArray arr2 = await APIRequest($"https://api.trello.com/1/boards/{boardId}/cards?key={Program.trelloKey}&token={token}");

            // TO-DO: put in embed and add each

            var builder = new EmbedBuilder()
                .WithTitle("Lists in Board")
                .WithAuthor(trelloAuthor);

            for (int i = 0; i < arr.Count; i++)
            {
                // get list id
                var listId = arr
                    .Select(x => x["id"].ToString())
                    .ToArray();

                var numCards = arr2.Where(x => x["idList"].ToString() == listId[i]).Count();

                builder.AddField(arr[i]["name"].ToString(), $"{numCards} cards", false);
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

            //if (arr2.Where(x => x["idList"].ToString() == listId).Select(x => x["labels"].Count()).FirstOrDefault() > 0)
            var numCards = arr2.Where(x => x["idList"].ToString() == listId).Count();
            var numLabels = arr2.Where(x => x["idList"].ToString() == listId).Select(x => x["labels"].Count()).FirstOrDefault();


            var cards = arr2
                .Where(x => x["idList"].ToString() == listId)
                .Select(x => new
                {
                    name = (string)x["name"],
                    label = "No label",
                    labelColour = (string)x["name"],
                })
                .ToArray();

            if (numLabels > 0)
            {
                cards = arr2
                    .Where(x => x["idList"].ToString() == listId)
                    .Select(x => new
                    {
                        name = (string)x["name"],
                        label = (string)x["labels"][0]["name"],
                        labelColour = (string)x["labels"][0]["color"]
                    })
                    .ToArray();
            }

            var builder = new EmbedBuilder()
                .WithTitle($"{arr.Where(x => x["name"].ToString().ToLower() == listName.ToLower()).Select(x => x["name"].ToString()).FirstOrDefault()}")
                .WithAuthor(trelloAuthor);

            // TO-DO: get # of members, image manipulation w member icons as thumbnail/image?

            var emoji = "";

            /*
             * 0 = none X
             * 1 = blue 🔵
             * 2 = green 🟢
             * 3 = orange 🟠
             * 4 = purple 🟣
             * 5 = red 🔴
             * 6 = yellow 🟡
             * 7 = sky 🌐
             * 8 = lime 🥝
             * 9 = pink 🌸
             * 10 = black ⚫️*/

            for (int i = 0; i < numCards; i++)
            {
                Enum.TryParse(cards[i].labelColour, out colour labelColour);

                switch (labelColour)
                {
                    case colour.none:
                        emoji = "";
                        break;
                    case colour.blue:
                        emoji = "🔵";
                        break;
                    case colour.green:
                        emoji = "🟢";
                        break;
                    case colour.orange:
                        emoji = "🟠";
                        break;
                    case colour.purple:
                        emoji = "🟣";
                        break;
                    case colour.red:
                        emoji = "🔴";
                        break;
                    case colour.yellow:
                        emoji = "🟡";
                        break;
                    case colour.sky:
                        emoji = "🌐";
                        break;
                    case colour.lime:
                        emoji = "🥝";
                        break;
                    case colour.pink:
                        emoji = "🌸";
                        break;
                    case colour.black:
                        emoji = "⚫️";
                        break;
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
                    label = "[No label]",
                    labelColour = (string)x["closed"],
                    numMembers = (int)x["idMembers"].Count(),
                    //members = (string)x["idMembers"][0],
                    numChecklists = (int)x["idChecklists"].Count(),
                    //checklistId = (string)x["idChecklists"][0]
                })
                .ToArray();

            if (arr.Where(x => x["name"].ToString().ToLower() == cardName.ToLower()).Select(x => x["labels"].Count()).FirstOrDefault() > 0)
            {
                card = arr
                .Where(x => x["name"].ToString().ToLower() == cardName.ToLower())
                .Select(x => new
                {
                    desc = (string)x["desc"],
                    label = (string)x["labels"][0]["name"] ?? "",
                    labelColour = (string)x["labels"][0]["color"] ?? "",
                    numMembers = (int)x["idMembers"].Count(),
                    //members = (string)x["idMembers"][0],
                    numChecklists = (int)x["idChecklists"].Count(),
                    //checklistId = (string)x["idChecklists"][0]
                })
                .ToArray();
            }

            var emoji = "";
            Enum.TryParse(card[0].labelColour, out colour labelColour);

            switch (labelColour)
            {
                case colour.none:
                    emoji = "";
                    break;
                case colour.blue:
                    emoji = "🔵";
                    break;
                case colour.green:
                    emoji = "🟢";
                    break;
                case colour.orange:
                    emoji = "🟠";
                    break;
                case colour.purple:
                    emoji = "🟣";
                    break;
                case colour.red:
                    emoji = "🔴";
                    break;
                case colour.yellow:
                    emoji = "🟡";
                    break;
                case colour.sky:
                    emoji = "🌐";
                    break;
                case colour.lime:
                    emoji = "🥝";
                    break;
                case colour.pink:
                    emoji = "🌸";
                    break;
                case colour.black:
                    emoji = "⚫️";
                    break;
            }

            var builder = new EmbedBuilder()
                .WithTitle($"{arr.Where(x => x["name"].ToString().ToLower() == cardName.ToLower()).Select(x => x["name"].ToString()).FirstOrDefault()}")
                .WithAuthor(trelloAuthor);

            if (card[0].label.Length > 0)
            {
                builder.AddField("*Label*", $"{emoji} {card[0].label}", true);
            }

            if (card[0].desc.Length > 0)
            {
                builder.AddField("*Description*", card[0].desc, true);
            }
            else
            {
                builder.AddField("*Description*", "-", true);
            }

            for (int i = 0; i < card[0].numChecklists; i++)
            {
                var checklistIds = arr
                .Where(x => x["name"].ToString().ToLower() == cardName.ToLower())
                .Select(x => new
                {
                    checklistId = (string)x["idChecklists"][i]
                }).FirstOrDefault();

                var arr2 = await APIRequest($"https://api.trello.com/1/checklists/{checklistIds.checklistId}?key={Program.trelloKey}&token={token}");

                //await ReplyAsync($"https://api.trello.com/1/checklists/{checklistIds.checklistId}?key={Program.trelloKey}&token={token}");

                var numItems = arr2.Select(x => x["checkItems"].Count()).FirstOrDefault();

                var checklist = "-";
                var checklistName = arr2.Select(x => x["name"]).FirstOrDefault();

                if (numItems > 0)
                {
                    checklist = "";
                    for (int n = 0; n < numItems; n++)
                    {
                        var checkItems = arr2
                            .Select(x => new
                            {
                                name = (string)x["checkItems"][n]["name"],
                                state = (string)x["checkItems"][n]["state"],
                                checklistName = (string)x["name"],
                            })
                            .ToArray();

                        var check = Emote.Parse("<:incomplete:863835094976692224>");

                        if (checkItems[0].state == "complete")
                        {
                            check = Emote.Parse("<a:complete:863861873961992223>");
                        }

                        checklist += $"{check} {checkItems[0].name}\n";
                    }
                }
                
                builder.AddField($"*{checklistName}*", checklist, false);
            }

            var members = " -";
            var planetGuild = Context.Client.GetGuild(864275714353135626);

            if (card[0].numMembers > 0)
            {
                members = "";
                for (int i = 0; i < card[0].numMembers; i++)
                {
                    var memberIds = arr
                    .Where(x => x["name"].ToString().ToLower() == cardName.ToLower())
                    .Select(x => new
                    {
                        memberId = (string)x["idMembers"][i]
                    }).FirstOrDefault();

                    var arr2 = await APIRequest($"https://api.trello.com/1/members/{memberIds.memberId}?key={Program.trelloKey}&token={token}");

                    var memberInfo = arr2
                            .Select(x => new
                            {
                                name = (string)x["username"],
                                avatarHash = (string)x["avatarHash"],
                                avatarHashType = x["avatarHash"].Type,
                            })
                            .ToArray();

                    if (memberInfo[0].avatarHashType == JTokenType.Null)
                    {
                        members += $"\n{Emote.Parse("<:default:864367930489831484>")} @{memberInfo[0].name}";
                    }
                    else if (!planetGuild.Emotes.Any(x => x.Name == memberInfo[0].name))
                    {
                        var avatarUrl = $"https://trello-members.s3.amazonaws.com/{memberIds.memberId}/{memberInfo[0].avatarHash}/original.png";

                        //await ReplyAsync(avatarUrl, false, null);

                        HttpWebRequest aRequest = (HttpWebRequest)WebRequest.Create(avatarUrl);
                        HttpWebResponse aResponse = (HttpWebResponse)aRequest.GetResponse();
                        var stream = aResponse.GetResponseStream();

                        var emote = await planetGuild.CreateEmoteAsync($"{memberInfo[0].name}", new Image(stream));

                        await stream.DisposeAsync();
                        members += $"\n{emote} @{memberInfo[0].name}";
                    }
                    else
                    {
                        members += $"\n{planetGuild.Emotes.First(x => x.Name == memberInfo[0].name)} @{memberInfo[0].name}";
                    }
                }
            }

            members = members.Substring(1);
            builder.AddField("*Members:*", members, false);

            // TO-DO: get # of members, image manipulation w member icons as thumbnail/image?
            // or custom emoji with members

            var embed = builder.Build();

            var msg = await ReplyAsync(null, false, embed);

        }
    }
}