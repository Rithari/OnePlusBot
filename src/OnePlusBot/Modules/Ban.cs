using Discord;
using Discord.Commands;
using OnePlusBot.Base;
using OnePlusBot.Helpers;
using System;
using System.Threading.Tasks;

namespace OnePlusBot.Modules
{
    public class BanModule : ModuleBase<SocketCommandContext>
    {
        [Command("ban", RunMode = RunMode.Async)]
        [Summary("Bans specified user.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]

        public async Task OBanAsync(ulong name, [Remainder] string reason = null)
        {
            var modlog = Context.Guild.GetTextChannel(Global.Channels["modlog"]);
            await Context.Guild.AddBanAsync(name, 0, reason);
            
            var emoteTrue = Emote.Parse("<:success:499567039451758603>");
            await Context.Message.AddReactionAsync(emoteTrue);
        }
    
        [Command("ban", RunMode = RunMode.Async)]
        [Summary("Bans specified user.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task BanAsync(IGuildUser user, [Remainder] string reason = null)
        {
            if (user.IsBot)
            {
                var emoteFalse = new Emoji("⚠");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(emoteFalse);
                await Context.Channel.EmbedAsync(
                    new EmbedBuilder()
                        .WithColor(9896005)
                        .WithDescription("⚠ You humans can't make us harm each other.")
                        .WithTitle(user.ToString()));
                return RuntimeResult(CommandError.UnmetPrecondition, "You can't ban bots.");
            }

            if (user.GuildPermissions.PrioritySpeaker)
            {
                var emoteFalse = new Emoji("⚠");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(emoteFalse);
                await Context.Channel.EmbedAsync(
                    new EmbedBuilder()
                        .WithColor(9896005)
                        .WithDescription("⚠ You can not ban authorities.")
                        .WithTitle(user.ToString()));
                return;
            }
            try
            {
                var emoteTrue = Emote.Parse("<:success:499567039451758603>");
                await Context.Message.AddReactionAsync(emoteTrue);

                const string banMessage = "You were banned on r/OnePlus for the following reason: {0}\n" +
                                          "If you believe this to be a mistake, please send an appeal e-mail with all the details to admin@kyot.me";
                await user.SendMessageAsync(string.Format(banMessage, reason));
                await Context.Guild.AddBanAsync(user, 0, reason);

            }
            catch (Exception)
            {
                await Context.Guild.AddBanAsync(user, 0, reason);
            }
        }
    }
}
