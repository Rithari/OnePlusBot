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
        public Boolean ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            return reaction.MessageId == Global.InfoRoleManagerMessageId;
        }
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
        public Boolean ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            return reaction.MessageId == Global.InfoRoleManagerMessageId;
        }
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
                await user.AddRoleAsync(role);
              }
            }
        }
    }
}