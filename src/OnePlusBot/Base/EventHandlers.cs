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
using OnePlusBot.Base;
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
            _bot.UserLeft += OnUserLeft;
            _bot.MessageReceived += OnCommandReceived;
            _bot.MessageReceived += OnMessageReceived;
            _bot.MessageDeleted += OnMessageRemoved;
            _bot.MessageUpdated += OnMessageUpdated;
            _bot.UserBanned += OnUserBanned;
            _bot.UserUnbanned += OnUserUnbanned;
            _bot.UserVoiceStateUpdated += UserChangedVoiceState;
            _commands.CommandExecuted += OnCommandExecutedAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task OnUserLeft(SocketGuildUser socketGuildUser)
        {
            var modlog = socketGuildUser.Guild.GetTextChannel(Global.Channels["joinlog"]);
            await modlog.SendMessageAsync(Extensions.FormatMentionDetailed(socketGuildUser) + "left the guild");
        }

        private async Task OnuserUserJoined(SocketGuildUser socketGuildUser)
        {
            var modlog = socketGuildUser.Guild.GetTextChannel(Global.Channels["joinlog"]);
            await modlog.SendMessageAsync(Extensions.FormatMentionDetailed(socketGuildUser) + "joined the guild");
        }

        private async Task OnUserUnbanned(SocketUser socketUser, SocketGuild socketGuild)
        {
            var modlog = socketGuild.GetTextChannel(Global.Channels["banlog"]);

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
            var modlog = socketGuild.GetTextChannel(Global.Channels["banlog"]);

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
            
            if(Global.NewsPosts.ContainsKey(message.Id))
            {
                var split = message.Content.Split(";news");
                if(split.Length > 0)
                {
                    var guild = Global.Bot.GetGuild(Global.ServerID);
                    var newsChannel = guild.GetTextChannel(Global.Channels["news"]);
                    var newsRole = guild.GetRole(Global.Roles["news"]);
                    var existingMessage = await newsChannel.GetMessageAsync(Global.NewsPosts[message.Id]) as SocketUserMessage;
                    await newsRole.ModifyAsync(x => x.Mentionable = true);
                    await existingMessage.ModifyAsync(x => x.Content = split[1] + Environment.NewLine + Environment.NewLine + newsRole.Mention + Environment.NewLine + "- " + author);
                    await newsRole.ModifyAsync(x => x.Mentionable = false);
                }
            }

            var fullChannel = Extensions.GetChannelById(message.Channel.Id);
            if(fullChannel != null){
                if(!fullChannel.ProfanityCheckExempt){
                    var profanityChecks = Global.ProfanityChecks;
                    var lowerMessage = message.Content.ToLower();
                    foreach (var regexObj in profanityChecks)
                    {
                        if(regexObj.Match(lowerMessage).Success)
                        {
                            await ReportProfanity(message);
                            break;
                        }
                    }
                }
            }
            
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

            var channel = (SocketTextChannel)socketChannel;
            IMessage deletedMessage = null;
            // this happened sometimes
            try 
            {
                deletedMessage = await cacheable.GetOrDownloadAsync();
            }
            catch(NullReferenceException ex)
            {
                await channel.Guild.GetTextChannel(Global.Channels["modlog"]).SendMessageAsync(ex.ToString());
            }
            // it was sometimes null
            if(deletedMessage == null)
            {
                return;
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
                await channel.Guild.GetTextChannel(Global.Channels["modlog"]).SendMessageAsync(embed: embed.Build());

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
                            await channel.Guild.GetTextChannel(Global.Channels["modlog"]).SendFileAsync(targetFileName, "", embed: pictureEmbed.Build());
                        } 
                        else 
                        {
                            await channel.Guild.GetTextChannel(Global.Channels["modlog"]).SendFileAsync(targetFileName, attachmentDescription);
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
                                await channel.Guild.GetTextChannel(Global.Channels["modlog"]).SendMessageAsync(embed: exceptionEmbed.Build());
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
            await channel.Guild.GetTextChannel(Global.Channels["modlog"]).SendMessageAsync(embed: exceptionEmbed.Build());
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

        private static void CacheAttachment(SocketMessage message)
        {
            // this causes the pic to not disappear resulting in an 403, in case the image is deleted instantly
            WebClient client = new WebClient();
            var attachments = message.Attachments;
            for(int index = 0; index < attachments.Count; index++)
            {
                var targetFileName = attachments.ElementAt(index).Filename;
                var url = attachments.ElementAt(index).Url;
                try 
                {
                    client.DownloadFile(url, targetFileName); 
                } 
                finally 
                {
                    File.Delete(targetFileName);   
                }
            }
            
        }

        private static async Task ReportProfanity(SocketMessage message){

            var guild = Global.Bot.GetGuild(Global.ServerID);
            var builder = new EmbedBuilder();
            builder.Title = "Profanity has been used!";
            builder.Color = Color.DarkBlue;
            
            builder.Timestamp = message.Timestamp;
            
            builder.ThumbnailUrl = message.Author.GetAvatarUrl();

            const string discordUrl = "https://discordapp.com/channels/{0}/{1}/{2}";
            builder.AddField("User in question ", Extensions.FormatMentionDetailed(message.Author))
                .AddField(
                    "Location of the profane message",
                    $"[#{message.Channel.Name}]({string.Format(discordUrl, guild.Id, message.Channel.Id, message.Id)})")
                .AddField("Message content", message.Content);


            var embed = builder.Build();
            var modQueue = guild.GetTextChannel(Global.Channels["modqueuetest"]);;

            await modQueue.SendMessageAsync(null,embed: embed).ConfigureAwait(false);
        }

        private static async Task OnMessageReceived(SocketMessage message)
        {
            if (Regex.IsMatch(message.Content, @"discord(?:\.gg|app\.com\/invite)\/([\w\-]+)") && message.Channel.Id != Global.Channels["referralcodes"] && !message.Content.Contains("discord.gg/oneplus"))
                await message.DeleteAsync();
            
            if(message.Attachments.Count > 0 && !message.Author.IsBot)
            {
               CacheAttachment(message);
            }
            var channel = Extensions.GetChannelById(message.Channel.Id);
            if(channel != null){
                if(!channel.ProfanityCheckExempt){
                    var profanityChecks = Global.ProfanityChecks;
                    var lowerMessage = message.Content.ToLower();
                    foreach (var regexObj in profanityChecks)
                    {
                        if(regexObj.Match(lowerMessage).Success)
                        {
                            await ReportProfanity(message);
                            break;
                        }
                    }
                }
            }
           
               
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

        private async Task UserChangedVoiceState(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
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
