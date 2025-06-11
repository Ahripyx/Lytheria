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
        // Create playlist command
        [SlashCommand("create", "Create a new playlist.")]
        public async Task CreatePlaylist(InteractionContext ctx, [Option("name", "Name of the playlist")] string playlistName)
        {
            await ctx.DeferAsync();

            var DBEngine = new DBEngine();

            // Checking to see if user is reigstered
            var (userExists, _) = await DBEngine.GetUserAsync(ctx.User.Id);

            if (!userExists)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "Failed to create playlist. Make sure you are registered by doing !store.",
                    Color = DiscordColor.Red
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                return;
            }

            // Checking already owned playlist count
            var playlistCount = await DBEngine.PlaylistCountAsync(ctx.User.Id);
            if (playlistCount >= 10)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "The max number amount of playlist is 10, please use /playlist remove to make space.",
                    Color = DiscordColor.Red
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                return;
            }

            // Attempting to create the playlist
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
                return;
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "Failed to create playlist.",
                    Color = DiscordColor.Red
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }

            var playlistInfo = new Database.Playlist
            {
                Name = playlistName
            };
        }

        // Remove playlist command
        [SlashCommand("delete", "Remove a playlist")]
        public async Task RemovePlaylist(InteractionContext ctx, [Option("name", "Name of the playlist")] string playlistName)
        {
            await ctx.DeferAsync();

            var DBEngine = new DBEngine();

            // Checking to see if user is reigstered
            var (userExists, _) = await DBEngine.GetUserAsync(ctx.User.Id);

            if (!userExists)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "Failed to create playlist. Make sure you are registered by doing !store.",
                    Color = DiscordColor.Red
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                return;
            }

            var playlistCount = await DBEngine.PlaylistCountAsync(ctx.User.Id);
            if (playlistCount <= 0)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "You have no playlists to remove.",
                    Color = DiscordColor.Red
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                return;
            }

            var removePlaylist = await DBEngine.RemovePlaylistAsync(ctx.User.Id, playlistName);

            if (removePlaylist)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Playlist Removed",
                    Description = $"Playlist **{playlistName}** has been removed!",
                    Color = DiscordColor.Green
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                return;
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "Failed to remove playlist. Make sure the playlist exists.",
                    Color = DiscordColor.Red
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
        }
    }
}
