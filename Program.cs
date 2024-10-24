using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using TiaoBot.Configuration;
using TiaoBot.Modules;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(async (context, services) =>
    {
        services.AddSingleton(provider => new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            LogLevel = LogSeverity.Debug
        }));

        services.AddSingleton<InteractionService>(provider =>
        {
            var client = provider.GetRequiredService<DiscordSocketClient>();
            return new InteractionService(client);
        });
        
        services.AddSingleton<BotConfiguration>();
        services.AddSingleton<AdminModule>();
        services.AddSingleton<IPlaywright>(await Playwright.CreateAsync());
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

var botConfiguration = host.Services.GetRequiredService<BotConfiguration>();
await botConfiguration.InitializeBotAsync();
await host.RunAsync();