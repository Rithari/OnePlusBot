using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace OnePlusBot.Base
{
    public class RemoveRoleReactionAction : IReactionAction
    {
        public Boolean ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            return reaction.MessageId == Global.RoleManagerMessageId;
        }
        public async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction) 
        {
            var dict = new Dictionary<string, string> //TODO: Change from string to emote and role IDs
            {
                { "1_", "OnePlus One" },
                { "2_", "OnePlus 2" },
                { "X_", "OnePlus X" },
                { "3_", "OnePlus 3" },
                { "3T", "OnePlus 3T" },
                { "5_", "OnePlus 5" },
                { "5T", "OnePlus 5T" },
                { "6_", "OnePlus 6" },
                { "6T", "OnePlus 6T" },
                { "7_", "OnePlus 7" },
                { "7P", "OnePlus 7 Pro" },
                { "\x2753", "Helper" },
                { "\xD83D\xDCF0", "News" }
            };

            if (dict.TryGetValue(reaction.Emote.Name, out string roleName))
            {
                var guild = ((IGuildChannel) channel).Guild;
                var role = guild.Roles.FirstOrDefault(x => x.Name == roleName);
                if (role != null)
                {
                    var user = (IGuildUser) reaction.User.Value;
                    await user.RemoveRoleAsync(role);
                }
            }
            await Task.CompletedTask;
        }
    }

    public class AddRoleReactionAction : IReactionAction
    {
        public Boolean ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            return reaction.MessageId == Global.RoleManagerMessageId;
        }
        public async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction) 
        {
            var dict = new Dictionary<string, string> //TODO: Change from string to emote and role IDs
            {
                { "1_", "OnePlus One" },
                { "2_", "OnePlus 2" },
                { "X_", "OnePlus X" },
                { "3_", "OnePlus 3" },
                { "3T", "OnePlus 3T" },
                { "5_", "OnePlus 5" },
                { "5T", "OnePlus 5T" },
                { "6_", "OnePlus 6" },
                { "6T", "OnePlus 6T" },
                { "7_", "OnePlus 7" },
                { "7P", "OnePlus 7 Pro" },
                { "\x2753", "Helper" },
                { "\xD83D\xDCF0", "News" }
            };

            if (dict.TryGetValue(reaction.Emote.Name, out string roleName))
            {
                var guild = ((IGuildChannel) channel).Guild;
                var role = guild.Roles.FirstOrDefault(x => x.Name == roleName);
                if (role != null)
                {
                    var user = (IGuildUser) reaction.User.Value;
                    await user.AddRoleAsync(role);
                }
            }
            await Task.CompletedTask;
        }
    }
}