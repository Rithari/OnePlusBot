using Discord;
using OnePlusBot._Extensions;
using Discord.Commands;
using System.Threading.Tasks;

namespace OnePlusBot
{
    public class ShowAvatar : ModuleBase<SocketCommandContext>
    {
        [Command("showavatar")]
        [Summary("Shows avatar of a user.")]
        public async Task Avatar(IGuildUser usr = null)
        {
            if (usr == null)
                usr = (IGuildUser)Context.User;

            var uri = usr.RealAvatarUrl().ToString().Replace("?size=128", "?size=4096");

            if (uri == null)
            {
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("User has no avatar."));
                return;
            }

            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005)
                .AddField(efb => efb.WithName("Username").WithValue(usr.ToString()).WithIsInline(false))
                .AddField(efb => efb.WithName("Avatar Url").WithValue(uri).WithIsInline(false))
                .WithThumbnailUrl(uri)
                .WithImageUrl(uri));
        }
    }
}
