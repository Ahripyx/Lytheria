using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Lytheria.config;

namespace Lytheria
{
    internal class Program
    {
        // Discord client instance to be used throughout the application.
        private static DiscordClient client { get; set; }
        // CommandsNextExtension instance to handle commands.
        private static CommandsNextExtension commands { get; set; }

        static async Task Main(string[] args)
        {
            var jsonReader = new JSONReader();
            await jsonReader.ReadJSON();

            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };

            client = new DiscordClient(discordConfig);

            client.Ready += Client_Ready;

            await client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
