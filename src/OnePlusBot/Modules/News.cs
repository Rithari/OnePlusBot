using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;

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
            var newspingchannel = channels.FirstOrDefault(x => x.Name == "news-alerts");

            var user = Context.Message.Author;

            if (news.Contains("@everyone") || news.Contains("@here"))
                return;

            var oldmessage = await newschannel.SendMessageAsync(news + "\n **Sent by**: " + user);
            var oldmessage = await newspingchannel.SendMessageAsync("@everyone" + news + "\n **Sent by**: " + user);
            await Context.Message.DeleteAsync();

           


         }
    }
}