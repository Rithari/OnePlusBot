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

        [Command("mute", RunMode=RunMode.Async),
        Summary("Mutes a specified user for a set amount of time"),
        RequireBotPermission(GuildPermission.KickMembers),
        RequireUserPermission(GuildPermission.PrioritySpeaker),
        RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task<RuntimeResult> MuteUser(IGuildUser user,params String[] arguments)
        {
             if (user.IsBot)
                return CustomResult.FromError("You can't mute bots.");

            if (user.GuildPermissions.PrioritySpeaker)
                return CustomResult.FromError("You can't mute staff.");
            
            if(arguments.Length != 2)
                return CustomResult.FromError("syntax ;mute <duration> <reason>.");
            
            String durationStr = arguments[0];
            String reason = arguments[1];
            char usedFormat = 'n';
            int duration = 0;
            bool validDuration = false;

            Regex rx = new Regex(@"(\d+[a-z]+)+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matches = rx.Matches(durationStr);

            foreach (Match match in matches)
            {
                  GroupCollection groups = match.Groups;
                  Console.WriteLine(groups["word"].Value);
            }


            foreach(char format in timeFormats){
                if(durationStr.Contains(format)){
                    var durationSplit = durationStr.Split(format);
                    var isNumeric = int.TryParse(durationSplit[0], out int n);
                    if(durationSplit.Length == 2 && durationSplit[1] == "" && isNumeric && n > 0){
                        duration = n;
                        validDuration = true;
                        usedFormat = format;
                    }
                    break;
                }
            }
            if(!validDuration)
                return CustomResult.FromError("Invalid time format and or number.");
            
            DateTime startTime = DateTime.Now;
            DateTime targetTime;
            switch(usedFormat){
                default:
                case 'm': targetTime = startTime.AddMinutes(duration); break;
                case 'h': targetTime = startTime.AddHours(duration); break;
                case 'd': targetTime = startTime.AddDays(duration); break;
                case 'w': targetTime = startTime.AddDays(duration * 7); break;
                case 's': targetTime = startTime.AddSeconds(duration); break;
            }
            

            var author = Context.Message.Author;

            var muteData = new Mute
            {
                MuteDate = startTime,
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

            var guild = Context.Guild;
            var mutedRoleName = "muted";
            if(!Global.Roles.ContainsKey(mutedRoleName)){
                return CustomResult.FromError("Mute role has not been configured");
            }

            await Extensions.MuteUser(user);
            if(targetTime <= DateTime.Now.AddHours(24)){
                var difference = targetTime - DateTime.Now;
                MuteTimerManager.UnmuteUserIn(user.Id, difference, muteData.ID);
            }
           
            return CustomResult.FromSuccess();
        }

         [Command("unmute", RunMode=RunMode.Async),
        Summary("Unmutes a specified user"),
        RequireBotPermission(GuildPermission.KickMembers),
        RequireUserPermission(GuildPermission.PrioritySpeaker),
        RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task<RuntimeResult> UnMuteUser(IGuildUser user)
        {
            await MuteTimerManager.UnMuteUser(user.Id, ulong.MaxValue);
            return CustomResult.FromSuccess();
        }

    }
}
