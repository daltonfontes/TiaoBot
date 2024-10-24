using Discord;
using Discord.Interactions;

namespace TiaoBot.Modules
{
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {

        [IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
        [SlashCommand("addkeyword", "Adiciona uma nova palavra-chave")]
        public async Task AddKeyword(string keyword)
        {
            await RespondAsync($"Palavra-chave '{keyword}' adicionada com sucesso!");
        }
    }
}
