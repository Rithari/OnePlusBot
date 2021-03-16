using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using OnePlusBot.Base;
using OnePlusBot.Data.Models;
using System.Runtime.InteropServices;

namespace OnePlusBot.Modules.Administration
{
    public partial class Administration : ModuleBase<SocketCommandContext>
    {
        [
          Command("updateLevels", RunMode=RunMode.Async),
          Summary("Re-evaluates the experience, levels and assigns the roles to the users (takes a long time, use with care)"),
          RequireRole(new string[]{"admin", "founder"}),
          CommandDisabledCheck
        ]
        public async Task<RuntimeResult> UpdateLevels([Optional] IGuildUser user)
        {
            if(CommandHandler.FeatureFlagDisabled(FeatureFlag.EXPERIENCE)) 
            {
                return CustomResult.FromIgnored();
            }
            if(user == null)
            {
                await Context.Channel.SendMessageAsync("DO NOT execute commands changing the role experience configuration while this is processing. Especially do not start another one, while one is running.");
                var message = await Context.Channel.SendMessageAsync("Processing");
                new ExpManager().UpdateLevelsOfMembers(message);
            }
            else
            {
                await new ExpManager().UpdateLevelOf(user);
            }
          
            return CustomResult.FromSuccess();
        }

        [
          Command("roleLevel"),
          Summary("Sets the level at which a role is given. If no parameters, shows the current role configuration"),
          RequireRole(new string[]{"admin", "founder"}),
          CommandDisabledCheck
        ]
        public async Task<RuntimeResult> SetRoleToLevel([Optional] uint level, [Optional] ulong roleId)
        {
            if(CommandHandler.FeatureFlagDisabled(FeatureFlag.EXPERIENCE)) 
            {
                return CustomResult.FromIgnored();
            }
            if(level == 0 && roleId == 0)
            {
                await new ExpManager().ShowLevelconfiguration(Context.Channel);
            } 
            else
            {
                new ExpManager().SetRoleToLevel(level, roleId);
            }
           
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }

        [
          Command("disableXpGain"),
          Summary("Enables/disables xp gain for a user"),
          RequireRole(new string[]{"admin", "founder"}),
          CommandDisabledCheck
        ]
        public async Task<RuntimeResult> SetExpGainEnabled(IGuildUser user, bool newValue)
        {
            if(CommandHandler.FeatureFlagDisabled(FeatureFlag.EXPERIENCE)) 
            {
                return CustomResult.FromIgnored();
            }
            new ExpManager().SetXPDisabledTo(user, newValue);
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }
    }
}