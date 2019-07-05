using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Runtime.InteropServices;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using System.Linq;

namespace OnePlusBot.Modules
{
    public class WarningModule : ModuleBase<SocketCommandContext>
    {
        [Command("warn")]
        [Summary("Warn someone.")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task<RuntimeResult> WarnAsync(IGuildUser user, [Optional] [Remainder] string reason)
        {
            var warningsChannel = Context.Guild.GetTextChannel(Global.Channels["warnings"]);

            var monitor = Context.Message.Author;

            var entry = new WarnEntry
            {
                WarnedUser = user.Username + '#' + user.Discriminator,
                WarnedUserID = user.Id,
                WarnedBy = monitor.Username + '#' + monitor.Discriminator,
                WarnedByID = monitor.Id,
                Reason = reason,
                Date = Context.Message.Timestamp.DateTime,
            };

            using (var db = new Database())
            {
                db.Warnings.Add(entry);
                db.SaveChanges();
            }

            var builder = new EmbedBuilder();
            builder.Title = "...a new warning has emerged from outer space!";
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
            builder.AddField("Warned User", user.Mention)
                .AddField("Warned by", monitor.Mention)
                .AddField(
                    "Location of the incident",
                    $"[#{Context.Message.Channel.Name}]({string.Format(discordUrl, Context.Guild.Id, Context.Channel.Id, Context.Message.Id)})")
                .AddField("Reason", reason ?? "No reason was provided.");
               
            var embed = builder.Build();
            await warningsChannel.SendMessageAsync(null,embed: embed).ConfigureAwait(false);

            return CustomResult.FromSuccess();
        }

        [Command("clearwarn")]
        [Summary("Clear warnings.")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task<RuntimeResult> ClearwarnAsync(uint index)
        {
            var warningsChannel = Context.Guild.GetTextChannel(Global.Channels["warnings"]);
            var monitor = Context.Message.Author;

            await ReplyAsync($"we are going to delete {index}");

            using (var db = new Database())
            {
                IQueryable<WarnEntry> warnings = db.Warnings;
                var selection = warnings.First(x => x.ID == index);
                db.Warnings.Remove(selection);
               await db.SaveChangesAsync();
            }

           return CustomResult.FromSuccess();
        }
    }
}
