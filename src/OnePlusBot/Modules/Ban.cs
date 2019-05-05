using System;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using OnePlusBot._Extensions;

namespace OnePlusBot.Modules
{
    public class BanModule : ModuleBase<SocketCommandContext>
    {
        [Command("ban", RunMode = RunMode.Async)]
        [Summary("Bans specified user.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        public async Task BanAsync(IGuildUser user, [Remainder] string reason = null)
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
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("⚠ You can not ban authorities.").WithTitle("" + user));
                return;
            }
            try
            {
                var EmoteTrue = new Emoji(":success:499567039451758603");
                await Context.Message.AddReactionAsync(EmoteTrue);

                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005)
                .WithTitle("⛔️ Banned User")
                .AddField(efb => efb.WithName("Username").WithValue(user.ToString()).WithIsInline(true))
                .AddField(efb => efb.WithName("ID").WithValue(user.Id.ToString()).WithIsInline(true)));

                await user.SendMessageAsync("You were banned on r/OnePlus for the following reason: " + reason + "\nIf you believe this to be a mistake, please send an appeal e-mail with all the details to admin@kyot.me");
                await Context.Guild.AddBanAsync(user, 0, reason);

            }
            catch (Exception)
            {
            }
        }
    }
}
