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

            var plaintiff = Context.Message.Author;
            var builder = new EmbedBuilder()
            .WithTitle("Report # ")
            .WithUrl("https://discordapp.com/channels/"+ Context.Guild.Id + "/" + Context.Channel.Id + "/" + Context.Message.Id)
            .WithColor(new Color(0xBB6AAF))
            .WithTimestamp(Context.Message.Timestamp)
            .WithFooter(footer => 
            {
              footer
                   .WithText("Case #0000")
                   .WithIconUrl("https://a.kyot.me/0WPy.png");
             })
            .WithThumbnailUrl("https://a.kyot.me/ab-y.png")
            .WithAuthor(author =>
            {
                author
                .WithName("New Report!")
                .WithUrl("https://discordapp.com")
                .WithIconUrl("https://a.kyot.me/ab-y.png");
            })

            .AddField("Defendant", user)
            .AddField("Plaintiff", plaintiff)
            .AddField("Location of the incident", Context.Message.Channel)
            .AddField("Reason", reason);
            var embed = builder.Build();
            await reportChannel.SendMessageAsync(
                null,
                embed: embed)
                .ConfigureAwait(false);


            //await reportChannel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription(reason).WithFooter("" + plaintiff));

            //await Context.Message.DeleteAsync();
            await ReplyAsync(user + " successfully reported.");



        }
    }
}
