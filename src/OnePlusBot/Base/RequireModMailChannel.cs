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

        private bool negated { get; }

        public RequireModMailContext(bool negated=false){
            this.negated = negated;
        }
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var channel = (SocketGuildChannel) context.Channel;
            var conditionResult = Global.ModMailThreads.Exists(ch => ch.ChannelId == channel.Id);
            if(negated)
            {
                conditionResult = !conditionResult;
            }
            if(conditionResult)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            string errorMeessage = negated ? "Command not available in modmail context." : "Command only available in modmail context.";
            return Task.FromResult(PreconditionResult.FromError(errorMeessage));
        }
    }
}