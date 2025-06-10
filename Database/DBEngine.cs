using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace Lytheria.Database
{
    public class DBEngine
    {
        //Connects to the PostgreSQL database using Npgsql
        private string connectionString = "Server=localhost\\SQLEXPRESS;Database=Lytheria;Trusted_Connection=True;TrustServerCertificate=True;";

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
                    
                    string query = "INSERT INTO userinfo (userno, username, servername, serverid) " +
                                   $"VALUES ('{userNum}', '{user.userName}', '{user.serverName}', '{user.serverID}');";

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

        public async Task<(bool, DiscordUser)> GetUserAsync(string userName)
        {
            try
            {
                DiscordUser result;

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = $"SELECT * FROM userinfo " +
                                   $"WHERE username = '{userName}'";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        var reader = await cmd.ExecuteReaderAsync();
                        reader.ReadAsync().Wait(); // Ensures the reader is ready

                        result = new DiscordUser
                        {
                            userName = reader["username"].ToString(),
                            serverName = reader["servername"].ToString(),
                            serverID = Convert.ToUInt64(reader["serverid"])
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
