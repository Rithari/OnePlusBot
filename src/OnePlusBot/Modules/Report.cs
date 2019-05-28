using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using OnePlusBot._Extensions;
using System;

namespace OnePlusBot.Modules
{
    public class ReportModule : ModuleBase<SocketCommandContext>
    {
        [Command("report")]
        [Summary("Suggests something to the server.")]
        public async Task ReportAsync(IGuildUser user, [Remainder] string reason)
        {
            var reportChannel = Context.Guild.TextChannels.FirstOrDefault(x => x.Name == "reports");

            var reporter = Context.Message.Author;

            var builder = new EmbedBuilder()
            .WithTitle("...a new report has emerged from outer space!")
            .WithColor(new Color(0x3E518))
            .WithTimestamp(Context.Message.Timestamp)
            .WithFooter(footer => 
            {
              footer
                // TO DO: Implement incremential Cases with database.
                   .WithText("Case # (WIP)")
                   .WithIconUrl("https://a.kyot.me/0WPy.png");
             })
            .WithThumbnailUrl("https://a.kyot.me/ab-y.png")
            .WithAuthor(author =>
            {
                author
                .WithName("Woah...")
                .WithIconUrl("https://a.kyot.me/cno0.png");
            })

            .AddField("Reported User", user)
            .AddField("Reported by", reporter)
            .AddField("Location of the incident", Context.Message.Channel.Name + " [Warp!](https://discordapp.com/channels/"+ Context.Guild.Id + "/" + Context.Channel.Id + "/" + Context.Message.Id + ")")
            .AddField("Reason", reason);


            var embed = builder.Build();
            await reportChannel.SendMessageAsync(null,embed: embed).ConfigureAwait(false);

            //await Context.Message.DeleteAsync();
            await ReplyAsync(user + " successfully reported.");



        }
    }
}
