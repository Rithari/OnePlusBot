using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

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