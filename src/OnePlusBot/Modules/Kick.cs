using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using OnePlusBot.Helpers;
using OnePlusBot.Base;

namespace OnePlusBot.Modules
{
    public class KickModule : ModuleBase<SocketCommandContext>
    {
        [Command("kick", RunMode = RunMode.Async)]
        [Summary("Kicks specified user.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task<RuntimeResult> KickAsync(IGuildUser user, [Remainder] string reason = null)
        {
            if (user.IsBot)
                return CustomResult.FromError("You can't kick bots.");


            if (user.GuildPermissions.PrioritySpeaker)
                return CustomResult.FromError("You can't kick staff.");

            await user.KickAsync(reason);
            return CustomResult.FromSuccess();
        }
    }
}
