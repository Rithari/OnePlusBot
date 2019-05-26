using System;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using OnePlusBot._Extensions;
using System.Linq;

namespace OnePlusBot.Modules
{
    public class BanModule : ModuleBase<SocketCommandContext>
    {
        [Command("oban", RunMode = RunMode.Async)]
        [Summary("Bans specified user.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]

        public async Task OBanAsync(ulong name, [Remainder] string reason = null)
        {
            var modlog = Context.Guild.GetTextChannel(378983972174168066);
            await Context.Guild.AddBanAsync(name, 0, reason);
            var EmoteTrue = new Emoji(":success:499567039451758603");
            await Context.Message.AddReactionAsync(EmoteTrue);
            await modlog.EmbedAsync(new EmbedBuilder().WithColor(9896005)
                .WithTitle("⛔️ Banned User")
                .AddField(efb => efb.WithName("Username").WithValue(("<@"+name.ToString())+">").WithIsInline(true))
                .AddField(efb => efb.WithName("ID").WithValue(name.ToString()).WithIsInline(true)));
        }
    
        [Command("ban", RunMode = RunMode.Async)]
        [Summary("Bans specified user.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task BanAsync(IGuildUser user, [Remainder] string reason = null)
        {
            var modlog = Context.Guild.GetTextChannel(378983972174168066);

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

                await modlog.EmbedAsync(new EmbedBuilder().WithColor(9896005)
                .WithTitle("⛔️ Banned User")
                .AddField(efb => efb.WithName("Username").WithValue(user.ToString()).WithIsInline(true))
                .AddField(efb => efb.WithName("ID").WithValue(user.Id.ToString()).WithIsInline(true)));

                await user.SendMessageAsync("You were banned on r/OnePlus for the following reason: " + reason + "\nIf you believe this to be a mistake, please send an appeal e-mail with all the details to admin@kyot.me");
                await Context.Guild.AddBanAsync(user, 0, reason);

            }
            catch (Exception)
            {
                await Context.Guild.AddBanAsync(user, 0, reason);
            }
        }
    }
}
