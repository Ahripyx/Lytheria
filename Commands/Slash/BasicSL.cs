using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace Lytheria.Commands.Slash
{
    public class BasicSL : ApplicationCommandModule
    {
        [SlashCommand("test", "First slash command test")]
        public async Task FirstSlashCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            var embedMessage = new DiscordEmbedBuilder
            {
                Title = "Test Embed",
                Description = "This is a test embed message.",
                Color = DiscordColor.Blue
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedMessage));
        }

        [SlashCommand("parameters", "This slash command allows parameters")]
        public async Task ParamSlashCommand(InteractionContext ctx, [Option("testoption", "type in anything")] string testParameter, [Option("numOption", "type in a number")] long number)
        {
            await ctx.DeferAsync();

            var embedMessage = new DiscordEmbedBuilder
            {
                Title = "Parameter Test",
                Description = $"You entered: {testParameter}\n" +
                $"              The year is: {number}",
                Color = DiscordColor.Green
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedMessage));
        }
    }
}
