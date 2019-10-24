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

namespace OnePlusBot.Modules
{
    public class Administration : ModuleBase<SocketCommandContext>
    {
        [
            Command("banid", RunMode = RunMode.Async),
            Summary("Bans specified user."),
            RequireRole("staff")
        ]
        public async Task<RuntimeResult> OBanAsync(ulong name, [Remainder] string reason = null)
        {
            var modlog = Context.Guild.GetTextChannel(Global.Channels["modlog"]);
            await Context.Guild.AddBanAsync(name, 0, reason);

            MuteTimerManager.UnMuteUserCompletely(name);

            return CustomResult.FromSuccess();
        }
    

        [
            Command("ban", RunMode = RunMode.Async),
            Summary("Bans specified user."),
            RequireRole("staff"),
            RequireBotPermission(GuildPermission.BanMembers)
        ]
        public async Task<RuntimeResult> BanAsync(IGuildUser user, [Remainder] string reason = null)
        {
            if (user.IsBot)
                return CustomResult.FromError("You can't ban bots.");

            if (user.GuildPermissions.PrioritySpeaker)
                return CustomResult.FromError("You can't ban staff.");
            try
            {
                try 
                {
                    const string banMessage = "You were banned on r/OnePlus for the following reason: {0}\n" +
                                          "If you believe this to be a mistake, please send an appeal e-mail with all the details to oneplus.appeals@pm.me";
                    await user.SendMessageAsync(string.Format(banMessage, reason));
                } catch (HttpException ex){
                    Console.WriteLine("User disabled DMs, unable to send message about ban.");
                }
               
                await Context.Guild.AddBanAsync(user, 0, reason);

                MuteTimerManager.UnMuteUserCompletely(user.Id);

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
            RequireBotPermission(GuildPermission.KickMembers)
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


        [
            Command("mute", RunMode=RunMode.Async),
            Summary("Mutes a specified user for a set amount of time"),
            RequireRole("staff"),
            RequireBotPermission(GuildPermission.ManageRoles)
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
                const string muteMessage = "You were muted on r/OnePlus for the following reason: {0} until {1} {2}.";
                await user.SendMessageAsync(string.Format(muteMessage, reason, targetTime, TimeZoneInfo.Local));
            } 
            catch(HttpException ex)
            {
                Console.WriteLine("Seems like user disabled the DMs, cannot send message about the mute.");
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
                   .AddField("Muted until", $"{ targetTime:dd.MM.yyyy HH:mm}")
                   .AddField("Mute id", muteData.ID);
               
            await guild.GetTextChannel(Global.Channels["mutes"]).SendMessageAsync(embed: builder.Build());
            // in case the mute is shorter than the timer defined in Mutetimer.cs, we better just start the unmuting process directly
            if(targetTime <= DateTime.Now.AddMinutes(60))
            {
                var difference = targetTime - DateTime.Now;
                MuteTimerManager.UnmuteUserIn(user.Id, difference, muteData.ID);
            }
           
            return CustomResult.FromSuccess();
        }

        [
            Command("unmute", RunMode=RunMode.Async),
            Summary("Unmutes a specified user"),
            RequireRole("staff"),
            RequireBotPermission(GuildPermission.ManageRoles)
        ]
        public async Task<RuntimeResult> UnMuteUser(IGuildUser user)
        {
            return await MuteTimerManager.UnMuteUser(user.Id, ulong.MaxValue);
        }

        [
            Command("purge", RunMode = RunMode.Async),
            Summary("Deletes specified amount of messages."),
            RequireRole("staff"),
            // this is a workaround, because purge can break the logging functionality of modmail (it removes the messages whose ids are stored)
            // so the command is disabled in modmail, because it should not be used
            RequireModMailContext(true),
            RequireBotPermission(GuildPermission.ManageMessages)
        ]
        public async Task<RuntimeResult> PurgeAsync([Remainder] double delmsg)
        {
            if (delmsg > 100 || delmsg <= 0)
                return CustomResult.FromError("Use a number between 1-100");

            int delmsgInt = (int)delmsg;
            ulong oldmessage = Context.Message.Id;

            // Download all messages that the user asks for to delete
            var messages = await Context.Channel.GetMessagesAsync(oldmessage, Direction.Before, delmsgInt).FlattenAsync();
            await ((ITextChannel) Context.Channel).DeleteMessagesAsync(messages);

            // await Context.Message.DeleteAsync();


            return CustomResult.FromSuccess(); //This will generate an error, because we delete the message, TODO: Implement a workaround.
        }

        [
            Command("warn"),
            Summary("Warn someone."),
            RequireRole("staff"),
            RequireBotPermission(GuildPermission.Administrator)
        ]
        public async Task<RuntimeResult> WarnAsync(IGuildUser user, [Optional] [Remainder] string reason)
        {
            var warningsChannel = Context.Guild.GetTextChannel(Global.Channels["warnings"]);

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
            RequireBotPermission(GuildPermission.Administrator)
        ]
        public async Task<RuntimeResult> ClearwarnAsync(uint index)
        {
            var warningsChannel = Context.Guild.GetTextChannel(Global.Channels["warnings"]);
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
                                   $"*{warning.Date.ToString("dd/M/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture)}* \n" +
                                   $"{decayed} \n"
                                   ));
                }
                else
                {
                    var warned = Context.Guild.GetUser(warning.WarnedUserID);
                    var warnedSafe = Extensions.FormatMentionDetailed(warned);
                    var warnedBySafe = Extensions.FormatMentionDetailed(warnedBy);
                    embed.AddField(new EmbedFieldBuilder()
                        .WithName($"Case #{warning.ID} - {warning.Date.ToString("dd/M/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture)}")
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

                    await ReplyAsync($"You have {individualWarnings.Count()} active and {totalWarnings.Count()} total warnings.");

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
            RequireRole("staff")
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
    }
}
