using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using Lytheria.commands;
using Lytheria.Commands.Prefix;
using Lytheria.Commands.Slash;
using Lytheria.config;

// Code by SamJesus8
// Edited by: Ahripyx

namespace Lytheria
{
    public sealed class Program
    {
        // Discord client instance to be used throughout the application.
        public static DiscordClient Client { get; set; }
        // CommandsNextExtension instance to handle commands.
        public static CommandsNextExtension Commands { get; set; }

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

            Client = new DiscordClient(discordConfig);

            Client.UseInteractivity(new InteractivityConfiguration() 
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            Client.Ready += Client_Ready;
            Client.MessageCreated += MessageCreatedHandler;
            Client.VoiceStateUpdated += VoiceChannelHandler;
            Client.ComponentInteractionCreated += Client_ComponentInteractionCreated;
            Client.ModalSubmitted += ModalSubmittedHandler;

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { jsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false
            };

            var slashCommandsConfiguration = Client.UseSlashCommands();

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.CommandErrored += CommandEventHandler;

            // Registering prefix commands
            Commands.RegisterCommands<TestCommands>();
            Commands.RegisterCommands<Basic>();
            Commands.RegisterCommands<DiscordComponentCommands>();

            // Registering slash commands
            slashCommandsConfiguration.RegisterCommands<BasicSL>();
            slashCommandsConfiguration.RegisterCommands<CalculatorSL>();
            slashCommandsConfiguration.RegisterCommands<HelpSL>();
            slashCommandsConfiguration.RegisterCommands<MusicSL>();
            slashCommandsConfiguration.RegisterCommands<PlaylistSL>();

            // Lavalink Configuration (change configs as updates release)
            var endpoint = new ConnectionEndpoint
            {
                Hostname = "lava-all.ajieblogs.eu.org",
                Port = 443,
                Secured = true
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "https://dsc.gg/ajidevserver",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            var lavalink = Client.UseLavalink();

            //Automatic reonnection if lavalink goes down
            lavalink.NodeDisconnected += async (s, e) =>
            {
                Console.WriteLine("Lavalink node disconnected. Attempting to reconnect...");
                int retries = 0;
                bool connected = false;
                while (!connected && retries < 5)
                {
                    try
                    {
                        await Task.Delay(5000); // Wait 5 seconds before retry
                        await lavalink.ConnectAsync(lavalinkConfig);
                        connected = true;
                        Console.WriteLine("Lavalink reconnected successfully.");
                    }
                    catch (Exception ex)
                    {
                        retries++;
                        Console.WriteLine($"Lavalink reconnect attempt {retries} failed: {ex.Message}");
                    }
                }
                if (!connected)
                {
                    Console.WriteLine("Failed to reconnect to Lavalink after multiple attempts.");
                }
            };

            // Connects to get the bot online
            await Client.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);
            await Task.Delay(-1);
        }

        private static async Task ModalSubmittedHandler(DiscordClient sender, ModalSubmitEventArgs e)
        {
            if (e.Interaction.Type == InteractionType.ModalSubmit)
            {
                var values = e.Values;
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"{e.Interaction.User.Username} submitted a modal with the input {values.Values.First()}"));
            }
        }

        private static async Task Client_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            //Drop-Down events
            if (args.Id == "dropDownList" && args.Interaction.Data.ComponentType == ComponentType.StringSelect)
            {
                var options = args.Values;

                foreach (var option in options)
                {
                    switch (option)
                    {
                        case "option1":
                            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                .WithContent("You chose option 1!"));
                            break;
                        case "option2":
                            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                .WithContent("You chose option 2!"));
                            break;
                        case "option3":
                            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                .WithContent("You chose option 3!"));
                            break;
                    }
                }
            }
            else if (args.Id == "channelDropDownList")
            {
                var options = args.Values;

                foreach (var channel in options)
                {
                    var selectedChannel = await Client.GetChannelAsync(ulong.Parse(channel));

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                        .WithContent($"{args.User.Username} selected the channl with name {selectedChannel.Name}"));
                }
            }
            else if (args.Id == "mentionDropDownList")
            {
                var options = args.Values;

                foreach (var mention in options)
                {
                    var selectedMention = await Client.GetUserAsync(ulong.Parse(mention));

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent($"{args.User.Username} selected the user with name {selectedMention.Username}"));
                }
            }

            //button events
            switch (args.Interaction.Data.CustomId)
            {
                case "button1":
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent("You clicked Button 1!"));
                    break;
                case "button2":
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent("You clicked Button 2!"));
                    break;
                case "basicsBtn":
                    var basicCommandsEmbed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Black,
                        Title = "Basic Commands",
                        Description = "Here are some basic commands you can use:\n" +
                                      "`!help` - Displays this help message.\n" +
                                      "`!calculate <num1> <operation> <num2>` - Performs a calculation.\n" +
                                      "`!button` - Displays buttons for interaction."
                    };
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(basicCommandsEmbed));
                    break;
                case "calculatorBtn":
                    var calculatorEmbed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Black,
                        Title = "Calculator Commands",
                        Description = "Here are some calculator commands you can use:\n" +
                                      "`!calculate <num1> <operation> <num2>` - Performs a calculation.\n" +
                                      "`!poll <opt1> <opt2> <opt3> <opt4> <pollTitle>` - Creates a poll."
                    };

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(calculatorEmbed));
                    break;
            }
        }

        private static async Task CommandEventHandler(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            if (e.Exception is ChecksFailedException ex)
            {
                string timeLeft = string.Empty;

                foreach(var check in ex.FailedChecks)
                {
                    var cooldown = (CooldownAttribute)check;
                    timeLeft = cooldown.GetRemainingCooldown(e.Context).ToString(@"hh\:mm\:ss");
                }

                var coolDownMessage = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = "Please wait for cooldown to end",
                    Description = $"Time: {timeLeft}"
                };

                await e.Context.Channel.SendMessageAsync(embed: coolDownMessage);
            }
        }

        private static async Task VoiceChannelHandler(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            if (e.Before == null  && e.Channel.Name == "General")
            {
                await e.Channel.SendMessageAsync($"{e.User.Username} has joined the voice channel.");
            }
        }

        private static async Task MessageCreatedHandler(DiscordClient sender, MessageCreateEventArgs e)
        {
            return;
        }

        private static async Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {

            await sender.UpdateStatusAsync(
                new DiscordActivity("Music | /help", ActivityType.Playing),
                UserStatus.Online
                );
            return;
        }
    }
}
