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
using Microsoft.EntityFrameworkCore;
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

        private static Regex messageRegex = new Regex("(https://(?:(?:canary|ptb).)?discordapp.com/channels/(\\d+)/(\\d+)/(\\d+))+", RegexOptions.Singleline | RegexOptions.Compiled);

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
            _bot.UserJoined += OnUserJoinedRole;
            _bot.UserLeft += OnUserLeft;
            _bot.MessageReceived += OnCommandReceived;
            _bot.MessageReceived += OnMessageReceived;
            _bot.MessageReceived += OnMessageReceivedEmbed;
            _bot.MessageReceived += HandleExpGain;
            _bot.MessageDeleted += OnMessageRemoved;
            _bot.MessageUpdated += OnMessageUpdated;
            // when only listening to the guild member updated event, the before state of the user contained the same username
            // as the after state, so we need to listen on both
            _bot.GuildMemberUpdated += OnGuildMemberUpdated;
            _bot.UserUpdated += OnGlobalUserUpdated;
            _bot.UserUnbanned += OnUserUnbanned;
            _bot.UserVoiceStateUpdated += UserChangedVoiceState;
            _commands.CommandExecuted += OnCommandExecutedAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        /// <summary>
        /// Fires when the global user is updated and checks if the new user is in accordance with the defined username regex.
        /// In case it violates this regex and no nickname is set, this method calls another method responsible for notifying the moderators.
        /// Additionally, calls the method responsible for notifying currently active modmail threads.
        /// </summary>
        /// <param name="before">The <see cref="Discord.WebSocket.SocketUser"/> object containing the user before the update</param>
        /// <param name="after">The <see cref="Discord.WebSocket.SocketUser"/> object containing the user after the update</param>
        /// <returns>Task</returns>
        private async Task OnGlobalUserUpdated(SocketUser before, SocketUser after)
        {
          // fires when the username changed
          bool userNameChanged = before.Username != after.Username;
          SocketGuild guild = Global.Bot.GetGuild(Global.ServerID);
          SocketGuildUser userInGuild = guild.GetUser(after.Id);
          bool nicknameSet = userInGuild.Nickname != null;
          string beforeText = before.Username ?? "No username?";
          string afterText = after.Username ?? "No username?";
          if(userNameChanged && !nicknameSet)
          {
            if(IllegalUserName(after.Username))
            {
              string embedTitle = "User changed username to an illegal username";
             
              await NotifyAboutIllegalUserName(embedTitle, beforeText, afterText, after, guild);
            }
          }
          if(userNameChanged && !nicknameSet)
          {
            await HandleNameChangesForModmail(after, guild, "User Changed username", beforeText, afterText);
          }
        }

        /// <summary>
        /// In case there is a curently open modmail thread for the given user, 
        /// this method posts an embed with the given title and values for two fields called 'Before' and 'After'
        /// </summary>
        /// <param name="user">The <see cref="Discord.IUser" /> object to check if there exists a modmail thread.</param>
        /// <param name="guild">The <see cref"Discord.WebSocket.SocketGuild"/> object for which the check should be executed for. </param>
        /// <param name="embedTitle">Title of the embed</param>
        /// <param name="beforeText">Text for the field 'Before'</param>
        /// <param name="afterText">Text for the field 'After'</param>
        /// <returns>Task</returns>
        private async Task HandleNameChangesForModmail(IUser user, SocketGuild guild, string embedTitle, string beforeText, string afterText)
        {
          using(var db = new Database())
          {
            var modmailThread = db.ModMailThreads.Where(th => th.UserId == user.Id && th.State != "CLOSED");
            var modmailThreadExists = modmailThread.Any();
            if(modmailThreadExists)
            {
              var embed = GetUserNameNotificationEmbed(embedTitle, beforeText, afterText, user);
              await guild.GetTextChannel(modmailThread.First().ChannelId).SendMessageAsync(embed: embed);
            }  
          }
        }

        /// <summary>
        /// Builds the embed posted in case a user changes the username/nickname.
        /// </summary>
        /// <param name="embedTitle">Title of the embed</param>
        /// <param name="beforeText">The text of the field 'Before' for the embed</param>
        /// <param name="afterText">The text of the field 'After' for the embed</param>
        /// <param name="user">The <see cref"Discord.IUser"/> object to take the avatar as thumbnail</param>
        /// <returns>Embed containing build after the given parameters</returns>
        private Embed GetUserNameNotificationEmbed(string embedTitle, string beforeText, string afterText, IUser user)
        {
          EmbedBuilder builder = new EmbedBuilder();
          builder.Title = embedTitle;
          builder.AddField("User", user.Mention);
          builder.AddField("Before", beforeText);
          builder.AddField("After", afterText);
          builder.Color = Color.DarkBlue;
          builder.Timestamp = DateTime.Now;
          builder.WithFooter(new EmbedFooterBuilder().WithIconUrl(user.GetAvatarUrl()).WithText("ID: " + user.Id));
            
          return builder.Build();
        }

        /// <summary>
        /// Fires in case the guild user updates and checks if the new nickname (or username, if user removed the nickname) is in accordance of the rules for usernames.
        /// If not, calls the method to notify the moderators. Additionally, calls the method responsible for notifying currently active modmail threads.
        /// </summary>
        /// <param name="before">The <see cref"Discord.WebSocket.SocketGuildUser"/> object containing the user before the update</param>
        /// <param name="after">The <see cref"Discord.WebSocket.SocketGuildUser"/> object containing the user after the update</param>
        /// <returns>Task</returns>
        private async Task OnGuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
          // fires when the nickname changes
          bool nickNameChanged = before.Nickname != after.Nickname;
          bool nicknameSet = after.Nickname != null;
          string beforeText = before.Nickname ?? "No nickname";
          string afterText = after.Nickname ?? "No nickname";
          string embedTitle = "User changed nickname";
          if(nickNameChanged && nicknameSet) 
          {
            if(IllegalUserName(after.Nickname))
            {
              embedTitle = "User changed nickname to an illegal nickname";
            
              await NotifyAboutIllegalUserName(embedTitle, beforeText, afterText, after, after.Guild);
            }
          } 
          else if(nickNameChanged && !nicknameSet)
          {
            // in case the user reset its nickname to nothing
            if(IllegalUserName(after.Username))
            {
              embedTitle = "User removed nickname, and username is illegal";
              beforeText = before.Nickname;
              afterText = after.Username ?? "No username?";
              await NotifyAboutIllegalUserName(embedTitle, beforeText, afterText, after, after.Guild);
            }
          }
          if(nickNameChanged)
          {
            await HandleNameChangesForModmail(after, after.Guild, embedTitle, beforeText, afterText);
          }
        }

        /// <summary>
        /// Posts a message towards the 'UserNameQueue' post target containing the name before and after the change with the given title.
        /// The embed has as thumbnail the avatar of the user (none if default) and is posted towards the given guild.
        /// </summary>
        /// <param name="embedTitle">Title of the embed</param>
        /// <param name="beforeText">Text of the embed to be used for the 'Before' field</param>
        /// <param name="afterText">Text of the embed to be used for the 'After' field</param>
        /// <param name="user">User with the invalid username, used for the avatar in the embed.</param>
        /// <param name="guild">The <see cref"Discord.WebSocket.SocketGuild"/> guild where this is posted towards in the 'UserNameQueue' post target</param>
        /// <returns></returns>
        private async Task NotifyAboutIllegalUserName(string embedTitle, string beforeText, string afterText, IUser user, SocketGuild guild)
        {
          var embed = GetUserNameNotificationEmbed(embedTitle, beforeText, afterText, user);
          var userlog = guild.GetTextChannel(Global.PostTargets[PostTarget.USERNAME_QUEUE]);
          var message = await userlog.SendMessageAsync(embed: embed);
          await message.AddReactionAsync(Global.Emotes[Global.OnePlusEmote.OPEN_MODMAIL].GetAsEmote());
          Global.UserNameNotifications.Add(message.Id, user.Id);
        }

        private async Task OnUserLeft(SocketGuildUser socketGuildUser)
        {
          var leaveLog = socketGuildUser.Guild.GetTextChannel(Global.PostTargets[PostTarget.LEAVE_LOG]);
          var message = Extensions.FormatMentionDetailed(socketGuildUser) + " left the guild";
          await leaveLog.SendMessageAsync(message);
          using(var db = new Database())
          {
            var modmailThread = db.ModMailThreads.Where(th => th.UserId == socketGuildUser.Id && th.State != "CLOSED");
            var modmailThreadExists = modmailThread.Any();
            if(modmailThreadExists)
            {
              var embed = new EmbedBuilder().WithDescription(message).Build();
              await socketGuildUser.Guild.GetTextChannel(modmailThread.First().ChannelId).SendMessageAsync(embed: embed);
            }
          }
        }

        /// <summary>
        /// Fires when a user joins the guild. Checks if the username of the user is in accordance with the given username regex.
        /// If this is not the case, calls the method to notify the moderators.
        /// </summary>
        /// <param name="socketGuildUser">The <see cref"Discord.WebSocket.SocketGuildUser"/> object containing the user joining the guild.</param>
        /// <returns>Task</returns>
        private async Task OnuserUserJoined(SocketGuildUser socketGuildUser)
        {
          
            var joinlog = socketGuildUser.Guild.GetTextChannel(Global.PostTargets[PostTarget.JOIN_LOG]);
            string name = socketGuildUser.Username;
            if(IllegalUserName(name))
            {
                var modQueue = socketGuildUser.Guild.GetTextChannel(Global.PostTargets[PostTarget.USERNAME_QUEUE]);
                var builder = new EmbedBuilder();
                builder.Title = "User with illegal character joined!";
                builder.Description = Extensions.FormatUserNameDetailed(socketGuildUser);
                builder.Color = Color.DarkBlue;
                
                builder.Timestamp = DateTime.Now;
                
                builder.ThumbnailUrl = socketGuildUser.GetAvatarUrl();
                var message = await modQueue.SendMessageAsync(embed: builder.Build());
                await message.AddReactionAsync(Global.Emotes[Global.OnePlusEmote.OPEN_MODMAIL].GetAsEmote());
                Global.UserNameNotifications.Add(message.Id, socketGuildUser.Id);
            }
            await joinlog.SendMessageAsync(Extensions.FormatMentionDetailed(socketGuildUser) + " joined the guild");
        }

        /// <summary>
        /// Checks if the given string is in accordance with the given username regex (only the first character is checked)
        /// </summary>
        /// <param name="userName">The username as string which is checked</param>
        /// <returns>True in case the string matches with the username regex.</returns>
        private bool IllegalUserName(string userName)
        {
          return Global.IllegalUserNameRegex.Match(userName.ToLower()[0] + "").Success;
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

        private async Task OnUserJoinedRole(SocketGuildUser user)
        {
            using (var db = new Database())
            {
                var dbUser = db.Users.Where(us => us.Id == user.Id).Include(us => us.ExperienceRoleReference).FirstOrDefault();
                if(dbUser != null)
                {
                    if(dbUser.ExperienceRoleReference != null){
                        var role = user.Guild.GetRole(dbUser.ExperienceRoleReference.ExperienceRoleId);
                        await user.AddRoleAsync(role);
                    }
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

                var embed = new EmbedBuilder();
                embed.WithDescription($":bulb: Message from '{Extensions.FormatUserNameDetailed(author)}' edited in {channel.Mention}");
                embed.WithColor(Color.Blue);
                embed.WithThumbnailUrl(author.GetAvatarUrl());
                embed.WithTimestamp(DateTime.Now);
                embed.AddField("Original message:", before.Content);
                embed.AddField("New message", message.Content);
                embed.AddField("Jump link", Extensions.FormatLinkWithDisplay("Jump!", message.GetJumpUrl()));

                await channel.Guild.GetTextChannel(Global.PostTargets[PostTarget.EDIT_LOG]).SendMessageAsync(embed: embed.Build());
            }
        }

        /// <summary>
        /// Reports deleted messages to the 'DELETE_LOG' post target. If the message was in the starboard channel, it marks the post 
        /// as deleted for the starboard mechanism
        /// </summary>
        /// <param name="cacheable">Cacheable which was deleted</param>
        /// <param name="socketChannel">The channel in which the message was deleted</param>
        /// <returns>Task</returns>
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
            fields.Add(new EmbedFieldBuilder() { Name = ":x: Original message: ", Value = originalMessage });
            if(deletedMessage != null) 
            {
              fields.Add(new EmbedFieldBuilder() { Name = "Link", Value=Extensions.FormatLinkWithDisplay("Jump!", deletedMessage.GetJumpUrl())});
            }
           
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
                    Description = $":bulb: Message from '{Extensions.FormatUserNameDetailed(cacheable.Value.Author)}' removed in {channel.Mention}",
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

        /// <summary>
        /// Gets executed after a command has been completed. Reacts with either success reaction, a warn symbole accompanid with an error message or nothing, if the result is ignored.
        /// </summary>
        /// <param name="command">The <see cref="Discord.Commands.CommandInfo"> object which just finished executing</param>
        /// <param name="context">The <see cref="Discord.ICommandContext"> in which the command finished executing</param>
        /// <param name="result">The <see cref="Discord.Commands.IResult"> object containing the result of the command</param>
        /// <returns></returns>
        private static async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            switch(result)
            {
                case PreconditionResult conditionResult:
                    if (conditionResult.IsSuccess)
                    {
                        await context.Message.AddReactionAsync(StoredEmote.GetEmote(Global.OnePlusEmote.SUCCESS));
                    }
                    else
                    {
                        await context.Message.AddReactionAsync(StoredEmote.GetEmote(Global.OnePlusEmote.FAIL));
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
                        await context.Message.AddReactionAsync(StoredEmote.GetEmote(Global.OnePlusEmote.SUCCESS));
                    }
                    else
                    {
                        await context.Message.AddReactionAsync(StoredEmote.GetEmote(Global.OnePlusEmote.FAIL));
                        await context.Channel.SendMessageAsync(customResult.Reason);
                    }
                    break;

                default:
                 if (!string.IsNullOrEmpty(result?.ErrorReason))
                 {

                    if (result.ErrorReason == "Unknown command.")
                    return;

                    await context.Message.AddReactionAsync(StoredEmote.GetEmote(Global.OnePlusEmote.FAIL));                 

                    await context.Channel.SendMessageAsync(result.ErrorReason);
                    return;
                 }
                break;
            }
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

            var matches = Regex.Matches(message.Content, @"https?:\/\/(?:www\.)?oneplus\.(?:[a-z]{1,63})[^\s]*invite(?:\#([^\s]+)|.+\=([^\s\&]+))", RegexOptions.IgnoreCase);

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

        private static async Task ReportProfanity(SocketMessage message, ProfanityCheck usedProfanity)
        {
            using(var db = new Database())
            {
              if(db.Profanities.Where(p => p.MessageId == message.Id).Any())
              {
                return;
              }  
            }
            var guild = Global.Bot.GetGuild(Global.ServerID);
            var builder = new EmbedBuilder();
            builder.Title = "Profanity has been used!";
            builder.Color = Color.DarkBlue;
            
            builder.Timestamp = message.Timestamp;
            
            builder.ThumbnailUrl = message.Author.GetAvatarUrl();

            builder.AddField("User in question ", Extensions.FormatMentionDetailed(message.Author))
                .AddField(f => {
                    f.Name = "Location of the profane message";
                    f.Value = Extensions.FormatLinkWithDisplay(message.Channel.Name, message.GetJumpUrl());
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
                StoredEmote.GetEmote(Global.OnePlusEmote.OP_YES), 
                StoredEmote.GetEmote(Global.OnePlusEmote.OP_NO)
            });

            var profanity = new UsedProfanity();
            profanity.MessageId = message.Id;
            profanity.ReportMessageId = report.Id;
            profanity.UserId = message.Author.Id;
            profanity.Valid = false;
            profanity.ProfanityId = usedProfanity.ID;
            profanity.ChannelId = message.Channel.Id;
            using(var db = new Database())
            {
                var user = db.Users.Where(us => us.Id == message.Author.Id).FirstOrDefault();
                if(user == null)
                {
                    var newUser = new UserBuilder(message.Author.Id).Build();
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
            if(!exists)
            {
                var element = new List<ulong>();
                element.Add(message.Author.Id);
                Global.RuntimeExp.TryAdd(minute, element);
            }
            else
            {
                List<ulong> poster = new List<ulong>();
                Global.RuntimeExp.TryGetValue(minute, out poster);
                if(!poster.Contains(message.Author.Id))
                {
                    poster.Add(message.Author.Id);
                }
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Checks if the message content contains a link to message and if so (and the channel is in a guild) embeds the message in the context channel
        /// </summary>
        /// <param name="message">The <see cref="Discord.WebSocket.SocketMessage"> object to check for message links</param>
        /// <returns>Task</returns>
        private static async Task OnMessageReceivedEmbed(SocketMessage message)
        {
          if(message.Channel is SocketGuildChannel)
          {
            var messageContent = message.Content;
            var matches = messageRegex.Matches(messageContent);
            if(matches.Count() > 0)
            {
              var bot = Global.Bot;
              foreach(Match match in matches)
              {
                var serverId = Convert.ToUInt64(match.Groups[2].Value);
                var channelId = Convert.ToUInt64(match.Groups[3].Value);
                var messageId = Convert.ToUInt64(match.Groups[4].Value);
                var server = bot.GetGuild(serverId);
                if(server != null)
                {
                  var channel = server.GetTextChannel(channelId);
                  if(channel != null)
                  {
                    var messageToEmbed = await channel.GetMessageAsync(messageId);
                    if(messageToEmbed != null)
                    {
                      messageContent = messageContent.Replace(match.Groups[1].Value, "");
                      var embedBuilder = Extensions.GetMessageAsEmbed(messageToEmbed);
                      var fieldValue = message.Author.Mention + " from " + Extensions.GetMessageUrl(server.Id, messageToEmbed.Channel.Id, messageToEmbed.Id, server.GetTextChannel(messageToEmbed.Channel.Id).Name);
                      embedBuilder.AddField("Quoted by", fieldValue);
                      await message.Channel.SendMessageAsync(embed: embedBuilder.Build());
                      await Task.Delay(500);
                    }
                  }
                }
              }
              if(messageContent.Trim() == string.Empty)
              {
                await message.DeleteAsync();
              }
            }
           
          }
        }

        private static async Task OnMessageReceived(SocketMessage message)
        {
            if (ViolatesRule(message))
            {
                await message.DeleteAsync();
            }

            var channel = Extensions.GetChannelById(message.Channel.Id);
            if(channel != null)
            {
                if(!channel.ProfanityExempt())
                {
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

                
            if (message.Channel is SocketDMChannel)
            {
                var guild = Global.Bot.GetGuild(Global.ServerID);
                var guildUser = guild.GetUser(message.Author.Id);
                if (guildUser != null && guildUser.Roles.Where(ro => ro.Id == Global.Roles["staff"]).Any())
                {
                    var feedbackChannel = guild.GetTextChannel(Global.PostTargets[PostTarget.FEEDBACK]);
                    await feedbackChannel.SendMessageAsync("Feedback!" + Environment.NewLine + message.Content);
                }
                else
                {
                    var modmailThread = ModMailThreadForUserExists(message.Author);

                    if(modmailThread && message.Channel is IDMChannel)
                    {
                        await new ModMailManager().HandleModMailUserReply(message);
                    }
                    else if(message.Channel is IDMChannel)
                    {
                        await new ModMailManager().CreateModmailThread(message);
                    }
                }
            }

         
            var channelId = message.Channel.Id;

            if (channelId == Global.Channels[Channel.SETUPS])
            {
                await ValidateSetupsMessage(message);
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
