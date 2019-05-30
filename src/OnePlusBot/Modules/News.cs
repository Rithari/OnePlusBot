using System.Threading.Tasks;
using Discord;
using System;
using Discord.Commands;
using System.Linq;
using Discord.WebSocket;
using OnePlusBot.Base;
using OnePlusBot.Helpers;

namespace OnePlusBot.Modules
{
    public class NewsModule : ModuleBase<SocketCommandContext>
    {
        [Command("news")]
        [Summary("Posts a News article to the server.")]
        public async Task NewsAsync([Remainder] string news)
        {
            var guild = Context.Guild;

            var newsChannel = guild.GetTextChannel(Global.Channels["news"]);
            var newsRole = guild.GetRole(Global.Roles["news"]);
            var journalistRole = guild.GetRole(Global.Roles["journalist"]);

            if (news.Contains("@everyone") || news.Contains("@here") || news.Contains("@news")) 
            {
                await Context.Channel.EmbedAsync(new EmbedBuilder()
                    .WithColor(9896005)
                    .WithDescription("⚠ That news contains an illegal ping!"));
                return;
            }

            var user = (SocketGuildUser) Context.Message.Author;

            if (!user.Roles.Contains(journalistRole))
                return;

            await newsRole.ModifyAsync(x => x.Mentionable = true);
            await newsChannel.SendMessageAsync(news + Environment.NewLine + Environment.NewLine + newsRole.Mention + Environment.NewLine + "- " + Context.Message.Author);
            await newsRole.ModifyAsync(x => x.Mentionable = false);

            await Context.Message.DeleteAsync();
        }
    }
}