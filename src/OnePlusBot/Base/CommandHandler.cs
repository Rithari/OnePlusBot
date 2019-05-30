using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using OnePlusBot.Helpers;

namespace OnePlusBot.Base
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _bot;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient bot, CommandService commands, IServiceProvider services)
        {
            _bot = bot;
            _commands = commands;
            _services = services;
        }

        public async Task InstallCommandsAsync()
        {
            _bot.MessageReceived += OnCommandReceived;
            _bot.MessageReceived += OnMessageReceived;  
            _commands.CommandExecuted += OnCommandExecutedAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private static async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!string.IsNullOrEmpty(result?.ErrorReason))
            {
                await context.Channel.EmbedAsync(
                    new EmbedBuilder()
                        .WithColor(9896005)
                        .WithDescription("\u26A0 " + result.ErrorReason)
                        .WithTitle(context.Message.Author.ToString()));
            }
        }

        private static async Task RoleReact(IUserMessage message)
        {
            Global.RoleManagerMessageId = message.Id;
            
            await message.AddReactionsAsync(new IEmote[]
            {
                Emote.Parse("<:1_:574655515586592769>"),
                Emote.Parse("<:2_:574655515548844073>"),
                Emote.Parse("<:X_:574655515481866251>"),
                Emote.Parse("<:3_:574655515452506132>"),
                Emote.Parse("<:3T:574655515846508554>"),
                Emote.Parse("<:5_:574655515745976340>"),
                Emote.Parse("<:5T:574655515494318109>"),
                Emote.Parse("<:6_:574655515615952896>"),
                Emote.Parse("<:6T:574655515846508573>"),
                Emote.Parse("<:7_:574655515603501077>"),
                Emote.Parse("<:7P:574655515230076940>"),
                new Emoji("\u2753"), 
                new Emoji("\uD83D\uDCF0")
            });
        }

        private static async Task ValidateSetupsMessage(SocketMessage message)
        {
            if (!Regex.IsMatch(message.Content, @"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$")
                && message.Attachments.Count == 0
                && message.Embeds.Count == 0)
            {
                await message.DeleteAsync();
            }
        }

        private static bool IsValidReferralMessage(SocketMessage message)
        {
            if (!Regex.IsMatch(message.Content, @"^https?:\/\/[^\s]+$"))
                return false;
            
            using (var db = new Database())
            {
                return db.ReferralCodes.All(x => x.Sender != message.Author.Id ||
                                                 (DateTime.UtcNow - x.Date).Days >= 14);
            }
        }

        private static async Task OnMessageReceived(SocketMessage message)
        {
            var channelId = message.Channel.Id;
            if (channelId == Global.Channels["setups"])
            {
                await ValidateSetupsMessage(message);
            }
            else if (channelId == Global.Channels["info"])
            {
                Console.WriteLine("info channel");
                if (message.Embeds.Count == 1)
                {
                    var userMessage = (IUserMessage) message;
                    await RoleReact(userMessage);
                }
            }
            else if (channelId == Global.Channels["referralcodes"])
            {
                if (message.Author.IsBot)
                    return;
                
                if (IsValidReferralMessage(message))
                {
                    var index = message.Content.IndexOf("invite#", StringComparison.OrdinalIgnoreCase);
                    var code = message.Content.Substring(index + 7);
                    
                    using (var db = new Database())
                    {
                        db.ReferralCodes.Add(new ReferralCode
                        {
                            Code = code,
                            Date = message.Timestamp.Date,
                            Sender = message.Author.Id
                        });
                        db.SaveChanges();
                    }
                }
                else
                {
                    await message.DeleteAsync();
                    const string msg = "{0} Please include only the link in the message.\n" +
                                       "Invites can be bumped once every 2 weeks.";
                    var reply = await message.Channel.SendMessageAsync(string.Format(msg, message.Author.Mention));
                    
                    new Task(async () =>
                    {
                        await Task.Delay(5000);
                        await reply.DeleteAsync();
                    }).Start();
                }
            }
        }

        private async Task OnCommandReceived(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message)) 
                return;
            if (message.Author.IsBot)
                return;
            
            int argPos = 0;
            if (!message.HasCharPrefix(';', ref argPos) && 
                !message.HasMentionPrefix(_bot.CurrentUser, ref argPos))
                return;

            var context = new SocketCommandContext(_bot, message);

            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services);
        }
    }
}
