using System.Globalization;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using OnePlusBot.Helpers;

namespace OnePlusBot.Modules
{
    public class Uinfo : ModuleBase<SocketCommandContext>
    {
        [Command("userinfo")]
        [Alias("u")]
        [Summary("Displays User Information")]
        public async Task UserInfo(IGuildUser user = null)
        {
            user = user ?? (IGuildUser) Context.User;
            
            var embed = new EmbedBuilder();

            embed.WithColor(9896005);
            embed.WithAuthor(x =>
            {
                x.Name = user.Username;
            });

            embed.ThumbnailUrl = user.GetAvatarUrl();
            
            embed.AddField(x =>
            {
                x.Name = "Status";
                x.Value = user.Status.ToString();
                x.IsInline = true;
            });
            

            embed.AddField(x =>
            {
                x.Name = "Activity";
                x.Value = user.Activity?.Name ?? "Nothing";
                x.IsInline = true;
            });

            if (user.JoinedAt.HasValue)
            {
                embed.AddField(x =>
                {
                    x.Name = "Joined";
                    x.Value = user.JoinedAt.Value.DateTime.ToString("ddd, MMM dd, yyyy, HH:mm tt", CultureInfo.InvariantCulture);
                    x.IsInline = true;
                });
            }
            
            embed.AddField(x =>
            {
                x.Name = "Registered";
                x.Value = user.CreatedAt.DateTime.ToString("ddd, MMM dd, yyyy, HH:mm tt", CultureInfo.InvariantCulture);
                x.IsInline = true;
            });
            
            
            await Context.Channel.EmbedAsync(embed);
        }
    }
}
