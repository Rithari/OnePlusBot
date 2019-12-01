using System.Linq;
using System;
using System.Threading.Tasks;
using Discord.Commands;
using OnePlusBot.Data.Models;


namespace OnePlusBot.Base
{

    public class CommandDisabledCheck : PreconditionAttribute
    {
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
              return Task.FromResult(PreconditionResult.FromError("Command disabled in this channel"));
            }
        }
    }
}