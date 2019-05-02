using Discord;
using OnePlusBot._Extensions;
using Discord.Commands;
using System.Threading.Tasks;

namespace OnePlusBot.Modules
{
    public class Uinfo : ModuleBase<SocketCommandContext>
    {
        [RequireContext(ContextType.Guild)]
        public async Task UserInfo(IGuildUser usr = null)
        {
            var user = usr ?? Context.User as IGuildUser;

            if (user == null)
                return;

            var embed = new EmbedBuilder()
                .AddField(fb => fb.WithName("Name").WithValue($"**{user.Username}**#{user.Discriminator}").WithIsInline(true));
            if (!string.IsNullOrWhiteSpace(user.Nickname))
            {
                embed.AddField(fb => fb.WithName("Nickname").WithValue(user.Nickname).WithIsInline(true));
            }
            embed.AddField(fb => fb.WithName("ID").WithValue(user.Id.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Joined Server").WithValue($"{user.JoinedAt?.ToString("dd.MM.yyyy HH:mm") ?? "?"}").WithIsInline(true))
                .AddField(fb => fb.WithName("Joined Discord").WithValue($"{user.CreatedAt:dd.MM.yyyy HH:mm}").WithIsInline(true));

            var av = user.RealAvatarUrl();
            if (av != null && av.IsAbsoluteUri)
                embed.WithThumbnailUrl(av.ToString());
            await Context.Channel.EmbedAsync(embed).ConfigureAwait(false);
        }
    }
}
