using System.Threading.Tasks;
using Discord;
using System;
using Discord.Commands;
using System.Linq;
using OnePlusBot._Extensions;

namespace OnePlusBot.Modules
{
    public class NewsModule : ModuleBase<SocketCommandContext>
    {
        [Command("news")]
        [Summary("Posts a News article to the server.")]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        public async Task NewsAsync([Remainder] string news)
        {
            var channels = Context.Guild.TextChannels;
            var newschannel = channels.FirstOrDefault(x => x.Name == "news");
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "News");

            var user = Context.Message.Author;

            if (news.Contains("@everyone") || news.Contains("@here") || news.Contains("@news"))
            {
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("⚠ That news contains an illegal ping! Don't do that!"));
                return;
            }


            await role.ModifyAsync(x => x.Mentionable = true);

            await newschannel.SendMessageAsync(news + Environment.NewLine + Environment.NewLine + role.Mention + Environment.NewLine + user);

            await role.ModifyAsync(x => x.Mentionable = false);

            await Context.Message.DeleteAsync();
         }
    }
}