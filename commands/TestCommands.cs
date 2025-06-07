using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Lytheria.other;

namespace Lytheria.commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("test")]
        public async Task FirstCommand(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"Hello {ctx.User.Username}!").ConfigureAwait(false);
        }

        [Command("add")]
        public async Task Add(CommandContext ctx, int number1, int number2)
        {
            int result = number1 + number2;
            await ctx.Channel.SendMessageAsync($"The result of {number1} + {number2} is {result}.").ConfigureAwait(false);
        }

        [Command("subtract")]
        public async Task Subtract(CommandContext ctx, int number1, int number2)
        {
            int result = number1 - number2;
            await ctx.Channel.SendMessageAsync($"The result of {number1} - {number2} is {result}").ConfigureAwait(false);
        }

        [Command("embed")]
        public async Task EmbedMessage(CommandContext ctx)
        {
            var message = new DiscordEmbedBuilder
            {
                Title = "First Discord Embed",
                Description = $"This command was executed by {ctx.User.Username}",
                Color = DiscordColor.HotPink
            };

            await ctx.Channel.SendMessageAsync(embed: message).ConfigureAwait(false);
        }

        [Command("cardgame")]
        public async Task CardGame(CommandContext ctx)
        {
            var userCard = new CardSystem();

            var userCardEmbed = new DiscordEmbedBuilder
            {
                Title = $"Your card is {userCard.SelectedCard}",
                Color = DiscordColor.Lilac
            };

            await ctx.Channel.SendMessageAsync(embed: userCardEmbed).ConfigureAwait(false);

            var botCard = new CardSystem();

            var botCardEmbed = new DiscordEmbedBuilder
            {
                Title = $"The bot card is {botCard.SelectedCard}",
                Color = DiscordColor.Purple
            };
            
            await ctx.Channel.SendMessageAsync(embed: botCardEmbed).ConfigureAwait(false);

            if (userCard.SelectedNumber > botCard.SelectedNumber)
            {
                //User wins
                var winMessage = new DiscordEmbedBuilder
                {
                    Title = "You win!",
                    Description = $"Your card ({userCard.SelectedCard}) is higher than the bot's card ({botCard.SelectedCard}).",
                    Color = DiscordColor.Green
                };

                await ctx.Channel.SendMessageAsync(embed: winMessage).ConfigureAwait(false);
            }
            else if (userCard.SelectedNumber < botCard.SelectedNumber)
            {

            }
            else
            {
                //Bot wins
                var loseMessage = new DiscordEmbedBuilder
                {
                    Title = "You lose!",
                    Description = $"Your card ({userCard.SelectedCard}) is lower than the bot's card ({botCard.SelectedCard}).",
                    Color = DiscordColor.Red
                };

                await ctx.Channel.SendMessageAsync(embed: loseMessage).ConfigureAwait(false);
            }
        }
    }
}
