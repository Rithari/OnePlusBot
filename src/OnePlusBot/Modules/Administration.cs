using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Helpers;
using OnePlusBot.Data.Models;
using System.Runtime.InteropServices;
using System.Linq;
using Discord.WebSocket;
using Discord.Net;
using System.Collections.Generic;
using System.Globalization;
using OnePlusBot.Base.Errors;

namespace OnePlusBot.Modules
{
    public class Administration : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// Bans the user by the given id with an optional reason.
        /// </summary>
        /// <param name="name">Id of the user to be banned</param>
        /// <param name="reason">Reason for the ban (optional)</param>
        /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
        [
            Command("banid", RunMode = RunMode.Async),
            Summary("Bans specified user."),
            RequireRole("staff"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> OBanAsync(ulong name, [Remainder] string reason = null)
        {
            var banLogChannel = Context.Guild.GetTextChannel(Global.PostTargets[PostTarget.BAN_LOG]);
            await Context.Guild.AddBanAsync(name, 0, reason);

            MuteTimerManager.UnMuteUserCompletely(name);


            if(reason == null)
            {
                reason = "No reason provided.";
            }

            var modlog = Context.Guild.GetTextChannel(Global.Channels["banlog"]);
            var banMessage = new EmbedBuilder()
            .WithColor(9896005)
            .WithTitle("⛔️ Banned User")
            .AddField("UserId", name, true)
            .AddField("By", Extensions.FormatUserName(Context.User) , true)
            .AddField("Reason", reason)
            .AddField("Link", Extensions.FormatLinkWithDisplay("Jump!", Context.Message.GetJumpUrl()));

            await banLogChannel.SendMessageAsync(embed: banMessage.Build());

            return CustomResult.FromSuccess();
        }
    

        /// <summary>
        /// Bans the given user with the given reason (default text if none provided). Also sends a direct message to the user containing the reason and the mail used for appeals
        /// </summary>
        /// <param name="user"><see ref="Discord.IGuildUser"> object of the user to be banned</param>
        /// <param name="reason">Optional reason for the ban</param>
        /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
        [
            Command("ban", RunMode = RunMode.Async),
            Summary("Bans specified user."),
            RequireRole("staff"),
            RequireBotPermission(GuildPermission.BanMembers),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> BanAsync(IGuildUser user, [Remainder] string reason = null)
        {
            if (user.IsBot)
            {
                return CustomResult.FromError("You can't ban bots.");
            }


            if (user.GuildPermissions.PrioritySpeaker)
            {
                return CustomResult.FromError("You can't ban staff.");
            }
                
            
            if(reason == null)
            {
                reason = "No reason provided.";
            }
            try
            {
                try 
                {
                    const string banDMMessage = "You were banned on r/OnePlus for the following reason: {0}\n" +
                                          "If you believe this to be a mistake, please send an appeal e-mail with all the details to oneplus.appeals@pm.me";
                    await user.SendMessageAsync(string.Format(banDMMessage, reason));
                } catch (HttpException){
                    Console.WriteLine("User disabled DMs, unable to send message about ban.");
                }
               
                await Context.Guild.AddBanAsync(user, 0, reason);

                MuteTimerManager.UnMuteUserCompletely(user.Id);

                var banLogChannel = Context.Guild.GetTextChannel(Global.PostTargets[PostTarget.BAN_LOG]);
                var banlog = new EmbedBuilder()
                .WithColor(9896005)
                .WithTitle("⛔️ Banned User")
                .AddField("User", Extensions.FormatUserNameDetailed(user), true)
                .AddField("By", Extensions.FormatUserName(Context.User), true)
                .AddField("Reason", reason)
                .AddField("Link", Extensions.FormatLinkWithDisplay("Jump!", Context.Message.GetJumpUrl()));

                await banLogChannel.SendMessageAsync(embed: banlog.Build());

                return CustomResult.FromSuccess();

            }
            catch (Exception ex)
            {   
                //  may not be needed
                // await Context.Guild.AddBanAsync(user, 0, reason);
                return CustomResult.FromError(ex.Message);
            }
        }

        [
            Command("kick", RunMode = RunMode.Async),
            Summary("Kicks specified user."),
            RequireRole("staff"),
            RequireBotPermission(GuildPermission.KickMembers),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> KickAsync(IGuildUser user, [Remainder] string reason = null)
        {
            if (user.IsBot)
                return CustomResult.FromError("You can't kick bots.");


            if (user.GuildPermissions.PrioritySpeaker)
                return CustomResult.FromError("You can't kick staff.");

            await user.KickAsync(reason);
            MuteTimerManager.UnMuteUserCompletely(user.Id);
            return CustomResult.FromSuccess();
        }


        /// <summary>
        /// Mutes the user for a certain timeperiod (effectively gives the user the two (!) configured roles denying the user the ability to send messages)
        /// </summary>
        /// <param name="user">The <see cref="Discord.IGuildUser"> user to mute</param>
        /// <param name="arguments">Arguments for muting, including: the duration and the reason</param>
        /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
        [
            Command("mute", RunMode=RunMode.Async),
            Summary("Mutes a specified user for a set amount of time"),
            RequireRole("staff"),
            RequireBotPermission(GuildPermission.ManageRoles),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> MuteUser(IGuildUser user,params string[] arguments)
        {
            if (user.IsBot)
                return CustomResult.FromError("You can't mute bots.");

            if (user.GuildPermissions.PrioritySpeaker)
                return CustomResult.FromError("You can't mute staff.");
            
            if(arguments.Length < 1)
                return CustomResult.FromError("The correct usage is `;mute <duration> <reason>`");

            string durationStr = arguments[0];
            TimeSpan span = Extensions.GetTimeSpanFromString(durationStr);
            var now = DateTime.Now;
            DateTime targetTime = now.Add(span);

            string reason;
            if(arguments.Length > 1)
            {
                string[] reasons = new string[arguments.Length -1];
                Array.Copy(arguments, 1, reasons, 0, arguments.Length - 1);
                reason = string.Join(" ", reasons);
            } 
            else 
            {
                return CustomResult.FromError("You need to provide a reason.");
            }

            var author = Context.Message.Author;

            var guild = Context.Guild;
            var mutedRoleName = "textmuted";
            if(!Global.Roles.ContainsKey(mutedRoleName))
            {
                return CustomResult.FromError("Text mute role has not been configured. Check your DB entry!");
            }

            mutedRoleName = "voicemuted";
            if(!Global.Roles.ContainsKey(mutedRoleName))
            {
                return CustomResult.FromError("Voice mute role has not been configured. Check your DB entry!");
            }

            await Extensions.MuteUser(user);
            await user.ModifyAsync(x => x.Channel = null);

            try
            {
              const string muteMessage = "You were muted on r/OnePlus for the following reason: {0} until {1}.";
              await user.SendMessageAsync(string.Format(muteMessage, reason, Extensions.FormatDateTime(targetTime)));
            } 
            catch(HttpException)
            {
              await Context.Channel.SendMessageAsync("Seems like user disabled the DMs, cannot send message about the mute.");
            }    

            var muteData = new Mute
            {
                MuteDate = now,
                UnmuteDate = targetTime,
                MutedUser = user.Username + '#' + user.Discriminator,
                MutedUserID = user.Id,
                MutedByID = author.Id,
                MutedBy = author.Username + '#' + author.Discriminator,
                Reason = reason,
                MuteEnded = false
            };

            using (var db = new Database())
            {
                db.Mutes.Add(muteData);
                db.SaveChanges();
            }        

            var builder = new EmbedBuilder();
            builder.Title = "A user has been muted!";
            builder.Color = Color.Red;
            
            builder.Timestamp = Context.Message.Timestamp;
            
            builder.ThumbnailUrl = user.GetAvatarUrl();
            
            const string discordUrl = "https://discordapp.com/channels/{0}/{1}/{2}";
            builder.AddField("Muted User", Extensions.FormatUserName(user))
                   .AddField("Muted by", Extensions.FormatUserName(author))
                   .AddField("Location of the mute",
                        $"[#{Context.Message.Channel.Name}]({string.Format(discordUrl, Context.Guild.Id, Context.Channel.Id, Context.Message.Id)})")
                   .AddField("Reason", reason ?? "No reason was provided.")
                   .AddField("Muted until", $"{ Extensions.FormatDateTime(targetTime)}")
                   .AddField("Mute id", muteData.ID);
               
            await guild.GetTextChannel(Global.PostTargets[PostTarget.MUTE_LOG]).SendMessageAsync(embed: builder.Build());
            // in case the mute is shorter than the timer defined in Mutetimer.cs, we better just start the unmuting process directly
            if(targetTime <= DateTime.Now.AddMinutes(60))
            {
                var difference = targetTime - DateTime.Now;
                MuteTimerManager.UnmuteUserIn(user.Id, difference, muteData.ID);
                using (var db = new Database())
                {
                    db.Mutes.Where(m => m.ID == muteData.ID).First().UnmuteScheduled = true;
                    db.SaveChanges();
                }    
            }
           
            return CustomResult.FromSuccess();
        }

        [
            Command("unmute", RunMode=RunMode.Async),
            Summary("Unmutes a specified user"),
            RequireRole("staff"),
            RequireBotPermission(GuildPermission.ManageRoles),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> UnMuteUser(IGuildUser user)
        {
            return await MuteTimerManager.UnMuteUser(user.Id, ulong.MaxValue);
        }

        [
            Command("purge", RunMode = RunMode.Async),
            Summary("Deletes specified amount of messages."),
            RequireRole("staff"),
            RequireBotPermission(GuildPermission.ManageMessages),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> PurgeAsync([Remainder] double delmsg)
        {
            if (delmsg > 100 || delmsg <= 0)
                return CustomResult.FromError("Use a number between 1-100");

            int delmsgInt = (int)delmsg;
            ulong oldmessage = Context.Message.Id;

            var isInModmailContext = Global.ModMailThreads.Exists(ch => ch.ChannelId == Context.Channel.Id);

            // Download all messages that the user asks for to delete
            var messages = await Context.Channel.GetMessagesAsync(oldmessage, Direction.Before, delmsgInt).FlattenAsync();

            var saveMessagesToDelete = new List<IMessage>();
            if(isInModmailContext){
                var manager = new ModMailManager();
                foreach(var message in messages){
                   var messageCanBeDeleted = await manager.DeleteMessageInThread(Context.Channel.Id, message.Id, false);
                   if(messageCanBeDeleted){
                       saveMessagesToDelete.Add(message);
                   }
                }
            }
            else
            {
                saveMessagesToDelete.AddRange(messages);
            }
            await ((ITextChannel) Context.Channel).DeleteMessagesAsync(saveMessagesToDelete);

            await Context.Message.DeleteAsync();


            return CustomResult.FromIgnored(); 
        }

        [
            Command("warn"),
            Summary("Warn someone."),
            RequireRole("staff"),
            RequireBotPermission(GuildPermission.Administrator),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> WarnAsync(IGuildUser user, [Optional] [Remainder] string reason)
        {
            var warningsChannel = Context.Guild.GetTextChannel(Global.PostTargets[PostTarget.WARN_LOG]);

            var monitor = Context.Message.Author;

            var entry = new WarnEntry
            {
                WarnedUser = user.Username + '#' + user.Discriminator,
                WarnedUserID = user.Id,
                WarnedBy = monitor.Username + '#' + monitor.Discriminator,
                WarnedByID = monitor.Id,
                Reason = reason,
                Date = Context.Message.Timestamp.DateTime,
            };

            using (var db = new Database())
            {
                db.Warnings.Add(entry);
                db.SaveChanges();
            }

            var builder = new EmbedBuilder();
            builder.Title = "...a new warning has emerged from outer space!";
            builder.Color = new Color(0x3E518);
            
            builder.Timestamp = Context.Message.Timestamp;
            
            builder.WithFooter(footer =>
            {
                footer
                    .WithText("Case #" + entry.ID)
                    .WithIconUrl("https://a.kyot.me/0WPy.png");
            });
            
            builder.ThumbnailUrl = user.GetAvatarUrl();
            
            builder.WithAuthor(author =>
            {
                author
                    .WithName("Woah...")
                    .WithIconUrl("https://a.kyot.me/cno0.png");
            });

            const string discordUrl = "https://discordapp.com/channels/{0}/{1}/{2}";
            builder.AddField("Warned User", Extensions.FormatMentionDetailed(user))
                .AddField("Warned by", Extensions.FormatMentionDetailed(monitor))
                .AddField(
                    "Location of the incident",
                    $"[#{Context.Message.Channel.Name}]({string.Format(discordUrl, Context.Guild.Id, Context.Channel.Id, Context.Message.Id)})")
                .AddField("Reason", reason ?? "No reason was provided.");
               
            var embed = builder.Build();
            await warningsChannel.SendMessageAsync(null,embed: embed).ConfigureAwait(false);

            return CustomResult.FromSuccess();
        }

        [
            Command("clearwarn"),
            Summary("Clear warnings."),
            RequireRole("staff"),
            RequireBotPermission(GuildPermission.Administrator),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ClearwarnAsync(uint index)
        {
            var monitor = Context.Message.Author;

            using (var db = new Database())
            {
                IQueryable<WarnEntry> warnings = db.Warnings;
                var selection = warnings.First(x => x.ID == index);
                db.Warnings.Remove(selection);
               await db.SaveChangesAsync();
            }

           return CustomResult.FromSuccess();
        }

        private const int TakeAmount = 6;
        
        private IUserMessage _message;
        private IGuildUser _user;
        private int _total;
        private int _index;
        
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> user, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot)
                return;
            if (reaction.MessageId != _message.Id)
                return;
            if (reaction.UserId != Global.CommandExecutorId)
                return;

            switch (reaction.Emote.Name)
            {
                case "⬅":
                    await reaction.Message.Value.DeleteAsync();
                    _index--;
                    break;
                
                case "➡":
                    await reaction.Message.Value.DeleteAsync();
                    _index++;
                    break;
                case "🗑":
                    await reaction.Message.Value.DeleteAsync();
                    return;
            }
            
            var skip = TakeAmount * _index;
            using (var db = new Database())
            {
                IQueryable<WarnEntry> warnings = db.Warnings;
                if (_user != null)
                    warnings = warnings.Where(x => x.WarnedUserID == _user.Id);
                warnings = warnings.Skip(skip).Take(TakeAmount);
                _message = await CreateWarnList(warnings.ToArray());
            }
        }

        /// <summary>
        /// Creates the and posts the message containing the given warnings
        /// </summary>
        /// <param name="warns">The warnings to display</param>
        /// <returns>The posted message in the current thread</returns>
        private async Task<IUserMessage> CreateWarnList(IEnumerable<WarnEntry> warns)
        {
            var embed = new EmbedBuilder();
            embed.WithColor(9896005);
            
            if (_user != null)
                embed.Title = $"Listing warnings of user {_user.Username}.";
            else
                embed.Title = $"There are {_total} warnings in total.";
            
            var counter = _index * TakeAmount;
            foreach (var warning in warns)
            {
                ++counter;
                var warnedBy = Context.Guild.GetUser(warning.WarnedByID);

                var decayed = warning.Decayed ? "" : "**Active**";
                if (_user != null)
                {
                    var warnedByUserSafe = Extensions.FormatMentionDetailed(warnedBy);
                    embed.AddField(new EmbedFieldBuilder()
                        .WithName("Warning #" + counter + " (" + warning.ID + ")")
                        .WithValue($"**Reason**: {warning.Reason}\n" +
                                   $"**Warned by**: {warnedByUserSafe}\n" + 
                                   $"*{Extensions.FormatDateTime(warning.Date)}* \n" +
                                   $"{decayed} \n"
                                   ));
                }
                else
                {
                    var warned = Context.Guild.GetUser(warning.WarnedUserID);
                    var warnedSafe = Extensions.FormatMentionDetailed(warned);
                    var warnedBySafe = Extensions.FormatMentionDetailed(warnedBy);
                    embed.AddField(new EmbedFieldBuilder()
                        .WithName($"Case #{warning.ID} - {Extensions.FormatDateTime(warning.Date)}")
                        .WithValue($"**Warned user**: {warnedSafe}\n" +
                                   $"**Reason**: {warning.Reason} \n" +
                                   $"**Warned by**: {warnedBySafe} \n" +
                                   $"{decayed}"
                                   ))
                        .WithFooter("*For more detailed info, consult the individual lists.*");
                }
            }

            var msg = await Context.Channel.EmbedAsync(embed);

            if (_index > 0)
                await msg.AddReactionAsync(new Emoji("⬅"));
            if (_index * TakeAmount + TakeAmount < _total)
                await msg.AddReactionAsync(new Emoji("➡"));
            await msg.AddReactionAsync(new Emoji("🗑"));

            return msg;
        }

        [
            Command("warnings"),
            Summary("Gets all warnings of given user"),
            CommandDisabledCheck
        ]
        public async Task GetWarnings([Optional] IGuildUser user)
        {
            var requestee = Context.User as SocketGuildUser;
            _user = user;
            Global.CommandExecutorId = Context.User.Id;
            
            using (var db = new Database())
            {
                IQueryable<WarnEntry> warnings = db.Warnings;
                IQueryable<WarnEntry> individualWarnings;
                
                if (user != null)
                    warnings = warnings.Where(x => x.WarnedUserID == user.Id);

                _total = warnings.Count();

                if (!requestee.Roles.Any(x => x.Name == "Staff"))
                {
                    individualWarnings = db.Warnings.Where(x => x.WarnedUserID == requestee.Id && !x.Decayed);
                    var totalWarnings = db.Warnings.Where(x => x.WarnedUserID == requestee.Id);
                    var builder = new EmbedBuilder();
                    builder.WithAuthor(new EmbedAuthorBuilder().WithIconUrl(requestee.GetAvatarUrl()).WithName(requestee.Username + '#' + requestee.Discriminator));
                    builder.WithDescription($"{requestee.Username + '#' + requestee.Discriminator} has {individualWarnings.Count()} active out of {totalWarnings.Count()} total warnings.");
                    await ReplyAsync(embed: builder.Build());

                    return;
                }

                if (_total == 0)
                {
                    await ReplyAsync("The specified user has no warnings.");
                    return;
                }
                
                warnings = warnings.Take(TakeAmount);

                _message = await CreateWarnList(warnings.ToArray());
                
                Context.Client.ReactionAdded += OnReactionAdded;
                _ = Task.Run(async () =>
                {
                    await Task.Delay(120_000);
                    Context.Client.ReactionAdded -= OnReactionAdded;
                    await _message.DeleteAsync();
                });
            }
        }

        [
            Command("reloaddb"),
            Summary("Reloades the cached info from db"),
            RequireRole("staff")
        ]
        public async Task<RuntimeResult> ReloadDB()
        {
            await Task.Delay(500);
            Global.LoadGlobal();
            return CustomResult.FromSuccess();
        }

        [
            Command("setstars"),
            Summary("sets the amount required to appear on the starboard"),
            RequireRole("staff"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> SetStars(string input)
        {
            ulong amount = 0;
            await Task.Delay(25);
            if(ulong.TryParse(input, out amount) && amount > 0){
                using (var db = new Database()){
                    var point = db.PersistentData.First(entry => entry.Name == "starboard_stars");
                    point.Value = amount;
                    db.SaveChanges();
                }
                Global.StarboardStars = amount;
                return CustomResult.FromSuccess();
            } 
            else
            {
                return CustomResult.FromError("whole numbers > 0 only");
            }
           
        }

        [
            Command("profanities"),
            Summary("Shows the actual and false profanities of a user"),
            RequireRole("staff"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ShowProfanities(IGuildUser user)
        {
            using(var db = new Database()){
                var allProfanities = db.Profanities.Where(pro => pro.UserId == user.Id);
                var actualProfanities = allProfanities.Where(pro => pro.Valid == true).Count();
                var falseProfanities = allProfanities.Where(pro =>pro.Valid == false).Count();
                var builder = new EmbedBuilder();
                builder.AddField(f => {
                    f.Name = "Actual";
                    f.Value = actualProfanities;
                    f.IsInline = true;
                });

                 builder.AddField(f => {
                    f.Name = "False positives";
                    f.Value = falseProfanities;
                    f.IsInline = true;
                });

                builder.WithAuthor(new EmbedAuthorBuilder().WithIconUrl(user.GetAvatarUrl()).WithName(Extensions.FormatUserName(user)));
                await Context.Channel.SendMessageAsync(embed: builder.Build());
                return CustomResult.FromSuccess();
            }
        }

        [
            Command("updateLevels", RunMode=RunMode.Async),
            Summary("Re-evaluates the experience, levels and assigns the roles to the users (takes a long time, use with care)"),
            RequireRole(new string[]{"admin", "founder"}),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> UpdateLevels([Optional] IGuildUser user)
        {
            if(user == null)
            {
                await Context.Channel.SendMessageAsync("DO NOT execute commands changing the role experience configuration while this is processing. Especially do not start another one, while one is running.");
                var message = await Context.Channel.SendMessageAsync("Processing");
                new ExpManager().UpdateLevelsOfMembers(message);
            }
            else
            {
                await new ExpManager().UpdateLevelOf(user);
            }
          
            return CustomResult.FromSuccess();
        }

        [
            Command("roleLevel"),
            Summary("Sets the level at which a role is given. If no parameters, shows the current role configuration"),
            RequireRole(new string[]{"admin", "founder"}),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> SetRoleToLevel([Optional] uint level, [Optional] ulong roleId)
        {
            if(level == 0 && roleId == 0)
            {
                await new ExpManager().ShowLevelconfiguration(Context.Channel);
            } 
            else
            {
                new ExpManager().SetRoleToLevel(level, roleId);
            }
           
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }

        [
            Command("disableXpGain"),
            Summary("Enables/disables xp gain for a user"),
            RequireRole(new string[]{"admin", "founder"}),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> SetExpGainEnabled(IGuildUser user, bool newValue)
        {
             new ExpManager().SetXPDisabledTo(user, newValue);
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }


        /// <summary>
        /// Creates the post in info responsible for managing the roles
        /// </summary>
        /// <returns>Task</returns>
        [
            Command("setupInfoPost"),
            Summary("Sets up the info post to let user self assign the roles"),
            RequireRole(new string[]{"admin", "founder"})
        ]
        public async Task<RuntimeResult> SetupInfoPost()
        {
            await new SelfAssignabeRolesManager().SetupInfoPost();
            return CustomResult.FromSuccess();
        }
        /// <summary>
        /// Sets the flag of the command identified by the given name in the channel group identified by the given group name to the given value
        /// </summary>
        /// <exception cref="OnePlusBot.Base.Errors.NotFoundException">In case no channel group or command with that name is found</exception>
        /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
        [
            Command("disableCommand"),
            Summary("Disables command in a specified channel group"),
            RequireRole(new string[]{"admin", "founder"})
        ]
        public async Task<RuntimeResult> DisableCommandInGroup(string commandName, string channelGroupName, bool newValue)
        {
            using(var db = new Database())
            {
              var commandInChannelGroup = db.CommandInChannelGroups.Where(co => co.ChannelGroupReference.Name == channelGroupName && co.CommandReference.Name == commandName);
              if(commandInChannelGroup.Any())
              {
                commandInChannelGroup.First().Disabled = newValue;
              }
              else 
              {
                var command = db.Commands.Where(co => co.Name == commandName);
                if(command.Any())
                {
                  var channelGroup = db.ChannelGroups.Where(chgrp => chgrp.Name == channelGroupName);
                  if(channelGroup.Any())
                  {
                    var newCommandInChannelGroup = new CommandInChannelGroup();
                    newCommandInChannelGroup.ChannelGroupId = channelGroup.First().Id;
                    newCommandInChannelGroup.CommandID = command.First().ID; 
                    newCommandInChannelGroup.Disabled = newValue;
                    db.CommandInChannelGroups.Add(newCommandInChannelGroup);
                  } 
                  else 
                  {
                    throw new NotFoundException("Channel group not found");
                  }
                }
                else 
                {
                  throw new NotFoundException("Command not found");
                }
                
              }
              db.SaveChanges();
            }
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }

        /// <summary>
        /// Changes the nickname of a user on the server (newNickname is optional, if not provided, will reset the nickname)
        /// </summary>
        /// <param name="user">The <see cref="Discord.IGuildUser"> object to change the nickname for</param>
        /// <param name="newNickname">The new nickname, optional, if not provided, it will reset the nickname</param>
        /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
        [
            Command("setNickname"),
            Summary("Changes the nickname of a given user, resets if empty"),
            RequireRole("staff"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> SetNicknameTo(IGuildUser user, [Optional]string newNickname)
        {
          await user.ModifyAsync((user) => user.Nickname = newNickname);
          return CustomResult.FromSuccess();
        }

        /// <summary>
        /// Changes the slow mode of the current channel to the given interval, 'off' for disabling slowmode
        /// </summary>
        /// <param name="slowModeConfig">Time format with the format 'd', 's', 'm', 'h', 'w'</param>
        /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
        [
            Command("slowmode"),
            Summary("Changes the slowmode configuration of the current channel, 'off' to turn off slowmode"),
            RequireRole("staff"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> SetSlowModeTo(string slowModeConfig)
        {
          var channelObj = Context.Guild.GetTextChannel(Context.Channel.Id);
          if(slowModeConfig == "off")
          {
            await channelObj.ModifyAsync(pro => pro.SlowModeInterval = 0);
          }
          else
          {
            var span = Extensions.GetTimeSpanFromString(slowModeConfig);
            if(span > TimeSpan.FromHours(6))
            {
              return CustomResult.FromError("Only values between 1 second and 6 hours allowed.");
            }
            await channelObj.ModifyAsync(pro => pro.SlowModeInterval = (int) span.TotalSeconds);
          }
          return CustomResult.FromSuccess();
        }

        
    }
}
