using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Lytheria.other;
using System;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Lytheria.commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("test")]
        public async Task FirstCommand(CommandContext ctx)
        {
            var interactivity = Program.Client.GetInteractivity();

            var messageToRetrieve = await interactivity.WaitForMessageAsync(message => message.Content == "Hello");

            if (messageToRetrieve.Result.Content == "Hello")
            {
                await ctx.Channel.SendMessageAsync($"{ctx.User.Username} said Hello");
            }
        }

        [Command("reaction")]
        public async Task ReactionCommnad(CommandContext ctx)
        {
            var interactivity = Program.Client.GetInteractivity();

            var messageToReact = await interactivity.WaitForReactionAsync(message => message.Message.Id == 1380791553609699461);

            if (messageToReact.Result.Message.Id == 1380791553609699461)
            {
                await ctx.Channel.SendMessageAsync($"{ctx.User.Username} reacted with {messageToReact.Result.Emoji.Name}");
            }
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

        [Command("poll")]

        public async Task Poll (CommandContext ctx, string opt1, string opt2, string opt3, string opt4, [RemainingText] string pollTitle)
        {
            var interactivity = Program.Client.GetInteractivity();

            var pollTime = TimeSpan.FromSeconds(10);

            DiscordEmoji[] emojiOptions = 
            {
                DiscordEmoji.FromName(ctx.Client, ":one:"),
                DiscordEmoji.FromName(ctx.Client, ":two:"),
                DiscordEmoji.FromName(ctx.Client, ":three:"),
                DiscordEmoji.FromName(ctx.Client, ":four:")
            };

            string optionDescription = $"{emojiOptions[0]} | {opt1}\n" +
                                      $"{emojiOptions[1]} | {opt2}\n" +
                                      $"{emojiOptions[2]} | {opt3}\n" +
                                      $"{emojiOptions[3]} | {opt4}";

            var pollMessage = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Title = pollTitle,
                Description = optionDescription
            };

            var sentPoll = await ctx.Channel.SendMessageAsync(embed: pollMessage);
            
            foreach (var emoji in emojiOptions)
            {
                await sentPoll.CreateReactionAsync(emoji);
            }

            var totalReactions = await interactivity.CollectReactionsAsync(sentPoll, pollTime);

            int count1 = 0;
            int count2 = 0;
            int count3 = 0;
            int count4 = 0;

            foreach (var emoji in totalReactions)
            {
                if (emoji.Emoji == emojiOptions[0])
                {
                    count1++;
                }
                else if (emoji.Emoji == emojiOptions[1])
                {
                    count2++;
                }
                else if (emoji.Emoji == emojiOptions[2])
                {
                    count3++;
                }
                else if (emoji.Emoji == emojiOptions[3])
                {
                    count4++;
                }
            }

            int totalVotes = count1 + count2 + count3 + count4;

            string resultsDescription = $"{emojiOptions[0]}: {count1} votes \n" +
                                        $"{emojiOptions[1]}: {count2} votes \n" +
                                        $"{emojiOptions[2]}: {count3} votes \n" +
                                        $"{emojiOptions[3]}: {count4} votes \n" +
                                        $"Total votes: {totalVotes}";

            var resultsEmbed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "Results of the Poll",
                Description = resultsDescription
            };

            await ctx.Channel.SendMessageAsync(embed: resultsEmbed);
        }
    }
}
