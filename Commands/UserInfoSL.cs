using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Lytheria.Commands
{
    public class UserInfoSL : ApplicationCommandModule
    {
        // Command for registering user into database
        [SlashCommand("register", "Register yourself into the system")]
        public async Task RegisterUser(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            var DBEngine = new Database.DBEngine();

            // Checking to see if user is reigstered
            var (userExists, _) = await DBEngine.GetUserAsync(ctx.User.Id);

            if (userExists)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are already registered."));
                return;
            }

            var userInfo = new Database.DiscordUser
            {
                userName = ctx.User.Username,
                profileId = ctx.User.Id
            };

            var isStored = await DBEngine.StoreUserAsync(userInfo);

            if (isStored)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("User information registered successfully"));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Failed to register user information. Please try again later."));
            }
        }

        [SlashCommand("profile", "View your profile information")]
        public async Task ViewProfile(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            var DBEngine = new Database.DBEngine();

            // Checking to see if user is registered
            var (userExists, user) = await DBEngine.GetUserAsync(ctx.User.Id);
            if (!userExists)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not registered. Please use /register to register."));
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{ctx.User.Username}'s Profile",
                Description = $"**Username:** {user.userName}\n**Profile ID:** {user.profileId}",
                Color = DiscordColor.Blurple
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}
