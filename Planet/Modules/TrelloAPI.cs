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
using System.Net.Http;
using Microsoft.AspNetCore.Http;

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

        [Command("trelloauth")]
        public async Task Authorize()
        {/*
            string requestURL = "https://trello.com/1/OAuthGetRequestToken";
            string accessURL = "https://trello.com/1/OAuthGetAccessToken";
            string authorizeURL = "https://trello.com/1/OAuthAuthorizeToken";
            string appName = "Planet";
            string scope = "read,write,account";
            string expiration = "never";
            string trellokey = Program.trelloKey;
            string trellosecret = Program.trelloSecret;
            string responseToken = "";
            string responseTokenSecret = "";

            //  https://trello.com/1/OAuthAuthorizeToken?expiration=never&name=Planet&scope=read,write,account&response_type=token&key={trellokey}
            
            //request token, accesstoken, 
            OAuthRequest client = new OAuthRequest
            {
                Method = "GET",
                Type = OAuthRequestType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ConsumerKey = trellokey,
                ConsumerSecret = trellosecret,
                RequestUrl = requestURL,
                Version = "1.0a",
                Realm = "trello.com"
            };

            string auth = client.GetAuthorizationQuery();

            var url = client.RequestUrl + "?" + auth;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = true;
            var response = (HttpWebResponse)request.GetResponse();
            */
            /*var encoding = ASCIIEncoding.ASCII;
            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
            {
                string responseText = reader.ReadToEnd();
                await ReplyAsync($"request url sent. the response is: {responseText}.");
            }*/
            
            

            //const oauth = new OAuth(requestURL, accessURL, key, secret, "1.0A", loginCallback, "HMAC-SHA1")
        }

        [Command("trellotoken")]
        public async Task AddTrelloToken(string token)
        {
            await Context.Message.DeleteAsync();
            await _trello.AddTokenAsync(Context.User, token);
            await ReplyAsync($"{Context.User.Username}'s Trello token was updated.");
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

            //if (arr2.Where(x => x["idList"].ToString() == listId).Select(x => x["labels"].Count()).FirstOrDefault() > 0)
            var numCards = arr2.Where(x => x["idList"].ToString() == listId).Count();

            var cards = arr2
            .Where(x => x["idList"].ToString() == listId)
            .Select(x => new
            {
                name = (string)x["name"],
                label = "No label",
                labelColour = (string)x["name"],
            })
            .ToArray();

            if (arr2.Where(x => x["idList"].ToString() == listId).Select(x => x["labels"].Count()).FirstOrDefault() > 0)
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

            /*cards = arr2
            .Where(x => x["idList"].ToString() == listId)
            .Where(x => x["labels"].Count() > 0)
            .Select(x => new
            {
                name = (string)x["name"],
                label = (string)x["labels"][0]["name"],
                labelColour = (string)x["labels"][0]["color"]
            })
            .ToArray();*/


            var builder = new EmbedBuilder()
                .WithTitle($"{arr.Where(x => x["name"].ToString().ToLower() == listName.ToLower()).Select(x => x["name"].ToString()).FirstOrDefault()}")
                .WithAuthor(trelloAuthor);

            // TO-DO: get # of members, image manipulation w member icons as thumbnail/image?

            var emoji = "";

            for (int i = 0; i < numCards; i++)
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
                    label = "[No label]",
                    labelColour = (string)x["closed"],
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
                    //members = (string)x["idMembers"][0],
                    numChecklists = (int)x["idChecklists"].Count(),
                    //checklistId = (string)x["idChecklists"][0]
                })
                .ToArray();
            }
            /*card = arr
                .Where(x => x["name"].ToString().ToLower() == cardName.ToLower())
                .Select(x => new
                {
                    desc = (string)x["desc"],
                    label = (string)x["labels"][0]["name"] ?? "",
                    labelColour = (string)x["labels"][0]["color"] ?? "",
                    //members = (string)x["idMembers"][0],
                    numChecklists = (int)x["idChecklists"].Count(),
                    //checklistId = (string)x["idChecklists"][0]
                })
                .ToArray();*/


            //await ReplyAsync($"{arr2[0]["checkItems"][0]["name"]}");



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
                .WithAuthor(trelloAuthor);

            if (card[0].label.Length > 0)
            {
                builder.AddField("*Label*", $"{emoji} {card[0].label}", true);
            }

            if (card[0].desc.Length > 0)
            {
                builder.AddField("*Description*", card[0].desc, true);
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

                var numItems = arr2.Select(x => x["checkItems"].Count()).FirstOrDefault();

                var checklist = "-";

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
                            })
                            .ToArray();

                        var check = Emote.Parse("<:incomplete:863835094976692224>");

                        if (checkItems[0].state == "complete")
                        {
                            check = Emote.Parse("<a:complete:863861873961992223>");
                        }

                        checklist += $"{check} {checkItems[0].name}\n";

                        //await ReplyAsync($"{checkItems[0].name}\n{checkItems[0].state}");
                    }
                }
                
                builder.AddField("*Checklist:*", checklist, false);
            }

            // TO-DO: get # of members, image manipulation w member icons as thumbnail/image?
            // or custom emoji with members

            var embed = builder.Build();

            var msg = await ReplyAsync(null, false, embed);
            //await ReplyAsync($"https://api.trello.com/1/checklists/{card[0].checklistId}?key={Program.trelloKey}&token={token}", false, null);
        }
    }
}