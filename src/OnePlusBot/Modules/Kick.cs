using OnePlusBot._Extensions;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace OnePlusBot.Modules
{
    public class KickModule : ModuleBase<SocketCommandContext>
    {
        [Command("kick", RunMode = RunMode.Async)]
        [Summary("Kicks specified user.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        public async Task BanAsync(IGuildUser user,[Remainder] string reason = null)
        {
            if (user.IsBot)
            {
                var EmoteFalse = new Emoji("⚠");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(EmoteFalse);
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("You humans can't make us harm each other."));
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
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("You can not kick authorities."));
                return;
            }


                var EmoteTrue = new Emoji(":success:499567039451758603");
                await Context.Message.AddReactionAsync(EmoteTrue);
                await user.KickAsync(reason);

            
        }
    }
}
