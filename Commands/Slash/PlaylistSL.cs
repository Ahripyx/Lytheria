using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
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

        [SlashCommand("add", "Add a song to a playlist")]
        public async Task AddSongToPlaylist(InteractionContext ctx, [Option("playlist", "Playlist name")] string playlistName, [Option("song", "Song name")] string song)
        {
            await ctx.DeferAsync();

            var DBEngine = new DBEngine();

            // Get playlist ID
            var playlistId = await DBEngine.GetPlaylistIdAsync(ctx.User.Id, playlistName);
            if (playlistId == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Playlist not found"));
                return;
            }

            // Searching song via lavalink
            var lavalink = ctx.Client.GetLavalink();
            var node = lavalink.ConnectedNodes.Values.FirstOrDefault();
            if (node == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No Lavalink node connected."));
                return;
            }

            var searchResult = await node.Rest.GetTracksAsync(song);
            if (searchResult.LoadResultType == LavalinkLoadResultType.NoMatches || !searchResult.Tracks.Any())
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No matches found for the song."));
                return;
            }

            var track = searchResult.Tracks.FirstOrDefault();

            var songInfo = new Database.Song
            {
                title = track.Title,
                artist = track.Author,
                duration = track.Length.ToString()
            };

            var songId = await DBEngine.AddOrGetSongAsync(
                songInfo.title,
                songInfo.artist,
                songInfo.duration,
                ctx.User.Id
                );

            if (songId == -1)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Failed to add song to the database."));
                return;
            }

            var addedSong = await DBEngine.AddSongToPlaylistAsync(playlistId.Value, songId);
            if (addedSong)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Song Added",
                    Description = $"Song **{songInfo.title}** by **{songInfo.artist}** has been added to playlist **{playlistName}**!",
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
                    Description = "Failed to add song to playlist. Make sure the playlist exists.",
                    Color = DiscordColor.Red
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                return;
            }

        }
    }
}
