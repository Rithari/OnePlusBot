using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using OnePlusBot.Helpers;

namespace OnePlusBot.Modules
{
    public class KickModule : ModuleBase<SocketCommandContext>
    {
        [Command("kick", RunMode = RunMode.Async)]
        [Alias("k")]
        [Summary("Kicks specified user.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task KickAsync(IGuildUser user, [Remainder] string reason = null)
        {
            if (user.IsBot)
            {
                var emoteFalse = new Emoji("⚠");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(emoteFalse);
                await Context.Channel.EmbedAsync(new EmbedBuilder()
                    .WithColor(9896005)
                    .WithDescription("⚠ You humans can't make us harm each other.")
                    .WithTitle(user.ToString()));
                return;
            }
            
            if (user.GuildPermissions.PrioritySpeaker)
            {
                var emoteFalse = new Emoji("⚠");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(emoteFalse);
                await Context.Channel.EmbedAsync(new EmbedBuilder()
                    .WithColor(9896005)
                    .WithDescription("⚠ You can not kick authorities.")
                    .WithTitle(user.ToString()));
                return;
            }

            var emoteTrue = Emote.Parse("<:success:499567039451758603>");
            await Context.Message.AddReactionAsync(emoteTrue);
            await user.KickAsync(reason);
        }
    }
}
