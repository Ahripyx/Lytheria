using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lytheria.Database;

namespace Lytheria.Commands.Slash
{
    [SlashCommandGroup("playlist", "Manage your playlists.")]
    public class PlaylistSL : ApplicationCommandModule
    {
        [SlashCommand("create", "Create a new playlist.")]
        public async Task CreatePlaylist(InteractionContext ctx, [Option("name", "Name of the playlist")] string playlistName)
        {
            await ctx.DeferAsync();

            var DBEngine = new DBEngine();

            var playlistInfo = new Database.Playlist
            {
                Name = playlistName
            };

            var createPlaylist = await DBEngine.CreatePlaylistAsync(ctx.User.Id, playlistName);

            if (createPlaylist)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Playlist Created",
                    Description = $"Playlist **{playlistName}** has been created!",
                    Color = DiscordColor.Green
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "Failed to create playlist. Make sure you are registered by doing !store.",
                    Color = DiscordColor.Red
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
        }
    }
}
