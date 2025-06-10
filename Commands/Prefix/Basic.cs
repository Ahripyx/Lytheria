using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Lytheria.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lytheria.commands
{
    public class Basic : BaseCommandModule
    {
        [Command("calculate")]
        public async Task CommandParameterExample(CommandContext ctx, int num1, string operation, int num2)
        {
            int result = 0;

            switch (operation)
            {
                case "+":
                    result = num1 + num2;
                    break;
                case "-":
                    result = num1 - num2;
                    break;
                case "*":
                    result = num1 * num2;
                    break;
                case "/":
                    if (num2 == 0)
                    {
                        await ctx.Channel.SendMessageAsync("Cannot divide by zero.");
                        return;
                    }
                    result = num1 / num2;
                    break;
                default:
                    await ctx.Channel.SendMessageAsync("Invalid operation. Use +, -, *, or /.");
                    return;
            }

            await ctx.Channel.SendMessageAsync($"Result: {result}");
        }

        [Command("store")]
        public async Task StoreUser(CommandContext ctx)
        {
            var DBEngine = new DBEngine();

            var userInfo = new Database.DiscordUser
            {
                userName = ctx.User.Username,
                serverName = ctx.Guild.Name,
                serverID = ctx.Guild.Id
            };

            var isStored = await DBEngine.StoreUserAsync(userInfo);

            if (isStored)
            {
                await ctx.Channel.SendMessageAsync("User information stored successfully.");
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Failed to store user information.");
            }
        }

        [Command("profile")]
        public async Task Profile(CommandContext ctx)
        {
            var DBEngine = new DBEngine();

            var userToRetrieve = await DBEngine.GetUserAsync(ctx.User.Username);
            if (userToRetrieve.Item1 == true)
            {
                var profileEmbed = new DiscordEmbedBuilder
                {
                    Title = $"{userToRetrieve.Item2.userName}'s Profile",
                    Description = $"Server: {userToRetrieve.Item2.serverName}\n" +
                                  $"Server ID: {userToRetrieve.Item2.serverID}",
                    Color = DiscordColor.Blue
                };

                await ctx.Channel.SendMessageAsync(embed: profileEmbed);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("User not found.");
            }
        }
    }
}
