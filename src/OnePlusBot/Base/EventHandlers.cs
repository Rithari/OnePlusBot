using System.IO;
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
using System.Net;
using System.Collections.Generic;

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
            _bot.UserJoined += OnUserJoinedMuteCheck;
            _bot.UserLeft += OnUserLeft;
            _bot.MessageReceived += OnCommandReceived;
            _bot.MessageReceived += OnMessageReceived;
            _bot.MessageReceived += HandleExpGain;
            _bot.MessageDeleted += OnMessageRemoved;
            _bot.MessageUpdated += OnMessageUpdated;
            _bot.UserUnbanned += OnUserUnbanned;
            _bot.UserVoiceStateUpdated += UserChangedVoiceState;
            _commands.CommandExecuted += OnCommandExecutedAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task OnUserLeft(SocketGuildUser socketGuildUser)
        {
            var leaveLog = socketGuildUser.Guild.GetTextChannel(Global.PostTargets[PostTarget.LEAVE_LOG]);
            await leaveLog.SendMessageAsync(Extensions.FormatMentionDetailed(socketGuildUser) + " left the guild");
        }

        private async Task OnuserUserJoined(SocketGuildUser socketGuildUser)
        {
            var joinlog = socketGuildUser.Guild.GetTextChannel(Global.PostTargets[PostTarget.JOIN_LOG]);
            string name = socketGuildUser.Username;
            if(Global.IllegalUserNameBeginnings.Contains(name[0]))
            {
                var modQueue = socketGuildUser.Guild.GetTextChannel(Global.PostTargets[PostTarget.USERNAME_QUEUE]);
                var builder = new EmbedBuilder();
                builder.Title = "User with illegal character joined!";
                builder.Description = Extensions.FormatUserNameDetailed(socketGuildUser);
                builder.Color = Color.DarkBlue;
                
                builder.Timestamp = DateTime.Now;
                
                builder.ThumbnailUrl = socketGuildUser.GetAvatarUrl();
                await modQueue.SendMessageAsync(embed: builder.Build());
            }
            await joinlog.SendMessageAsync(Extensions.FormatMentionDetailed(socketGuildUser) + " joined the guild");
        }

        private async Task OnUserJoinedMuteCheck(SocketGuildUser user)
        {
            using (var db = new Database())
            {
                if(db.Mutes.Where(us => us.MutedUserID == user.Id && !us.MuteEnded).Any())
                {
                    await Extensions.MuteUser(user);
                }
            }
        }

        private async Task OnUserUnbanned(SocketUser socketUser, SocketGuild socketGuild)
        {
            var unBanlogChannel = socketGuild.GetTextChannel(Global.PostTargets[PostTarget.UNBAN_LOG]);

            var restAuditLogs = await socketGuild.GetAuditLogsAsync(10).FlattenAsync();

            var unbanLog = restAuditLogs.FirstOrDefault(x => x.Action == ActionType.Unban);


            await unBanlogChannel.EmbedAsync(new EmbedBuilder()
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

        private async Task OnMessageUpdated(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel socketChannel)
        {
            var before = await cacheable.GetOrDownloadAsync();
            var author = before.Author;

            if (before.Author == _bot.CurrentUser || message.Author == _bot.CurrentUser || before.Content == "" || message.Content == "")
                return;
            if (before.Content == message.Content)
                return;

            if (ViolatesRule(message))
            {
                await message.DeleteAsync();
            }


            var fullChannel = Extensions.GetChannelById(message.Channel.Id);
            if(fullChannel != null){
                if(!fullChannel.ProfanityExempt()){
                    var profanityChecks = Global.ProfanityChecks;
                    var lowerMessage = message.Content.ToLower();
                    foreach (var profanityCheck in profanityChecks)
                    {
                        if(profanityCheck.RegexObj.Match(lowerMessage).Success)
                        {
                            await ReportProfanity(message, profanityCheck);
                            break;
                        }
                    }
                }
            }

            if(before.Author.IsBot)
                return;
            

            if(socketChannel is SocketTextChannel){
                if(Global.NewsPosts.ContainsKey(message.Id))
                {
                    var split = message.Content.Split(";news");
                    if(split.Length > 0)
                    {
                        var guild = Global.Bot.GetGuild(Global.ServerID);
                        var newsChannel = guild.GetTextChannel(Global.PostTargets[PostTarget.NEWS]);
                        var newsRole = guild.GetRole(Global.Roles["news"]);
                        var rawMessage = await newsChannel.GetMessageAsync(Global.NewsPosts[message.Id]);
                        var existingMessage = rawMessage as SocketUserMessage;
                        await newsRole.ModifyAsync(x => x.Mentionable = true);
                        await existingMessage.ModifyAsync(x => x.Content = split[1] + Environment.NewLine + Environment.NewLine + newsRole.Mention + Environment.NewLine + "- " + author);
                        await newsRole.ModifyAsync(x => x.Mentionable = false);
                    }
                }

                var channel = (SocketTextChannel)socketChannel;

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

                await channel.Guild.GetTextChannel(Global.PostTargets[PostTarget.EDIT_LOG]).SendMessageAsync(embed: embed.Build());
            }
        }

        private async Task OnMessageRemoved(Cacheable<IMessage, ulong> cacheable, ISocketMessageChannel socketChannel)
        {

            var channel = (SocketTextChannel)socketChannel;
            IMessage deletedMessage = null;
            // this happened sometimes
            try 
            {
                deletedMessage = await cacheable.GetOrDownloadAsync();
            }
            catch(NullReferenceException)
            {
                return;
            }
            // it was sometimes null
            if(deletedMessage == null)
            {
                return;
            }

            if(channel.Id == Global.Channels[Channel.STARBOARD])
            {
                var starPost = Global.StarboardPosts.Where(po => po.StarboardMessageId == deletedMessage.Id).DefaultIfEmpty(null).First();
                if(starPost != null)
                {
                    using(var db = new Database())
                    {
                        var existingPost = db.StarboardMessages.Where(po => po.StarboardMessageId == starPost.StarboardMessageId).First();
                        existingPost.Ignored = true;
                        db.SaveChanges();
                    }
                }
            }

            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            var originalMessage = "";
            // I distinctly remember having a null value once, couldnt find the situation again for that tho
            // the check should not be too bad, it should short circuit anyway
            if(cacheable.Value == null || cacheable.Value.Content == "" || cacheable.Value.Content == null)
            {
                originalMessage = "none";
            } 
            else 
            {
                originalMessage = cacheable.Value.Content;
            }
            fields.Add(new EmbedFieldBuilder() { IsInline = false, Name = $":x: Original message: ", Value = originalMessage });
            if(deletedMessage != null && deletedMessage.Attachments != null){
                    // you can upload multiple attachments at once on mobile
                var attachments = deletedMessage.Attachments.ToList();
                if(attachments.Count > 0)
                {
                    fields.Add(new EmbedFieldBuilder() { IsInline = false, Name = $":frame_photo: Amount of attachments: ", Value = attachments.Count });
                }
                
                var embed = new EmbedBuilder
                {
                    Color = Color.Blue,
                    Description = $":bulb: Message from '{cacheable.Value.Author.Username}' removed in {channel.Mention}",
                    Fields = fields,
                    ThumbnailUrl = cacheable.Value.Author.GetAvatarUrl(),
                    Timestamp = DateTime.Now
                };
                await channel.Guild.GetTextChannel(Global.PostTargets[PostTarget.DELETE_LOG]).SendMessageAsync(embed: embed.Build());

                WebClient client = new WebClient();
                for(int index = 0; index < attachments.Count; index++)
                {
                    var oneBasedIndex = index + 1;
                    var targetFileName = attachments.ElementAt(index).Filename;
                    var url = attachments.ElementAt(index).Url;
                    try 
                    {
                        await Task.Delay(500);
                        client.DownloadFile(url, targetFileName);
                        var upperFileName = targetFileName.ToUpper();
                        var attachmentDescription = "Attachment #" + oneBasedIndex;
                        if(upperFileName.EndsWith("JPG") || upperFileName.EndsWith("PNG") || upperFileName.EndsWith("GIF"))
                        {
                            var attachmentString = $"attachment://{targetFileName}";
                            var pictureEmbed = new EmbedBuilder()
                            {
                                Color = Color.Blue,
                                Footer = new EmbedFooterBuilder() { Text =  attachmentDescription},
                                ImageUrl = attachmentString,
                            };
                            await channel.Guild.GetTextChannel(Global.PostTargets[PostTarget.DELETE_LOG]).SendFileAsync(targetFileName, "", embed: pictureEmbed.Build());
                        } 
                        else 
                        {
                            await channel.Guild.GetTextChannel(Global.PostTargets[PostTarget.DELETE_LOG]).SendFileAsync(targetFileName, attachmentDescription);
                        }
                    }
                    catch(WebException webEx)
                    {
                        if (webEx.Status == WebExceptionStatus.ProtocolError)
                        {
                            var response = webEx.Response as HttpWebResponse;
                            if (response != null)
                            {
                                var exceptionEmbed = new EmbedBuilder    
                                {
                                    Color = Color.Red,
                                    Description = $"Discord did not let us download attachment #" + oneBasedIndex,
                                    Fields = {new EmbedFieldBuilder() { IsInline = false, Name = $":x: It returned ", Value = (int)response.StatusCode }},
                                    ThumbnailUrl = cacheable.Value.Author.GetAvatarUrl(),
                                };
                                await channel.Guild.GetTextChannel(Global.PostTargets[PostTarget.DELETE_LOG]).SendMessageAsync(embed: exceptionEmbed.Build());
                            }
                            else
                            {
                                SendExceptionEmbed(cacheable, webEx, channel);
                            }
                        }
                        else
                        {
                            SendExceptionEmbed(cacheable, webEx, channel);
                        }
                    }
                    catch(Exception ex)
                    {
                        SendExceptionEmbed(cacheable, ex, channel);
                    }
                    finally 
                    {
                        File.Delete(targetFileName);  
                    }     
                }
            }
        }

        private static async void SendExceptionEmbed(Cacheable<IMessage, ulong> cacheable, Exception exception, SocketTextChannel channel)
        {
            var exceptionEmbed = new EmbedBuilder    
            {
                Color = Color.Red,
                Description = $"Error when downloading or posting the attachment.",
                Fields = {
                    new EmbedFieldBuilder() { IsInline = false, Name = $":x: Exception type ", Value = exception.GetType().FullName },
                    new EmbedFieldBuilder() { IsInline = false, Name = $" Exception message", Value = exception.Message }
                },
                ThumbnailUrl = cacheable.Value.Author.GetAvatarUrl(),
                Timestamp = DateTime.Now
            };
            await channel.Guild.GetTextChannel(Global.PostTargets[PostTarget.DELETE_LOG]).SendMessageAsync(embed: exceptionEmbed.Build());
        }

        private static async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            switch(result)
            {
                case PreconditionResult conditionResult:
                    if (conditionResult.IsSuccess)
                    {
                        await context.Message.AddReactionAsync(Global.OnePlusEmote.SUCCESS);
                    }
                    else
                    {
                        await context.Message.AddReactionAsync(Global.OnePlusEmote.FAIL);
                        await context.Channel.SendMessageAsync(conditionResult.ErrorReason);
                    }
                    break;
                case CustomResult customResult:
                    if(customResult.Ignore)
                    {
                        return;
                    }
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

                    await context.Message.AddReactionAsync(Global.OnePlusEmote.FAIL);

                    if (result.ErrorReason == "The input text has too few parameters.")
                    {
                        return;
                    }
                  

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

                Emote.Parse("<a:1_:623172008540110859>"),
                Emote.Parse("<a:2_:623171995684831252>"),
                Emote.Parse("<a:X_:623171985542742027>"),
                Emote.Parse("<a:3_:623171978198777856>"),
                Emote.Parse("<a:3T:623171970326069268>"),
                Emote.Parse("<a:5_:623171961400590366>"),
                Emote.Parse("<a:5T:623171951510159371>"),
                Emote.Parse("<a:6_:623171940647043092>"),
                Emote.Parse("<a:6T:623171933407674369>"),
                Emote.Parse("<a:7_:623171923706380329>"),
                Emote.Parse("<a:7P:623171916236324874>"),
                Emote.Parse("<a:7T:623171903447760906>"),
                Emote.Parse("<a:7TP:625640956133113869>"),
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
            var safeUsername = Extensions.FormatMentionDetailed(message.Author);
            embed.Description = $"Sent by {safeUsername}";
            
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

        private static async Task ReportProfanity(SocketMessage message, ProfanityCheck usedProfanity){

            var guild = Global.Bot.GetGuild(Global.ServerID);
            var builder = new EmbedBuilder();
            builder.Title = "Profanity has been used!";
            builder.Color = Color.DarkBlue;
            
            builder.Timestamp = message.Timestamp;
            
            builder.ThumbnailUrl = message.Author.GetAvatarUrl();

            builder.AddField("User in question ", Extensions.FormatMentionDetailed(message.Author))
                .AddField(f => {
                    f.Name = "Location of the profane message";
                    f.Value = Extensions.GetMessageUrl(Global.ServerID, message.Channel.Id, message.Id, message.Channel.Name);
                    f.IsInline = true;
                    })
                .AddField(f => {
                    f.Name ="Profanity type";
                    f.Value = usedProfanity.Label;
                    f.IsInline = true;
                    })
                .AddField("Message content", message.Content);


            var embed = builder.Build();
            var modQueue = guild.GetTextChannel(Global.PostTargets[PostTarget.PROFANITY_QUEUE]);

            var report = await modQueue.SendMessageAsync(null,embed: embed);

            await report.AddReactionsAsync(new IEmote[]
            {
                Global.OnePlusEmote.OP_YES, 
                Global.OnePlusEmote.OP_NO
            });

            var profanity = new UsedProfanity();
            profanity.MessageId = report.Id;
            profanity.UserId = message.Author.Id;
            profanity.Valid = false;
            profanity.ProfanityId = usedProfanity.ID;
            using(var db = new Database()){
                var user = db.Users.Where(us => us.Id == message.Author.Id).FirstOrDefault();
                if(user == null)
                {
                    var newUser = new User();
                    newUser.Id = message.Author.Id;
                    newUser.ModMailMuted = false;
                    db.Users.Add(newUser);
                }
               
                db.Profanities.Add(profanity);
                db.SaveChanges();
            }

            Global.ReportedProfanities.Add(profanity);
           
        }

        private static bool ModMailThreadForUserExists(IUser user){
            return Global.ModMailThreads.Exists(ch => ch.UserId == user.Id && ch.State != "CLOSED");
        }

        private static bool ContainsIllegalInvite(string message)
        {
            MatchCollection groups =  Regex.Matches(message, @"discord(\.gg|app\.com/invite)/[\w\-]+");
            foreach(Group gr in groups){
                CaptureCollection captures =  gr.Captures;

                foreach(Capture capture in captures)
                {
                    var linkExists = Global.InviteLinks.Exists(link => capture.Value.Contains(link.Link));
                    if(!linkExists)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool ViolatesRule(SocketMessage message)
        {
        /*    var guildUser = message.Author as IGuildUser;
            if (message.Channel is SocketDMChannel)
            {
                if (guildUser.RoleIds.Contains(Global.Roles["staff"]))
                {
                    var guild = Global.Bot.GetGuild(Global.ServerID);
                    var feedbackChannel = guild.GetTextChannel(Global.Channels["feedback"]);
                    feedbackChannel.SendMessageAsync("Feedback!" + Environment.NewLine + message.Content);
                }
            }*/

            string messageText = message.Content;
            var channelObj = Global.FullChannels.Where(ch => ch.ChannelID == message.Channel.Id).FirstOrDefault();
            bool ignoredChannel = channelObj != null && channelObj.InviteCheckExempt();
            return ContainsIllegalInvite(messageText) && message.Channel.Id != Global.Channels[Channel.REFERRAL] && !ignoredChannel;
        }

        private static async Task HandleExpGain(SocketMessage message)
        {
            if(message.Author.IsBot)
            {
                return;
            }
            if(Global.XPGainDisabled)
            {
                return;
            }
            var channelObj = Global.FullChannels.Where(ch => ch.ChannelID == message.Channel.Id).FirstOrDefault();
            bool ignoredChannel = channelObj != null && channelObj.ExperienceGainExempt();
            if(ignoredChannel)
            {
                return;
            }
            
            var minute = (long) DateTime.Now.Subtract(DateTime.MinValue).TotalMinutes;
            var exists = Global.RuntimeExp.ContainsKey(minute);
            if(!exists){
                var element = new List<ulong>();
                element.Add(message.Author.Id);
                Global.RuntimeExp.Add(minute, element);
            }
            else
            {
                var poster = Global.RuntimeExp[minute];
                if(!poster.Contains(message.Author.Id)){
                    poster.Add(message.Author.Id);
                }
            }
            await Task.CompletedTask;
        }

        private static async Task OnMessageReceived(SocketMessage message)
        {
            if (ViolatesRule(message))
            {
                await message.DeleteAsync();
            }

            var channel = Extensions.GetChannelById(message.Channel.Id);
            if(channel != null){
                if(!channel.ProfanityExempt()){
                    var profanityChecks = Global.ProfanityChecks;
                    var lowerMessage = message.Content.ToLower();
                    foreach (var profanityCheck in profanityChecks)
                    {
                        if(profanityCheck.RegexObj.Match(lowerMessage).Success)
                        {
                            await ReportProfanity(message, profanityCheck);
                            break;
                        }
                    }
                }
            }

            if(message.Author.IsBot)
            {
                return;
            }
            var modmailThread = ModMailThreadForUserExists(message.Author);

            if(modmailThread && message.Channel is IDMChannel)
            {
                await new ModMailManager().HandleModMailUserReply(message);
            }
            else if(message.Channel is IDMChannel)
            {
                await new ModMailManager().CreateModmailThread(message);
            }



               
            var channelId = message.Channel.Id;

            if (channelId == Global.Channels[Channel.SETUPS])
            {
                await ValidateSetupsMessage(message);
            }
            else if (channelId == Global.Channels[Channel.INFO])
            {
                if (message.Embeds.Count == 1)
                {
                    var userMessage = (IUserMessage) message;
                    await RoleReact(userMessage);
                }
            }
            else if (channelId == Global.Channels[Channel.REFERRAL])
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

            if(message.Channel is IDMChannel)
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

        private async Task UserChangedVoiceState(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {

            if(oldState.VoiceChannel != null && newState.VoiceChannel != null)
            {
                if(oldState.VoiceChannel.Id == newState.VoiceChannel.Id)
                {
                    return;
                }
            }

            if(newState.VoiceChannel == null)
            {
                if(newState.IsDeafened)
                {
                    return;
                }
            }

            var bot = Global.Bot;
            var guild = bot.GetGuild(Global.ServerID);
            
            if(newState.VoiceChannel != null)
            {
                var targetNotesChannel = newState.VoiceChannel.Name + "notes";
                if(Global.Channels.ContainsKey(targetNotesChannel))
                {
                    var notesChannel = guild.GetChannel(Global.Channels[targetNotesChannel]);
                    var nullablePermissionObj = notesChannel.GetPermissionOverwrite(user);
                    var permission = nullablePermissionObj.HasValue ? nullablePermissionObj.Value : new OverwritePermissions();
                    permission = permission.Modify(viewChannel:PermValue.Allow, sendMessages:PermValue.Allow);     
                    await notesChannel.AddPermissionOverwriteAsync(user, permission);
                }
            }

            if(oldState.VoiceChannel != null)
            {
                var sourceNotesChannel = oldState.VoiceChannel.Name + "notes";
                if(Global.Channels.ContainsKey(sourceNotesChannel))
                {
                    var notesChannel = guild.GetChannel(Global.Channels[sourceNotesChannel]);
                    await notesChannel.RemovePermissionOverwriteAsync(user);
                }
                
            }
        }
    }
}
