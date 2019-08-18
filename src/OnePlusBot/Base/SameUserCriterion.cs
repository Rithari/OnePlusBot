using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
   
namespace OnePlusBot.Base
{
    internal class ReactionSameUserCriterion : ICriterion<SocketReaction>
    {
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketReaction parameter)
        {
            return Task.FromResult(parameter.UserId == sourceContext.User.Id);
        }
    }
}