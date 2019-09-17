using System.Linq;
using System;
using System.Threading.Tasks;
using Discord;
using OnePlusBot.Base;
using OnePlusBot.Data.Models;

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

        public static string GetChannelUrl(ulong serverId, ulong channelId, string displayName)
        {
            const string discordUrl = "https://discordapp.com/channels/{0}/{1}/";
            return $"[{ displayName }]({string.Format(discordUrl, serverId, channelId)})";
        }

        public static string GetMessageUrl(ulong serverId, ulong channelId, ulong messageId, string displayName)
        {
            const string discordUrl = "https://discordapp.com/channels/{0}/{1}/{2}";
            return $"[{ displayName }]({string.Format(discordUrl, serverId, channelId, messageId)})";
        }

        public static IUser GetUserById(ulong userId)
        {
            return Global.Bot.GetGuild(Global.ServerID).GetUser(userId); 
        }
    }
}
