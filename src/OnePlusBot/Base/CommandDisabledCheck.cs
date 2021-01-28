using System.Linq;
using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace OnePlusBot.Base
{
    /// <summary>
    /// Executed before most commands are executed (commands related to disabling commands are exempt from this)
    /// </summary>
    public class CommandDisabledCheck : PreconditionAttribute
    {
        /// <summary>
        /// Checks whether the given command should be able to be executed in the given channel
        /// </summary>
        /// <param name="context">The <see cref="Discord.Commands.ICommandContext"> object containing context for the command</param>
        /// <param name="command">The <see cref="Discord.Commands.CommandInfo"> object containing infor about the command</param>
        /// <param name="services">The <see cref="System.IServiceProvider"> service provider object of this bot</param>
        /// <returns><see cref="Discord.Commands.PreconditionResult"> result whether or not the command should be executed.</returns>
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var channelId = context.Channel.Id;
            var commandFromDb = Global.Commands.Where(cmd => cmd.Name == command.Name);
            var result = false;
            if(commandFromDb.Any())
            {
              result = commandFromDb.First().CommandEnabled(context.Channel.Id);
            }
            else
            {
              result = false;
            }
            if(result)
            {
              return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else 
            {
              var guild = Global.Bot.GetGuild(Global.ServerID);
              var guildUser = guild.GetUser(context.User.Id);
              if(guildUser != null && guildUser.Roles.Where(ro => ro.Id == Global.Roles["staff"]).Any())
              {
                return Task.FromResult(PreconditionResult.FromSuccess());
              }
              return Task.FromResult(PreconditionResult.FromError("Command disabled in this channel"));
            }
        }
    }
}