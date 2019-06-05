using System.Threading.Tasks;
using System;
using Discord.Commands;
using System.Linq;
using Discord.WebSocket;
using OnePlusBot.Base;

namespace OnePlusBot.Modules
{
    public class NewsModule : ModuleBase<SocketCommandContext>
    {
        [Command("news")]
        [Summary("Posts a News article to the server.")]
        public async Task<RuntimeResult> NewsAsync([Remainder] string news)
        {
            var guild = Context.Guild;

            var user = (SocketGuildUser)Context.Message.Author;
            var newsChannel = guild.GetTextChannel(Global.Channels["news"]);
            var newsRole = guild.GetRole(Global.Roles["news"]);
            var journalistRole = guild.GetRole(Global.Roles["journalist"]);

            if (news.Contains("@everyone") || news.Contains("@here") || news.Contains("@news")) 
                return CustomResult.FromError("Do not use illegal pings!");


            if (!user.Roles.Contains(journalistRole))
                return CustomResult.FromError("Jounralists only!");

            await newsRole.ModifyAsync(x => x.Mentionable = true);
            await newsChannel.SendMessageAsync(news + Environment.NewLine + Environment.NewLine + newsRole.Mention + Environment.NewLine + "- " + Context.Message.Author);
            await newsRole.ModifyAsync(x => x.Mentionable = false);

            await Context.Message.DeleteAsync();
            return CustomResult.FromSuccess();
        }
    }
}