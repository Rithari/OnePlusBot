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
        public static async Task MuteUser(IGuildUser user)
        {
            var muteRole = user.Guild.GetRole(Global.Roles["muted"]);
            await user.AddRoleAsync(muteRole);
        }


        public static async Task UnMuteUser(IGuildUser user)
        {
            var muteRole = user.Guild.GetRole(Global.Roles["muted"]);
            await user.RemoveRoleAsync(muteRole);
        }

        public static String FormatUserName(IUser user)
        {
            return user.Username + '#' + user.Discriminator;
        }
    }
}
