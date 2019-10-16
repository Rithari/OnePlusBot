using System.Text;
using System.Linq;
using System;
using System.Threading.Tasks;
using Discord;
using OnePlusBot.Base;
using OnePlusBot.Data.Models;
using System.Text.RegularExpressions;

namespace OnePlusBot.Helpers
{
    public static class Extensions
    {
        public static Task<IUserMessage> EmbedAsync(this IMessageChannel ch, EmbedBuilder embed, string msg = "")
        {
            return ch.SendMessageAsync(msg,
                embed: embed.Build(),
                options: new RequestOptions
                {
                    RetryMode = RetryMode.AlwaysRetry
                });
        }

        public static Uri RealAvatarUrl(this IUser usr, ushort size = 0)
        {
            if (usr.AvatarId == null)
                return null;
            
            return new Uri(usr.GetAvatarUrl(ImageFormat.Auto, size));
        }
        public static async Task<CustomResult> MuteUser(IGuildUser user)
        {
            // will be replaced with a better handling in the future
            if(!Global.Roles.ContainsKey("voicemuted") || !Global.Roles.ContainsKey("textmuted"))
            {
                return CustomResult.FromError("Configure the voicemuted and textmuted roles correctly. Check your Db!");
            }
            var muteRole = user.Guild.GetRole(Global.Roles["voicemuted"]);
            await user.AddRoleAsync(muteRole);

            muteRole = user.Guild.GetRole(Global.Roles["textmuted"]);
            await user.AddRoleAsync(muteRole);
            return CustomResult.FromSuccess();
        }


        public static async Task<CustomResult> UnMuteUser(IGuildUser user)
        {
            // will be replaced with a better handling in the future
            if(!Global.Roles.ContainsKey("voicemuted") || !Global.Roles.ContainsKey("textmuted"))
            {
                return CustomResult.FromError("Configure the voicemuted and textmuted roles correctly. Check your Db!");
            }
            var muteRole = user.Guild.GetRole(Global.Roles["voicemuted"]);
            await user.RemoveRoleAsync(muteRole);

            muteRole = user.Guild.GetRole(Global.Roles["textmuted"]);
            await user.RemoveRoleAsync(muteRole);
            return CustomResult.FromSuccess();
        }

        public static String FormatUserName(IUser user)
        {
            return user?.Username + '#' + user?.Discriminator;
        }

        public static String FormatUserNameDetailed(IUser user)
        {
            return FormatUserName(user) + " (" + user?.Id +  ") ";
        }

        public static String FormatMentionDetailed(IUser user){
            return user?.Mention + " " + FormatUserNameDetailed(user);
        }

        public static Channel GetChannelById(ulong channelId){
            return Global.FullChannels.Where(chan => chan.ChannelID == channelId).DefaultIfEmpty(null).First();
        }

        public static EmbedBuilder FaqCommandEntryToBuilder(FAQCommandChannelEntry entry) {
            var faqEmbedEntry = new EmbedBuilder();
            faqEmbedEntry.WithColor(new Color(entry.HexColor));
            faqEmbedEntry.WithDescription(entry.Text);
            if(entry.ImageURL != null){
                faqEmbedEntry.WithImageUrl(entry.ImageURL);
            }
            faqEmbedEntry.WithTimestamp(entry.ChangedDate);
            faqEmbedEntry.WithAuthor(entry.Author, entry.AuthorAvatarUrl);

            return faqEmbedEntry;
        }

        private static string DiscordChannelUrl = "https://discordapp.com/channels/{0}/{1}";

        private static string DiscordMessageUrl = DiscordChannelUrl + "/{2}";
        public static string GetChannelUrl(ulong serverId, ulong channelId, string displayName)
        {
            return $"[{ displayName }]({string.Format(DiscordChannelUrl, serverId, channelId)})";
        }

        public static string GetMessageUrl(ulong serverId, ulong channelId, ulong messageId, string displayName)
        {
            return $"[{ displayName }]({GetSimpleMessageUrl(serverId, channelId, messageId)})";
        }

        public static string GetSimpleMessageUrl(ulong serverId, ulong channelId, ulong messageId){
            return $"{string.Format(DiscordMessageUrl, serverId, channelId, messageId)}";
        }

        public static string MakeLinkNotEmbedded(string link){
            return "<" + link + ">";
        }

        public static IUser GetUserById(ulong userId)
        {
            return Global.Bot.GetGuild(Global.ServerID).GetUser(userId); 
        }
        

        private static char[] timeFormats = {'m', 'h', 'd', 'w', 's'};
        public static TimeSpan GetTimeSpanFromString(string duration){
            CaptureCollection captures =  Regex.Match(duration, @"(\d+[a-z]+)+").Groups[1].Captures;

            DateTime targetTime = DateTime.Now;
            DateTime timeAtStart = targetTime;
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
                throw new FormatException("Invalid format, it needs to be positive, and combinations of " + string.Join(", ", timeFormats));
            }
            return targetTime - timeAtStart;
        }

        public static string RemoveIllegalPings(string text){
            text = text.Replace("@everyone", "");
            text = text.Replace("@here", "");
            return text;
        }

        public static string FormatTimeSpan(TimeSpan span)
        {
            StringBuilder builder = new StringBuilder("");
            if(span.Days >= 1)
            {
                builder.Append($@"{span:%d} days ");
            }
            if (span.Hours >= 1) 
            {
                builder.Append($@"{span:%h} hours ");
            }
            if (span.Minutes >= 1) 
            {
                builder.Append($@"{span:%m} minutes ");
            }
            if (span.Seconds >= 1)
            {
                builder.Append($@"{span:%s} seconds ");
            }
            if(builder.ToString() == string.Empty)
            {
               builder.Append("Reversed timespan");
            }
            return builder.ToString();
        }
    }
}
