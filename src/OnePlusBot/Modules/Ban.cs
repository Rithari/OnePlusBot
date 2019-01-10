using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;

namespace OnePlusBot.Modules
{
    public class BanModule : ModuleBase<SocketCommandContext>
    {
        [Command("ban", RunMode = RunMode.Async)]
        [Summary("Bans specified user.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(IGuildUser user,[Remainder] string reason = null)
        {
            if (user.IsBot)
            {
                var EmoteFalse = new Emoji("⚠");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(EmoteFalse);
                await ReplyAsync("You humans can't make us harm each other.");
                return;
            }

            if(reason == null)
            {
                reason = "No reason provided.";
            }

            if (user.GuildPermissions.PrioritySpeaker)
            {
                var EmoteFalse = new Emoji("⚠");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(EmoteFalse);
                await ReplyAsync("You can not ban authorities.");
                return;
            }
            try
            {
                var EmoteTrue = new Emoji(":success:499567039451758603");
                await Context.Message.AddReactionAsync(EmoteTrue);
                await user.SendMessageAsync("You were banned on /r/OnePlus for the following reason: " + reason + "\nIf you believe this to be a mistake, please send an appeal e-mail with all the details to admin@kyot.me");
                await Context.Guild.AddBanAsync(user, 0, reason);

            }
            catch (Exception)
            {
                await Context.Guild.AddBanAsync(user, 0, reason);
            }
        }
    }
}
