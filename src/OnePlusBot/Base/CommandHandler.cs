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
            _bot.MessageUpdated += OnMessageUpdatedAsync;
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

        private async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel socketChannel)
        {
            var channel = (SocketTextChannel)socketChannel;
            var before = await cacheable.GetOrDownloadAsync();
            var author = before.Author;

            if (before.Author == _bot.CurrentUser || message.Author == _bot.CurrentUser || before.Content == "" || message.Content == "")
                return;
            if (before.Content == message.Content)
                return;

            var embed = new EmbedBuilder
            {
                Color = Color.Blue,
                Description = $":bulb: Message from '{author.Username}' edited in {channel.Mention}",
                Fields = {
                    new EmbedFieldBuilder()
                    {
                        IsInline = false,
                        Name = $":x: Original message: ",
                        Value = before.Content
                    },
                    new EmbedFieldBuilder()
                    {
                        IsInline = false,
                        Name = $":pencil2: New message: ",
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
            if (!string.IsNullOrEmpty(result?.ErrorReason))
            {
                if (result.ErrorReason == "Unknown command.")
                    return;

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
            if (Regex.IsMatch(message.Content, @"discord(?:\.gg|app\.com\/invite)\/([\w\-]+)") && message.Channel.Id != Global.Channels["referralcodes"])
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
