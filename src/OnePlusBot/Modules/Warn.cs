using System;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using OnePlusBot._Extensions;

namespace OnePlusBot.Modules
    public class WarnModule : ModuleBase<SocketCommandContext>
    {
        [Command("warn", RunMode = RunMode.Async)]
        [Summary("Warns specified user.")]
        [RequireBotPermission(GuildPermission.WarnMembers)]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        public async Task WarnAsync(IGuildUser user, [Remainder] string reason = null)
        {
            if (user.IsBot)
            {
                var EmoteFalse = new Emoji("⚠");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(EmoteFalse);
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("⚠ You humans can't make us harm each other.").WithTitle("" + user));
                return;
            }

            if (reason == null)
            {
                reason = "No reason provided.";
            }

            if (user.GuildPermissions.PrioritySpeaker)
            {
                var EmoteFalse = new Emoji("⚠");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(EmoteFalse);
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("⚠ You can not warn authorities.").WithTitle("" + user));
                return;
            }
            try
            {
                var EmoteTrue = new Emoji(":success:499567039451758603");
                await Context.Message.AddReactionAsync(EmoteTrue);

                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005)
                .WithTitle("⚠ Warned User")
                .AddField(efb => efb.WithName("Username").WithValue(user.ToString()).WithIsInline(true))
                .AddField(efb => efb.WithName("ID").WithValue(user.Id.ToString()).WithIsInline(true)));

                await user.SendMessageAsync("You were warned on r/OnePlus for the following reason: " + reason + "\nIf you believe this to be a mistake, please send an appeal e-mail with all the details to admin@kyot.me");
                await Context.Guild.AddWarnAsync(user, 0, reason);

            }
            catch (Exception)
            {
                await Context.Guild.AddWarnAsync(user, 0, reason);
            }
        }
    }
}
