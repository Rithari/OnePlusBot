using System.Linq;
using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord;


namespace OnePlusBot.Base
{

    public class RequireModMailContext : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var channel = (SocketGuildChannel) context.Channel;
            if(Global.ModMailThreads.Exists(ch => ch.ChannelId == channel.Id))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            return Task.FromResult(PreconditionResult.FromError("Command only available in modmail context."));
        }
    }
}