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
        [
            Command("banid", RunMode = RunMode.Async),
            Summary("Bans specified user."),
            RequireBotPermission(GuildPermission.BanMembers),
            RequireUserPermission(GuildPermission.PrioritySpeaker),
            RequireUserPermission(GuildPermission.ManageNicknames)
        ]
        public async Task<RuntimeResult> OBanAsync(ulong name, [Remainder] string reason = null)
        {
            var modlog = Context.Guild.GetTextChannel(Global.Channels["modlog"]);
            await Context.Guild.AddBanAsync(name, 0, reason);

            return CustomResult.FromSuccess();
        }
    

        [
            Command("ban", RunMode = RunMode.Async),
            Summary("Bans specified user."),
            RequireBotPermission(GuildPermission.BanMembers),
            RequireUserPermission(GuildPermission.PrioritySpeaker),
            RequireUserPermission(GuildPermission.ManageNicknames)
        ]
        public async Task<RuntimeResult> BanAsync(IGuildUser user, [Remainder] string reason = null)
        {
            if (user.IsBot)
                return CustomResult.FromError("You can't ban bots.");

            if (user.GuildPermissions.PrioritySpeaker)
                return CustomResult.FromError("You can't ban staff.");
            try
            {
                const string banMessage = "You were banned on r/OnePlus for the following reason: {0}\n" +
                                          "If you believe this to be a mistake, please send an appeal e-mail with all the details to admin@kyot.me";
                await user.SendMessageAsync(string.Format(banMessage, reason));
                await Context.Guild.AddBanAsync(user, 0, reason);
                return CustomResult.FromSuccess();

            }
            catch (Exception ex)
            {   
                //  may not be needed
                // await Context.Guild.AddBanAsync(user, 0, reason);
                return CustomResult.FromError(ex.Message);
            }
        }
    }
}
