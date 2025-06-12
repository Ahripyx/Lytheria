using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Npgsql;

namespace Lytheria.Database
{
    public class DBEngine
    {
        // Connects to the PostgreSQL database using Npgsql
        private string connectionString = "Server=localhost\\SQLEXPRESS;Database=Lytheria;Trusted_Connection=True;TrustServerCertificate=True;";

        // Checks for playlist count for user
        public async Task<int> PlaylistCountAsync(ulong profileId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    
                    string getUserQuery = $"SELECT userId FROM userinfo WHERE profileId = '{profileId}'";
                    long userId;
                    using (var getUserCmd = new SqlCommand(getUserQuery, conn))
                    {
                        var userIdResult = await getUserCmd.ExecuteScalarAsync();
                        if (userIdResult == null)
                        {
                            throw new Exception("User not found.");
                        }
                        userId = Convert.ToInt64(userIdResult);
                    }
                    string countQuery = $"SELECT COUNT(*) FROM playlist WHERE userId = '{userId}'";
                    using (var countPlaylistCmd = new SqlCommand(countQuery, conn))
                    {
                        var countResult = await countPlaylistCmd.ExecuteScalarAsync();
                        return Convert.ToInt32(countResult);
                    }
                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting playlists: {ex.Message}");
                return -1;
            }
        }

        // SQL Command for creating a playlist
        public async Task<bool> CreatePlaylistAsync(ulong profileId, string playlistName)
        {
            try
            {
                long userId;
                using (var conn = new SqlConnection(connectionString))
                {

                    await conn.OpenAsync();

                    string getUserQuery = $"SELECT userId FROM userinfo WHERE profileId = '{profileId}'";
                    
                    using (var getUserCmd = new SqlCommand(getUserQuery, conn))
                    {
                        var userIdResult = await getUserCmd.ExecuteScalarAsync();
                        
                        if (userIdResult == null)
                        {
                            throw new Exception("User not found.");
                        }
                        userId = Convert.ToInt64(userIdResult);
                    }

                    string insertQuery = "INSERT INTO playlist (playlistName, userId) " +
                                   $"VALUES ('{playlistName}', '{userId}');";

                    using (var cmd = new SqlCommand(insertQuery, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating playlist: {ex.Message}");
                return false;
            }
        }

        // SQL Command for removing a playlist
        public async Task<bool> RemovePlaylistAsync(ulong profileId, string playlistName)
        {
            try
            {
                long userId;
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string getUserQuery = $"SELECT userId FROM userinfo WHERE profileId = '{profileId}'";
                    
                    using (var getUserCmd = new SqlCommand(getUserQuery, conn))
                    {
                        var userIdResult = await getUserCmd.ExecuteScalarAsync();
                        
                        if (userIdResult == null)
                        {
                            throw new Exception("User not found.");
                        }
                        userId = Convert.ToInt64(userIdResult);
                    }
                    string deleteQuery = "DELETE FROM playlist " +
                                         $"WHERE playlistName = '{playlistName}' AND userId = '{userId}';";
                    using (var cmd = new SqlCommand(deleteQuery, conn))
                    {
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing playlist: {ex.Message}");
                return false;
            }
        }

        // Retrieves the playlist ID for a given profile ID and playlist name
        public async Task<long?> GetPlaylistIdAsync(ulong profileId, string playlistName)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    
                    string getUserQuery = $"SELECT userId FROM userinfo WHERE profileId = '{profileId}'";
                    long userId;
                    
                    using (var getUserCmd = new SqlCommand(getUserQuery, conn))
                    {
                        var userIdResult = await getUserCmd.ExecuteScalarAsync();
                        
                        if (userIdResult == null)
                        {
                            throw new Exception("User not found.");
                        }
                        userId = Convert.ToInt64(userIdResult);
                    }
                    string query = $"SELECT playlistId FROM playlist WHERE playlistName = '{playlistName}' AND userId = '{userId}';";
                    
                    using (var getPlaylistCmd = new SqlCommand(query, conn))
                    {
                        var playlistIdresult = await getPlaylistCmd.ExecuteScalarAsync();
                        
                        if (playlistIdresult == null)
                        {
                            return null; // Playlist not found
                        }
                        return Convert.ToInt64(playlistIdresult);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching playlist ID: {ex.Message}");
                return null;
            }
        }

        // Adds a song to the database or retrieves its ID if it already exists
        public async Task<long> AddOrGetSongAsync(string title, string artist, string duration,  ulong profileId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Checking to see if song exists already
                    string checkSongQuery = $"SELECT songId FROM song WHERE title = '{title}' AND artist = '{artist}' AND duration = '{duration}'"; 
                    using (var checkCmd = new SqlCommand(checkSongQuery, conn))
                    {
                        var songIdResult = await checkCmd.ExecuteScalarAsync();
                        if (songIdResult != null)
                        {
                            return Convert.ToInt64(songIdResult);
                        }
                    }

                    // Inserting song if not in database
                    string insertSongQuery = "INSERT INTO song (title, artist, duration) " +
                                             $"VALUES ('{title}', '{artist}', '{duration}');" +
                                             "SELECT SCOPE_IDENTITY();";
                    using (var insertCmd = new SqlCommand(insertSongQuery, conn))
                    {
                        var insertId = await insertCmd.ExecuteScalarAsync();
                        return Convert.ToInt64(insertId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding or getting song: {ex.Message}");
                return -1;
            }
        }

        // Retrieves list of users playlists
        public async Task<List<string>> GetUserPlaylistsAsync(ulong profileId)
        {
            try
            {
                List<string> playlists = new List<string>();
                
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    
                    string getUserQuery = $"SELECT userId FROM userinfo WHERE profileId = '{profileId}'";
                    long userId;
                    
                    using (var getUserCmd = new SqlCommand(getUserQuery, conn))
                    {
                        var userIdResult = await getUserCmd.ExecuteScalarAsync();
                        
                        if (userIdResult == null)
                        {
                            throw new Exception("User not found.");
                        }
                        userId = Convert.ToInt64(userIdResult);
                    }
                    
                    string query = $"SELECT playlistName FROM playlist WHERE userId = '{userId}';";
                    
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        var reader = await cmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            playlists.Add(reader["playlistName"].ToString());
                        }
                    }
                }
                return playlists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user playlists: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AddSongToPlaylistAsync(long playlistId, long songId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string insertQuery = "INSERT INTO playlist_songs (playlistId, songId) " +
                                   $"VALUES ('{playlistId}', '{songId}');";

                    using (var cmd = new SqlCommand(insertQuery, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                        return true; 
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error adding song to playlist.");
                return false;
            }
        }

        public async Task<List<string>> GetPlaylistSongsAsync(long playlistId)
        {
            try
            {
                List<string> songs = new List<string>();
                
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    
                    string query = $"SELECT song.title, song.artist FROM playlist_songs " +
                                   $"JOIN song ON playlist_songs.songId = song.songId " +
                                   $"WHERE playlist_songs.playlistId = '{playlistId}';";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        var reader = await cmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            songs.Add(reader["title"].ToString());
                            songs.Add(" by ");
                            songs.Add(reader["artist"].ToString());
                        }
                    }
                }
                return songs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching playlist songs: {ex.Message}");
                return null;
            }
        }

        // Stores user infomration
        public async Task<bool> StoreUserAsync(DiscordUser user)
        {
            try
            {
                var userNum = await GetTotalUsersAsync() + 1;

                if (userNum == -1)
                {
                    throw new Exception();
                }

                using (var conn= new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    
                    string query = "INSERT INTO userinfo (userId, username, profileId ) " +
                                   $"VALUES ('{userNum}', '{user.userName}', '{user.profileId}');";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured: {ex.Message}");
                return false;
            }
            
        }

        // Retrieves user information
        public async Task<(bool, DiscordUser)> GetUserAsync(ulong profileId)
        {
            try
            {
                DiscordUser result;

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = $"SELECT * FROM userinfo " +
                                   $"WHERE profileId = '{profileId}'";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        var reader = await cmd.ExecuteReaderAsync();
                        reader.ReadAsync().Wait(); // Ensures the reader is ready

                        result = new DiscordUser
                        {
                            userName = reader["username"].ToString(),
                            profileId = Convert.ToUInt64(reader["profileId"])
                        };
                    }
                }
                return (true, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user: {ex.Message}");
                return (false, null);
            }
        }

        // Retrieves the total number of users
        private async Task<long> GetTotalUsersAsync()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string query = "SELECT COUNT(*) FROM userinfo";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        var userCount = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt64(userCount);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching total users: {ex.Message}");
                return -1;
            }
        } 
    }
}
