using OnePlusBot.Helpers;
using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using Discord.Rest;

namespace OnePlusBot.Base
{
    public class ProfanityPromptReaction : IReactionAction
    {

        public Boolean ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
        {
          var socketUser = Global.Bot.GetGuild(Global.ServerID).GetUser(reaction.UserId);
          return Global.ReportedProfanities.Where(prof => prof.PromptMessagId == message.Id).Any() 
          && (
            reaction.Emote.Name == StoredEmote.GetEmote(Global.OnePlusEmote.OP_NO).Name || 
            reaction.Emote.Name == StoredEmote.GetEmote(Global.OnePlusEmote.OP_YES).Name
          ) &&
          Extensions.UserHasRole(socketUser, new string[]{"admin", "founder"});
        }
        public virtual async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction) 
        {
          var bot = Global.Bot;
          var guild = bot.GetGuild(Global.ServerID);
          using(var db = new Database())
          {
            var profaneLinq = db.Profanities.Where(p => p.PromptMessagId == message.Id);
            if(profaneLinq.Any())
            {
              if(reaction.Emote.Name == StoredEmote.GetEmote(Global.OnePlusEmote.OP_YES).Name)
              {
                var profaneMessageFromDb = profaneLinq.First();
                var profaneMessage = await guild.GetTextChannel(profaneMessageFromDb.ChannelId).GetMessageAsync(profaneMessageFromDb.MessageId);
                if(profaneMessage is RestUserMessage)
                {
                  var castedProfaneMessage = profaneMessage as RestUserMessage;
                  await castedProfaneMessage.DeleteAsync();
                }
                else if(profaneMessage is SocketUserMessage)
                {
                  var castedProfaneMessage = profaneMessage as SocketUserMessage;
                  await castedProfaneMessage.DeleteAsync();
                }
                Global.ReportedProfanities.Remove(profaneMessageFromDb);
              }
            }
          }
          await message.DeleteAsync();
        }
    }
}