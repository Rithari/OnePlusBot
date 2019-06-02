using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using OnePlusBot.Helpers;

namespace OnePlusBot.Modules
{
    public class ShowAvatar : ModuleBase<SocketCommandContext>
    {
        [Command("showavatar")]
        [Alias("ava")]
        [Summary("Shows avatar of a user.")]
        public async Task Avatar(IGuildUser user = null)
        {
            if (user == null)
                user = (IGuildUser) Context.User;

            var uri = user.RealAvatarUrl(4096).ToString();

            if (uri == null)
            {
                await Context.Channel.EmbedAsync(new EmbedBuilder()
                    .WithColor(9896005)
                    .WithDescription("User has no avatar."));
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithColor(9896005);
            embed.Url = uri;

            embed.AddField(x =>
            {
                x.Name = "Username";
                x.Value = user.Mention;
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "Image";
                x.Value = $"[Link]({uri})";
                x.IsInline = true;
            });

            embed.ImageUrl = uri;
            
            await Context.Channel.EmbedAsync(embed);
        }
    }
}
