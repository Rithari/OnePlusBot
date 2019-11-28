using System.Runtime.CompilerServices;
using System.Linq;
using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord;


namespace OnePlusBot.Base
{

    public class CommandDisabledCheck : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var channelId = context.Channel.Id;
            var commandInChannel = Global.CommandChannelGroups.Where(
              coChGrp => coChGrp.CommandReference.Name == command.Name &&
              coChGrp.ChannelGroupReference.Channels.
                Where(ch => ch.ChannelId == channelId).Any());
            if(!commandInChannel.Any())
            {
              return Task.FromResult(PreconditionResult.FromSuccess());
            }
            if(commandInChannel.Where(co => co.Disabled).Any())
            {
              return Task.FromResult(PreconditionResult.FromError("Command is disabled in this channel"));
            }
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}