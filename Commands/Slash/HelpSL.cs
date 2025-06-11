using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Lytheria.Commands.Slash
{
    
    public class HelpSL : ApplicationCommandModule
    {
        [SlashCommand("help", "Shows a list of commands")]
        public async Task Help(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            var embed = new DiscordEmbedBuilder
            {
                Title = "Command List",
                Description = "Here are the available commands:",
                Color = DiscordColor.Blurple
            };
            embed.AddField("Music Commands", "/play - Play a song\n/stop - Stop the current song\n/pause - Pause the current song\n/resume - Resume the current song\n/skip - Skip the current song\n/queue - Show the current queue (WIP)");
            embed.AddField("Playlist Commands", "/playlist create - Create a new playlist\n/playlist delete - Delete a playlist\n/playlist list - List your playlists (WIP)");
            embed.AddField("User Commands", "/register - Register yourself in the system (WIP, use !store for now)");
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}
