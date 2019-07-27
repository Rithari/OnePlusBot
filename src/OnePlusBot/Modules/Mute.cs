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
        public async Task<RuntimeResult> MuteUser(IGuildUser user,params String[] arguments)
        {
            if (user.IsBot)
                return CustomResult.FromError("You can't mute bots.");

            if (user.GuildPermissions.PrioritySpeaker)
                return CustomResult.FromError("You can't mute staff.");
            
            if(arguments.Length < 1)
                return CustomResult.FromError("syntax ;mute <duration> [<reason>]");
            
            String durationStr = arguments[0];

            String reason;
            if(arguments.Length > 1)
            {
                String[] reasons = new String[arguments.Length -1];
                Array.Copy(arguments, 1, reasons, 0, arguments.Length - 1);
                reason = String.Join(" ", reasons);

            } 
            else 
            {
                reason = "No reason provided";
            }

            CaptureCollection captures =  Regex.Match(durationStr, @"(\d+[a-z]+)+").Groups[1].Captures;

            DateTime targetTime = DateTime.Now;
            // this basically means *one* of the values has been wrong, maybe negative or something like that
            Boolean validFormat = false;
            // this means, that one *valid* unit has been found, not necessarily a valid valid, this is needed for the case, in which there is
            // no valid value possible (e.g. 3y), this would cause the for loop to do nothing, but we are unaware of that
            foreach(Capture capture in captures)
            {
                // this is needed, because at one iteration of the loop, we cannot know whether or not, any case applies
                Boolean timeUnitApplied = false;
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
                return CustomResult.FromError("Invalid format, it needs to be positive, and combinations of " + String.Join(", ", timeFormats));
            }

            var author = Context.Message.Author;

            var guild = Context.Guild;
            var mutedRoleName = "muted";
            if(!Global.Roles.ContainsKey(mutedRoleName))
            {
                return CustomResult.FromError("Mute role has not been configured.");
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
               
            var embed = builder.Build();
            await guild.GetTextChannel(Global.Channels["modlog"]).SendMessageAsync(embed: builder.Build());
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
