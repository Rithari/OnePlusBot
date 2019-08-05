using System;
using System.Threading.Tasks;
using Discord;
using OnePlusBot.Base;

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
            return user.Username + '#' + user.Discriminator;
        }

        public static String FormatUserNameDetailed(IUser user)
        {
            return FormatUserName(user) + " (" + user.Id +  ") ";
        }
    }
}
