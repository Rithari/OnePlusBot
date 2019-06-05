using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Runtime.InteropServices;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using OnePlusBot.Helpers;
using System;

namespace OnePlusBot.Modules
{
    public class ReportModule : ModuleBase<SocketCommandContext>
    {
        [Command("report")]
        [Summary("Suggests something to the server.")]
        public async Task<RuntimeResult> ReportAsync(IGuildUser user, [Optional] [Remainder] string reason)
        {
            var reportChannel = Context.Guild.GetTextChannel(Global.Channels["reports"]);;

            var reporter = Context.Message.Author;

            var entry = new ReportEntry
            {
                ReportedUser = user.Username + '#' + user.Discriminator,
                ReportedUserId = user.Id,
                ReportedBy = reporter.Username + '#' + reporter.Discriminator,
                ReportedById = reporter.Id,
                Reason = reason,
                ChannelID = Context.Channel.Id,
                Date = Context.Message.Timestamp.Date,
            };

            using (var db = new Database())
            {
                db.Reports.Add(entry);
                db.SaveChanges();
            }

            var builder = new EmbedBuilder();
            builder.Title = "...a new report has emerged from outer space!";
            builder.Color = new Color(0x3E518);
            
            builder.Timestamp = Context.Message.Timestamp;
            
            builder.WithFooter(footer =>
            {
                footer
                    .WithText("Case #" + entry.ID)
                    .WithIconUrl("https://a.kyot.me/0WPy.png");
            });
            
            builder.ThumbnailUrl = user.GetAvatarUrl();
            
            builder.WithAuthor(author =>
            {
                author
                    .WithName("Woah...")
                    .WithIconUrl("https://a.kyot.me/cno0.png");
            });

            const string discordUrl = "https://discordapp.com/channels/{0}/{1}/{2}";
            builder.AddField("Reported User", user.Mention)
                .AddField("Reported by", reporter.Mention)
                .AddField(
                    "Location of the incident",
                    $"[#{Context.Message.Channel.Name}]({string.Format(discordUrl, Context.Guild.Id, Context.Channel.Id, Context.Message.Id)})")
                .AddField("Reason", reason ?? "No reason was provided.");


            var embed = builder.Build();
            await reportChannel.SendMessageAsync(null,embed: embed).ConfigureAwait(false);
            return CustomResult.FromSuccess();
        }
    }
}