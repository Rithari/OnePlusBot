using System;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace OnePlusBot.Base
{
    public interface IReactionAction
    {
        Boolean ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction);
        Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction);
    }
}