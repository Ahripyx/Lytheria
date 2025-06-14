using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using Microsoft.Identity.Client;

namespace Lytheria.Commands
{
    // Handles all music related slash commands
    public class MusicSL : ApplicationCommandModule
    {
        private static readonly ConcurrentDictionary<ulong, Queue<LavalinkTrack>> _queues = new();

        private static readonly ConcurrentDictionary<ulong, CancellationTokenSource> _embedUpdaters = new();

        // Error handling function
        private async Task SendErrorAsync(InteractionContext ctx, string errorMessage)
        {
            var errorEmbed = new DiscordEmbedBuilder
            {
                Title = "Error",
                Description = errorMessage,
                Color = DiscordColor.Red
            };
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(errorEmbed));
        }

        private string GetProgressBar(TimeSpan position, TimeSpan duration, int barLength = 20)
        {
            double progress = duration.TotalSeconds == 0 ? 0 : position.TotalSeconds / duration.TotalSeconds;
            int progressBlocks = (int)(progress * barLength);
            string progressBar = new string('▬', progressBlocks) + "🔘" + new string('▬', barLength - progressBlocks);
            return progressBar;
        }
        private async Task<string> MusicUpdateAsync(InteractionContext ctx, LavalinkTrack track, TimeSpan ? position = null)
        {

            var queue = _queues.GetOrAdd(ctx.Guild.Id, _ => new Queue<LavalinkTrack>());
            var currentPosition = position ?? TimeSpan.Zero;
            var bar = GetProgressBar(currentPosition, track.Length);

            string displayTitle = track.Title.Length > 50 ? track.Title.Substring(0,50) + ". . ." : track.Title;

            string musicDesc = $"**Title:** {displayTitle} \n\n" +
                                      $"**Author:** {track.Author}\n\n" +
                                      $"**Requested by:** {ctx.User.Username}\n\n" +
                                      $"{currentPosition:mm\\:ss} {bar} {track.Length:mm\\:ss}\n\n" +
                                      $"**Queue Length:** {queue.Count} tracks"; 

            return musicDesc;
        }

        private void StartEmbedUpdater(InteractionContext ctx, LavalinkGuildConnection conn, LavalinkTrack track)
        {
            // Cancel any previous updater for this guild
            if (_embedUpdaters.TryRemove(ctx.Guild.Id, out var oldCts))
                oldCts.Cancel();

            var cts = new CancellationTokenSource();
            _embedUpdaters[ctx.Guild.Id] = cts;

            _ = Task.Run(async () =>
            {
                try
                {
                    while (!cts.Token.IsCancellationRequested && conn.CurrentState.CurrentTrack != null)
                    {
                        var position = conn.CurrentState.PlaybackPosition;
                        var desc = await MusicUpdateAsync(ctx, track, position);
                        var embed = new DiscordEmbedBuilder
                        {
                            Title = "Now Playing:",
                            Description = desc,
                            Color = DiscordColor.Blue
                        };
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
                    }
                }
                catch (TaskCanceledException) { }
            }, cts.Token);
        }

        // Play song command
        [SlashCommand("play", "Play a song in your voice channel")]
        public async Task PlayMusic(InteractionContext ctx, [Option("song", "Song")] string song)
        {
            try
            {
                await ctx.DeferAsync();

                var lavalinkInstance = ctx.Client.GetLavalink();

                // PRE-Executions Checks
                if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
                {
                    await SendErrorAsync(ctx, "Must be in a voice channel to connect.");
                    return;
                }

                var userVC = ctx.Member.VoiceState.Channel;

                if (!lavalinkInstance.ConnectedNodes.Any())
                {
                    await SendErrorAsync(ctx, "Connection is not established.");
                    return;
                }

                if (userVC.Type != ChannelType.Voice)
                {
                    await SendErrorAsync(ctx, "Please enter a valid voice channel");
                    return;
                }

                var node = lavalinkInstance.ConnectedNodes.Values.FirstOrDefault();
                await node.ConnectAsync(userVC);

                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn == null)
                {
                    await SendErrorAsync(ctx, "Lavalink failed to connect.");
                    return;
                }

                var searchQuery = await node.Rest.GetTracksAsync(song);

                if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed)
                {
                    await SendErrorAsync(ctx, $"Failed to find music with query: {song}");
                    return;
                }

                var musicTrack = searchQuery.Tracks.First();

                var queue = _queues.GetOrAdd(ctx.Guild.Id, _ => new Queue<LavalinkTrack>());

                if (conn.CurrentState.CurrentTrack != null)
                {
                    queue.Enqueue(musicTrack);
                    var queueEmbed = new DiscordEmbedBuilder()
                    {
                        Title = "Song added to queue",
                        Description = $"**{musicTrack.Title}** has been added to the queue.",
                        Color = DiscordColor.Orange
                    };

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(queueEmbed));
                }
                else
                {
                    await conn.PlayAsync(musicTrack);
                    
                    var nowPlayingDescription = await MusicUpdateAsync(ctx, musicTrack, TimeSpan.Zero);

                    var nowPlayingEmbed = new DiscordEmbedBuilder()
                    {
                        Title = $"Now Playing:",
                        Description = nowPlayingDescription,
                        Color = DiscordColor.Blue
                    };

                    StartEmbedUpdater(ctx, conn, musicTrack);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(nowPlayingEmbed));

                    conn.PlaybackFinished += async (s, e) =>
                    {
                        if (queue.TryDequeue(out var nextTrack))
                        {
                            await conn.PlayAsync(nextTrack);
                            StartEmbedUpdater(ctx, conn, nextTrack);
                        }
                        else
                        {
                            // Cancel updater when queue is empty
                            if (_embedUpdaters.TryRemove(ctx.Guild.Id, out var cts))
                                cts.Cancel();
                        }
                    };
                }
                  
            }
            catch (Exception ex)
            {
                await SendErrorAsync(ctx, "An unexpected error has occured.");
                Console.WriteLine(ex);
            }

        }

        [SlashCommand("skip", "Skip the current song that is playing")]
        public async Task SkipMusic(InteractionContext ctx)
        {
            try
            {
                await ctx.DeferAsync();

                var lavalinkInstance = ctx.Client.GetLavalink();

                // PRE-Executions Checks
                if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
                {
                    await SendErrorAsync(ctx, "Must be in a voice channel to connect.");
                    return;
                }

                var userVC = ctx.Member.VoiceState.Channel;

                if (!lavalinkInstance.ConnectedNodes.Any())
                {
                    await SendErrorAsync(ctx, "Connection is not established.");
                    return;
                }
                if (userVC.Type != ChannelType.Voice)
                {
                    await SendErrorAsync(ctx, "Please enter a valid voice channel");
                    return;
                }
                var node = lavalinkInstance.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
                if (conn == null)
                {
                    await SendErrorAsync(ctx, "Lavalink failed to connect.");
                    return;
                }
                if (conn.CurrentState.CurrentTrack == null)
                {
                    await SendErrorAsync(ctx, "No tracks are playing at the moment.");
                    return;
                }

                var queue = _queues.GetOrAdd(ctx.Guild.Id, _ => new Queue<LavalinkTrack>());

                if (queue.TryDequeue(out var nextTrack))
                {
                    await conn.PlayAsync(nextTrack);

                    var nowPlayingDescription = await MusicUpdateAsync(ctx, nextTrack, TimeSpan.Zero);

                    var skipEmbed = new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Green,
                        Title = "Track Skipped",
                        Description = nowPlayingDescription
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(skipEmbed));
                }
                else
                {
                    await conn.StopAsync();

                    var stopEmbed = new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Orange,
                        Title = "Track Skipped",
                        Description = "No more songs in the queue. Playback stopped."
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(stopEmbed));
                }
            }
            catch(Exception ex)
            {
                await SendErrorAsync(ctx, "An unexpected error has occured.");
                Console.WriteLine(ex);
            }

        }

        // Pause song command
        [SlashCommand("pause", "Pause the song currently playing")]
        public async Task PauseMusic(InteractionContext ctx)
        {
            try
            {
                await ctx.DeferAsync();

                var lavalinkInstance = ctx.Client.GetLavalink();

                // PRE-Executions Checks
                if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
                {
                    await SendErrorAsync(ctx, "Must be in a voice channel to connect.");
                    return;
                }

                var userVC = ctx.Member.VoiceState.Channel;

                if (!lavalinkInstance.ConnectedNodes.Any())
                {
                    await SendErrorAsync(ctx, "Connection is not established.");
                    return;
                }

                if (userVC.Type != ChannelType.Voice)
                {
                    await SendErrorAsync(ctx, "Please enter a valid voice channel");
                    return;
                }

                var node = lavalinkInstance.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn == null)
                {
                    await SendErrorAsync(ctx, "Lavalink failed to connect.");
                    return;
                }

                if (conn.CurrentState.CurrentTrack == null)
                {
                    await SendErrorAsync(ctx, "No tracks are playing at the moment.");
                    return;
                }

                await conn.PauseAsync();

                var pausedEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Track Paused"
                };

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(pausedEmbed));
            }
            catch(Exception ex)
            {
                await SendErrorAsync(ctx, "An unexpected error has occured.");
                Console.WriteLine(ex);
            }
            
        }

        // Resume song command
        [SlashCommand("resume", "Resume the paused song")]
        public async Task ResumeMusic(InteractionContext ctx)
        {
            try
            {
                await ctx.DeferAsync();

                var lavalinkInstance = ctx.Client.GetLavalink();

                // PRE-Executions Checks
                if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
                {
                    await SendErrorAsync(ctx, "Must be in a voice channel to connect.");
                    return;
                }
                var userVC = ctx.Member.VoiceState.Channel;

                if (!lavalinkInstance.ConnectedNodes.Any())
                {
                    await SendErrorAsync(ctx, "Connection is not established.");
                    return;
                }

                if (userVC.Type != ChannelType.Voice)
                {
                    await SendErrorAsync(ctx, "Please enter a valid voice channel.");
                    return;
                }

                var node = lavalinkInstance.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn == null)
                {
                    await SendErrorAsync(ctx, "Lavalink failed to connect.");
                    return;
                }

                if (conn.CurrentState.CurrentTrack == null)
                {
                    await SendErrorAsync(ctx, "No tracks are playing at the moment.");
                    return;
                }

                await conn.ResumeAsync();

                var resumedEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Green,
                    Title = "Track Resumed"
                };

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(resumedEmbed));
            }
            catch(Exception ex)
            {
                await SendErrorAsync(ctx, "An unexpected error has occured.");
                Console.WriteLine(ex);
            }
            
        }

        // Stop song command
        [SlashCommand("stop", "Stop track from playing")]
        public async Task StopMusic(InteractionContext ctx)
        {
            try
            {
                await ctx.DeferAsync();

                var lavalinkInstance = ctx.Client.GetLavalink();

                // PRE-Executions Checks
                if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
                {
                    await SendErrorAsync(ctx, "Must be in a voice channel to connect.");
                    return;
                }

                var userVC = ctx.Member.VoiceState.Channel;

                if (!lavalinkInstance.ConnectedNodes.Any())
                {
                    await SendErrorAsync(ctx, "Connection is not established.");
                    return;
                }

                if (userVC.Type != ChannelType.Voice)
                {
                    await SendErrorAsync(ctx, "Please enter a valid voice channel.");
                    return;
                }

                var node = lavalinkInstance.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn == null)
                {
                    await SendErrorAsync(ctx, "Lavalink failed to connect.");
                    return;
                }

                if (conn.CurrentState.CurrentTrack != null)
                {
                    await conn.StopAsync();
                }

                await conn.DisconnectAsync();

                var stopEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Green,
                    Title = "Track stopped",
                    Description = "Successfully disconnected from the voice channel"
                };

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(stopEmbed));
            }
            catch(Exception ex)
            {
                await SendErrorAsync(ctx, "An unexpected error has occured.");
                Console.WriteLine(ex);
            }
            
        }
    }
}
