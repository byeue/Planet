using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Planet.Services;
using static Infrastructure.PlanetContext;

namespace Planet
{
    class Program
    {
        public static string trelloKey, trelloSecret;
        public static ulong planetGuildId;
        public static SocketGuild planetGuild;

        static async Task Main()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug); // Defines what kind of information should be logged (e.g. Debug, Information, Warning, Critical) adjust this to your liking
                })
                .ConfigureDiscordHost((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose, // Defines what kind of information should be logged from the API (e.g. Verbose, Info, Warning, Critical) adjust this to your liking
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200,
                    };

                    config.Token = context.Configuration["token"];
                    //reach from db after
                    trelloKey = context.Configuration["trellokey"];
                    trelloSecret = context.Configuration["trellosecret"];
                    planetGuildId = Convert.ToUInt64(context.Configuration["planetguildid"]);

                })
                .UseCommandService((context, config) =>
                {
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Verbose;
                })
                .ConfigureServices((context, services) =>
                {
                    services
                    .AddHostedService<CommandHandler>()
                    .AddDbContext<PlanetContext>()
                    .AddSingleton<Servers>()
                    .AddSingleton<Trellos>()
                    .AddSingleton<Utilities.Trello>();
                })
                .UseConsoleLifetime();
            
            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}