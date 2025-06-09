using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lytheria.Commands.Slash
{
    [SlashCommandGroup("calculator", "Peform calculator operations.")]
    public class CalculatorSL : ApplicationCommandModule
    {
        [SlashCommand("add", "Adds two numbers together.")]
        public async Task Add(InteractionContext ctx, [Option("number1", "Number 1")]double number1, [Option("number2", "Number 1")] double number2)
        {
            await ctx.DeferAsync();

            double result = number1 + number2;

            var embedMessage = new DiscordEmbedBuilder
            {
                Title = "Addition Result",
                Description = $"{number1} + {number2} = {result}",
                Color = DiscordColor.Green
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedMessage));
        }

        [SlashCommand("subtract", "Subtracts two numbers.")]
        public async Task Subtract(InteractionContext ctx, [Option("number1", "Number 1")] double number1, [Option("number2", "Number 2")] double number2)
        {
            await ctx.DeferAsync();
            double result = number1 - number2;
            var embedMessage = new DiscordEmbedBuilder
            {
                Title = "Subtraction Result",
                Description = $"{number1} - {number2} = {result}",
                Color = DiscordColor.Green
            };
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedMessage));
        }

        [SlashCommand("multiply", "Multiplies two numbers.")]
        public async Task Multiply(InteractionContext ctx, [Option("number1", "Number 1")] double number1, [Option("number2", "Number 2")] double number2)
        {
            await ctx.DeferAsync();
            double result = number1 * number2;
            var embedMessage = new DiscordEmbedBuilder
            {
                Title = "Multiplication Result",
                Description = $"{number1} * {number2} = {result}",
                Color = DiscordColor.Green
            };
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedMessage));
        }

        [SlashCommand("divide", "Divides two numbers.")]
        public async Task Divide(InteractionContext ctx, [Option("number1", "Number 1")] double number1, [Option("number2", "Number 2")] double number2)
        {
            await ctx.DeferAsync();
            if (number2 == 0)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "Cannot divide by zero.",
                    Color = DiscordColor.Red
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(errorEmbed));
                return;
            }
            double result = number1 / number2;
            var embedMessage = new DiscordEmbedBuilder
            {
                Title = "Division Result",
                Description = $"{number1} / {number2} = {result}",
                Color = DiscordColor.Green
            };
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedMessage));
        }

        [SlashCommand("modulus", "Calculates the modulus of two numbers.")]
        public async Task Modulus(InteractionContext ctx, [Option("number1", "Number 1")] double number1, [Option("number2", "Number 2")] double number2)
        {
            await ctx.DeferAsync();
            if (number2 == 0)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "Cannot divide by zero.",
                    Color = DiscordColor.Red
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(errorEmbed));
                return;
            }
            double result = number1 % number2;
            var embedMessage = new DiscordEmbedBuilder
            {
                Title = "Modulus Result",
                Description = $"{number1} % {number2} = {result}",
                Color = DiscordColor.Green
            };
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedMessage));
        }

    }
}
