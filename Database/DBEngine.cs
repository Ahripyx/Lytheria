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
