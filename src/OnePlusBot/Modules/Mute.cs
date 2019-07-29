using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System;
using OnePlusBot.Helpers;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using System.Text.RegularExpressions;

namespace OnePlusBot.Modules
{
    public class MuteModule : ModuleBase<SocketCommandContext>
    {

        private char[] timeFormats = {'m', 'h', 'd', 'w', 's'};

        [
            Command("mute", RunMode=RunMode.Async),
            Summary("Mutes a specified user for a set amount of time"),
            RequireBotPermission(GuildPermission.ManageRoles),
            RequireUserPermission(GuildPermission.PrioritySpeaker),
            RequireUserPermission(GuildPermission.ManageNicknames)
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

            CaptureCollection captures =  Regex.Match(durationStr, @"(\d+[a-z]+)+").Groups[1].Captures;

            DateTime targetTime = DateTime.Now;
            // this basically means *one* of the values has been wrong, maybe negative or something like that
            bool validFormat = false;
            foreach(Capture capture in captures)
            {
                // this means, that one *valid* unit has been found, not necessarily a valid valid, this is needed for the case, in which there is
                // no valid value possible (e.g. 3y), this would cause the for loop to do nothing, but we are unaware of that
                bool timeUnitApplied = false;
                foreach(char format in timeFormats)
                {
                    var captureValue = capture.Value;
                    // this check is needed in order that our durationSplit check makes sense
                    // if the format is not present, the split would return a wrong value, and the check would be wrong
                    if(captureValue.Contains(format))
                    {
                        timeUnitApplied = true;
                        var durationSplit = captureValue.Split(format);
                        var isNumeric = int.TryParse(durationSplit[0], out int n);
                        if(durationSplit.Length == 2 && durationSplit[1] == "" && isNumeric && n > 0)
                        {
                            switch(format)
                            {
                                case 'm': targetTime = targetTime.AddMinutes(n); break;
                                case 'h': targetTime = targetTime.AddHours(n); break;
                                case 'd': targetTime = targetTime.AddDays(n); break;
                                case 'w': targetTime = targetTime.AddDays(n * 7); break;
                                case 's': targetTime = targetTime.AddSeconds(n); break;
                                default: validFormat = false; goto AfterLoop; 
                            }
                            validFormat = true;
                        }
                        else 
                        {
                            validFormat = false;
                            goto AfterLoop;
                        }
                    }
                }
                if(!timeUnitApplied)
                {
                    validFormat = false;
                    break;
                }
            }

            AfterLoop:
            if(!validFormat)
            {
                return CustomResult.FromError("Invalid format, it needs to be positive, and combinations of " + string.Join(", ", timeFormats));
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

            var muteData = new Mute
            {
                MuteDate = DateTime.Now,
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
            RequireBotPermission(GuildPermission.ManageRoles),
            RequireUserPermission(GuildPermission.PrioritySpeaker),
            RequireUserPermission(GuildPermission.ManageNicknames)
        ]
        public async Task<RuntimeResult> UnMuteUser(IGuildUser user)
        {
            await MuteTimerManager.UnMuteUser(user.Id, ulong.MaxValue);
            return CustomResult.FromSuccess();
        }

    }
}
