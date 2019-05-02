using Discord;
using System.Threading.Tasks;
using System;

namespace OnePlusBot._Extensions
{
    public static class Extensions
    {
        public static Task<IUserMessage> EmbedAsync(this IMessageChannel ch, EmbedBuilder embed, string msg = "")
            => ch.SendMessageAsync(msg, embed: embed.Build(),
                 options: new RequestOptions() { RetryMode = RetryMode.AlwaysRetry });

        public static Uri RealAvatarUrl(this IUser usr, int size = 0)
        {
            var append = size <= 0
                ? ""
                : $"?size={size}";

            return usr.AvatarId == null
                ? null
                : new Uri(usr.AvatarId.StartsWith("a_", StringComparison.InvariantCulture)
                    ? $"{DiscordConfig.CDNUrl}avatars/{usr.Id}/{usr.AvatarId}.gif" + append
                    : usr.GetAvatarUrl(ImageFormat.Auto) + append);
        }
    }
}
