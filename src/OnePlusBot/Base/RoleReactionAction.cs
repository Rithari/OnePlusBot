using System;
using Discord;
using OnePlusBot.Data;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using OnePlusBot.Data.Models;

namespace OnePlusBot.Base
{
    public class RemoveRoleReactionAction : IReactionAction
    {
        /// <summary>
        /// Checks wheter or not the added reaction should be handled by this action
        /// </summary>
        /// <param name="message">The <see cref="Discord.IUserMessage"> object containing the info message reponsible for handling the info post</param>
        /// <param name="channel">The <see cref="Discord.WebSocket.ISocketMessageChannel"> info context channel</param>
        /// <param name="reaction">The <see cref="Discord.WebSocket.SocketReaction"> object containing the added reaction</param>
        /// <returns>boolean whether or not this action should be executed</returns>
        public Boolean ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            return reaction.MessageId == Global.InfoRoleManagerMessageId;
        }

        /// <summary>
        /// Removes the mapped role from the user reacting to the message in info (devices/helper/news)
        /// </summary>
        /// <param name="message">The <see cref="Discord.IUserMessage"> object containing the info message reponsible for handling the info post</param>
        /// <param name="channel">The <see cref="Discord.WebSocket.ISocketMessageChannel"> info context channel</param>
        /// <param name="reaction">The <see cref="Discord.WebSocket.SocketReaction"> object containing the added reaction</param>
        /// <returns>Task</returns>
        public async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction) 
        {
            if(!(channel is IGuildChannel))
            {
              return;
            }
            var guildChannel = (IGuildChannel) channel;
            var isCustom = !(reaction.Emote is Emoji);
            using(var db = new Database())
            {
              IQueryable<ReactionRole> appropriateRole;
              if(isCustom)
              {
                var emote = reaction.Emote as Emote;
                appropriateRole = db.ReactionRoles.Include(ro => ro.EmoteReference).Where(ro => ro.EmoteReference.EmoteId == emote.Id);
              }
              else
              {
                var emoji = reaction.Emote as Emoji;
                appropriateRole = db.ReactionRoles.Include(ro => ro.EmoteReference).Where(ro => ro.EmoteReference.Name == emoji.Name);
              }
              if(appropriateRole.Any())
              {
                var user = (IGuildUser) reaction.User.Value;
                var role = guildChannel.Guild.GetRole(appropriateRole.First().RoleID);
                await user.RemoveRoleAsync(role);
              }
             
            }
        }
    }

    public class AddRoleReactionAction : IReactionAction
    {
        /// <summary>
        /// Checks wheter or not the added reaction should be handled by this action
        /// </summary>
        /// <param name="message">The <see cref="Discord.IUserMessage"> object containing the info message reponsible for handling the info post</param>
        /// <param name="channel">The <see cref="Discord.WebSocket.ISocketMessageChannel"> info context channel</param>
        /// <param name="reaction">The <see cref="Discord.WebSocket.SocketReaction"> object containing the added reaction</param>
        /// <returns>boolean whether or not this action should be executed</returns>
        public Boolean ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            return reaction.MessageId == Global.InfoRoleManagerMessageId;
        }

        /// <summary>
        /// Gives the user reacting to the message in info the mapped role (devices/helper/news)
        /// </summary>
        /// <param name="message">The <see cref="Discord.IUserMessage"> object containing the info message reponsible for handling the info post</param>
        /// <param name="channel">The <see cref="Discord.WebSocket.ISocketMessageChannel"> info context channel</param>
        /// <param name="reaction">The <see cref="Discord.WebSocket.SocketReaction"> object containing the added reaction</param>
        /// <returns>task</returns>
        public async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction) 
        {
            if(!(channel is IGuildChannel))
            {
              return;
            }
            var guildChannel = (IGuildChannel) channel;
            var isCustom = !(reaction.Emote is Emoji);
            using(var db = new Database())
            {
              IQueryable<ReactionRole> appropriateRole;
              if(isCustom)
              {
                var emote = reaction.Emote as Emote;
                appropriateRole = db.ReactionRoles.Include(ro => ro.EmoteReference).Where(ro => ro.EmoteReference.EmoteId == emote.Id);
              }
              else
              {
                var emoji = reaction.Emote as Emoji;
                appropriateRole = db.ReactionRoles.Include(ro => ro.EmoteReference).Where(ro => ro.EmoteReference.Name == emoji.Name);
              }
              if(appropriateRole.Any())
              {
                var user = (IGuildUser) reaction.User.Value;
                var roleToGive = appropriateRole.First();
                if(roleToGive.MinLevel > 0)
                {
                  var userInDb = db.Users.Include(u => u.CurrentLevel).AsQueryable().Where(dbU => dbU.Id == user.Id);
                  if(userInDb.Any())
                  {
                    var userFromDB = userInDb.FirstOrDefault();
                    if(userFromDB == null || userFromDB.CurrentLevel == null || userFromDB.CurrentLevel.Level < roleToGive.MinLevel)
                    {
                      await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                      return;
                    }
                  }
                }
                var role = guildChannel.Guild.GetRole(roleToGive.RoleID);
                await user.AddRoleAsync(role);
              }
            }
        }
    }
}