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

        [SlashCommand("discordParameters", "This slash command allows passing of DiscordParameters")]
        public async Task DiscordParameters(InteractionContext ctx, [Option("user", "Pass in a discord user")] DiscordUser user, [Option("attachment", "Upload a file here.")] DiscordAttachment file)
        {
            await ctx.DeferAsync();

            var member = (DiscordMember)user;

            var embedMessage = new DiscordEmbedBuilder
            {
                Title = "Discord Parameters Test",
                Description = $"You selected user: {member.Nickname}\n" +
                              $"You uploaded a file: {file.FileName} {file.FileSize}",
                Color = DiscordColor.Blue
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedMessage));
        }
    }
}
