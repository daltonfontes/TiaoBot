using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TiaoBot.Modules;

namespace TiaoBot.Configuration
{
    public class BotConfiguration : IDisposable
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly InteractionService _interactionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BotConfiguration> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BotConfiguration(
            IConfiguration configuration,
            ILogger<BotConfiguration> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = ConfigureUserSecrets(configuration);

            _discordSocketClient = new DiscordSocketClient(CreateBotConfig());
            _interactionService = new InteractionService(_discordSocketClient);

            RegisterModules();
        }

        private void RegisterModules()
        {
            _interactionService.AddModuleAsync<AdminModule>(_serviceProvider);
        }

        private static IConfiguration ConfigureUserSecrets(IConfiguration configuration)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddUserSecrets<BotConfiguration>();

            return builder.Build();
        }

        public async Task InitializeBotAsync()
        {
            string token = GetBotToken();
            if (!IsTokenValid(token))
            {
                return;
            }

            await ConnectBotAsync(token);
            await Task.Delay(-1);
        }

        public void Dispose() => _discordSocketClient?.Dispose();

        private async Task ConnectBotAsync(string token)
        {
            await LoginBotAsync(token);
            await StartBotAsync();
        }

        private async Task LoginBotAsync(string token)
        {
            try
            {
                await _discordSocketClient.LoginAsync(TokenType.Bot, token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao fazer login: {ex}");
                throw;
            }
        }

        private async Task StartBotAsync()
        {
            await _discordSocketClient.StartAsync();
            _discordSocketClient.Ready += OnBotReady;
            _discordSocketClient.InteractionCreated += HandleInteractionAsync;
        }

        private Task OnBotReady()
        {
            _logger.LogInformation($"Bot {_discordSocketClient.CurrentUser.Username} está online!");
            Task.Run(async () => await _interactionService.RegisterCommandsGloballyAsync());
            return Task.CompletedTask;
        }

        private string GetBotToken()
        {
            string? token = _configuration["Discord:ServiceApiKey"];
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Token indefinido. Verifique o user secrets.");
                throw new InvalidOperationException("Token indefinido.");
            }
            return token;
        }

        private static bool IsTokenValid(string token) => !string.IsNullOrEmpty(token);

        private static DiscordSocketConfig CreateBotConfig()
        {
            return new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
                LogLevel = LogSeverity.Debug
            };
        }

        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_discordSocketClient, interaction);
                await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar interação: {ex}");
            }
        }
    }
}