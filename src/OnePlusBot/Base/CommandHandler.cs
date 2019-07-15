using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using OnePlusBot.Helpers;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OnePlusBot.Base;

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
            _bot.UserJoined += OnuserUserJoined;
            _bot.UserLeft += OnUserLeft;
            _bot.MessageReceived += OnCommandReceived;
            _bot.MessageReceived += OnMessageReceived;
            _bot.MessageDeleted += OnMessageRemoved;
            _bot.MessageUpdated += OnMessageUpdated;
            _bot.UserBanned += OnUserBanned;
            _bot.UserUnbanned += OnUserUnbanned;
            _commands.CommandExecuted += OnCommandExecutedAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task OnUserLeft(SocketGuildUser socketGuildUser)
        {
            var modlog = socketGuildUser.Guild.GetTextChannel(Global.Channels["joinlog"]);
            await modlog.SendMessageAsync(socketGuildUser.Mention + " left the guild");
        }

        private async Task OnuserUserJoined(SocketGuildUser socketGuildUser)
        {
            var modlog = socketGuildUser.Guild.GetTextChannel(Global.Channels["joinlog"]);
            await modlog.SendMessageAsync(socketGuildUser.Mention + " joined the guild");
        }

        private async Task OnUserUnbanned(SocketUser socketUser, SocketGuild socketGuild)
        {
            var modlog = socketGuild.GetTextChannel(Global.Channels["modlog"]);

            var restAuditLogs = await socketGuild.GetAuditLogsAsync(10).FlattenAsync();

            var unbanLog = restAuditLogs.FirstOrDefault(x => x.Action == ActionType.Unban);


            await modlog.EmbedAsync(new EmbedBuilder()
                .WithColor(9896005)
                .WithTitle("♻️ Unbanned User")
                .AddField(efb => efb
                    .WithName("Username")
                    .WithValue(socketUser.ToString())
                    .WithIsInline(true))
                .AddField(efb => efb
                    .WithName("ID")
                    .WithValue(socketUser.Id.ToString())
                    .WithIsInline(true))
                .AddField(efb => efb
                    .WithName("By")
                    .WithValue(unbanLog.User)
                    .WithIsInline(true)));
        }

        private async Task OnUserBanned(SocketUser socketUser, SocketGuild socketGuild)
        {
            var modlog = socketGuild.GetTextChannel(Global.Channels["modlog"]);

            var restAuditLogs = await socketGuild.GetAuditLogsAsync(10).FlattenAsync(); //As above, might be unnecessary as requests come in packs of 100.

            var banLog = restAuditLogs.FirstOrDefault(x => x.Action == ActionType.Ban);


            await modlog.EmbedAsync(new EmbedBuilder()
                .WithColor(9896005)
                .WithTitle("⛔️ Banned User")
                .AddField(efb => efb
                    .WithName("Username")
                    .WithValue(socketUser.ToString())
                    .WithIsInline(true))
                .AddField(efb => efb
                    .WithName("ID")
                    .WithValue(socketUser.Id.ToString())
                    .WithIsInline(true))
                .AddField(efb => efb
                    .WithName("Reason")
                    .WithValue(banLog.Reason)
                    .WithIsInline(true))
                .AddField(efb => efb
                    .WithName("By")
                    .WithValue(banLog.User)
                    .WithIsInline(true)));
        }

        private async Task OnMessageUpdated(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel socketChannel)
        {
            var channel = (SocketTextChannel)socketChannel;
            var before = await cacheable.GetOrDownloadAsync();
            var author = before.Author;

            if (before.Author == _bot.CurrentUser || message.Author == _bot.CurrentUser || before.Content == "" || message.Content == "")
                return;
            if (before.Content == message.Content)
                return;

            if(before.Author.IsBot)
                return;

            var embed = new EmbedBuilder
            {
                Color = Color.Blue,
                Description = $":bulb: Message from '{author.Username}' edited in {channel.Mention}",
                Fields = {
                    new EmbedFieldBuilder()
                    {
                        IsInline = false,
                        Name = $"Original message: ",
                        Value = before.Content
                    },
                    new EmbedFieldBuilder()
                    {
                        IsInline = false,
                        Name = $"New message: ",
                        Value = message.Content
                    }
                },
                ThumbnailUrl = author.GetAvatarUrl(),
                Timestamp = DateTime.Now
            };

            await channel.Guild.GetTextChannel(Global.Channels["modlog"]).SendMessageAsync(embed: embed.Build());
        }

        private async Task OnMessageRemoved(Cacheable<IMessage, ulong> cacheable, ISocketMessageChannel socketChannel)
        {
            var deletedMessage = await cacheable.GetOrDownloadAsync();
            var channel = (SocketTextChannel)socketChannel;

            var embed = new EmbedBuilder
            {
                Color = Color.Blue,
                Description = $":bulb: Message from '{cacheable.Value.Author.Username}' removed in {channel.Mention}",
                Fields = {
                    new EmbedFieldBuilder() { IsInline = false, Name = $":x: Original message: ", Value = cacheable.Value.Content },
                },
                ThumbnailUrl = cacheable.Value.Author.GetAvatarUrl(),
                Timestamp = DateTime.Now
            };
            await channel.Guild.GetTextChannel(Global.Channels["modlog"]).SendMessageAsync(embed: embed.Build());

        }

        private static async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            switch(result)
            {
                case CustomResult customResult:
                    if (customResult.IsSuccess)
                    {
                        await context.Message.AddReactionAsync(Global.OnePlusEmote.SUCCESS);
                    }
                    else
                    {
                        await context.Message.AddReactionAsync(Global.OnePlusEmote.FAIL);
                        await context.Channel.SendMessageAsync(customResult.Reason);
                    }
                    break;

                default:
                 if (!string.IsNullOrEmpty(result?.ErrorReason))
                 {
                    if (result.ErrorReason == "Unknown command.")
                    return;

                    await context.Channel.SendMessageAsync(result.ErrorReason);
                    return;
                 }
                break;
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

        private static async Task HandleReferralMessage(SocketMessage message)
        {
            using (var db = new Database())
            {
                if (db.ReferralCodes.Any(x => (DateTime.UtcNow - x.Date).Days < 14 && 
                                              x.Sender == message.Author.Id))
                {
                    var msg = await message.Channel.SendMessageAsync($"{message.Author.Mention} You already have sent a referral in the last 2 weeks");
                    await message.DeleteAsync();
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                    return;
                }
            }

            var matches = Regex.Matches(message.Content, @"https?:\/\/(?:www\.)?oneplus\.com[^\s]*invite(?:\#([^\s]+)|.+\=([^\s\&]+))", RegexOptions.IgnoreCase);

            if (matches.Count > 2)
            {
                var msg = await message.Channel.SendMessageAsync($"{message.Author.Mention} Max 2 referrals per message");
                await Task.Delay(2000);
                await msg.DeleteAsync();
                return;
            }

            if(matches.Count == 0)
            {
                var msg = await message.Channel.SendMessageAsync($"{message.Author.Mention} Post a referral code!");
                await message.DeleteAsync();
                await Task.Delay(2000);
                await msg.DeleteAsync();
                return;
            }
            
            var embed = new EmbedBuilder();
            embed.WithColor(9896005);
            embed.Author = new EmbedAuthorBuilder()
                .WithName(message.Author.Username)
                .WithIconUrl(message.Author.GetAvatarUrl());
            embed.Description = $"Sent by {message.Author.Mention}";
            
            foreach (Match match in matches)
            {
                using (var db = new Database())
                {
                    db.ReferralCodes.Add(new ReferralCode
                    {
                        Code = match.Groups[1].Value,
                        Date = message.CreatedAt.DateTime,
                        Sender = message.Author.Id
                    });
                    db.SaveChanges();
                }

                string name;
                // Cheap way to check without having a command
                if (match.Groups[1].Length < 20) 
                    name = "Smartphone";
                else
                    name = "Wireless Bullets 2";
                embed.AddField(new EmbedFieldBuilder()
                    .WithName(name)
                    .WithValue($"Referral: [#{match.Groups[1].Value}]({match.Value})"));
            }

            await message.Channel.EmbedAsync(embed);
            
            await message.DeleteAsync();
        }

        private static async Task OnMessageReceived(SocketMessage message)
        {
            if (Regex.IsMatch(message.Content, @"discord(?:\.gg|app\.com\/invite)\/([\w\-]+)") && message.Channel.Id != Global.Channels["referralcodes"] && !message.Content.Contains("discord.gg/oneplus"))
                await message.DeleteAsync();

            var channelId = message.Channel.Id;

            if (channelId == Global.Channels["setups"])
            {
                await ValidateSetupsMessage(message);
            }
            else if (channelId == Global.Channels["info"])
            {
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

                await HandleReferralMessage(message);
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
