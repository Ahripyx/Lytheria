using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus;

namespace Lytheria.Commands.Prefix
{
    internal class DiscordComponentCommands : BaseCommandModule
    {
        [Command("button")]
        public async Task Buttons(CommandContext ctx)
        {
            var button1 = new DiscordButtonComponent(ButtonStyle.Primary, "button1", "Button");

            var button2 = new DiscordButtonComponent(ButtonStyle.Primary, "button2", "Button");

            var message = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Aquamarine)
                    .WithTitle("Test Embed"))
                .AddComponents(button1, button2);

            await ctx.Channel.SendMessageAsync(message);
        }

        [Command("help")]
        public async Task HelpCommand(CommandContext ctx)
        {
            var basicsBtn = new DiscordButtonComponent(ButtonStyle.Primary, "basicsBtn", "Basic Commands");
            var calculatorButton = new DiscordButtonComponent(ButtonStyle.Success, "calculatorBtn", "Calculator Commands");

            var message = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Aquamarine)
                    .WithTitle("Help")
                    .WithDescription("Please press a button to view its commands."))
                .AddComponents(basicsBtn, calculatorButton);

            await ctx.Channel.SendMessageAsync(message);
        }
    }
}
