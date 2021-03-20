using System;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Threading.Tasks;
using OnePlusBot.Data.Models;
using System.Linq;
using OnePlusBot.Data;
using OnePlusBot.Helpers;

namespace OnePlusBot.Base
{
    public class ProfanityReportReactionAdded : IReactionAction
    {
        /// <summary>
        /// Checks wheter or not the added reaction should be handled by this action
        /// </summary>
        /// <param name="message">The <see cref="Discord.IUserMessage"> object containing the profanity report at which the reaction was added</param>
        /// <param name="channel">The <see cref="Discord.WebSocket.ISocketMessageChannel"> channel in which the profanity was reported to</param>
        /// <param name="reaction">The <see cref="Discord.WebSocket.SocketReaction"> object containing the added reaction</param>
        /// <returns>boolean whether or not this action should be executed</returns>
        public Boolean ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
        {
          return Global.ReportedProfanities.Where(prof => prof.ReportMessageId == message.Id).Any() && CommandHandler.FeatureFlagEnabled(FeatureFlag.PROFANITY);
        }
        
        /// <summary>
        /// Updates the state of the given profanity report in the database as either valid or not valid, dependingn on which reaction was added
        /// After this is executed once for a given message, and the votes cast are over a certain threshold (yes or no) this method will either delete the message
        /// or remove it only from the runtime data structure
        /// </summary>
        /// <param name="message">The <see cref="Discord.IUserMessage"> object containing the profanity report at which the reaction was added</param>
        /// <param name="channel">The <see cref="Discord.WebSocket.ISocketMessageChannel"> channel in which the profanity was reported to</param>
        /// <param name="reaction">The <see cref="Discord.WebSocket.SocketReaction"> object containing the added reaction</param>
        /// <returns></returns>
        public async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction) 
        {
            var noEmote = StoredEmote.GetEmote(Global.OnePlusEmote.OP_NO);
            var yesEmote = StoredEmote.GetEmote(Global.OnePlusEmote.OP_YES);
            using(var db = new Database())
            {
                var profanityLq = db.Profanities.AsQueryable().Where(prof => prof.ReportMessageId == message.Id);
                if(profanityLq.Any())
                {
                    if(reaction.Emote.Name == noEmote.Name || reaction.Emote.Name == yesEmote.Name)
                    {
                      var profanity = profanityLq.First();
                      ReactionMetadata yesReaction = message.Reactions.Where(re => re.Key.Name == yesEmote.Name).First().Value;
                      var yesCount = yesReaction.ReactionCount;
                      if(yesReaction.IsMe)
                      {
                        yesCount -= 1;
                      }

                      ReactionMetadata noReaction = message.Reactions.Where(re => re.Key.Name == noEmote.Name).First().Value;
                      var noCount = noReaction.ReactionCount;
                      if(noReaction.IsMe)
                      {
                        noCount -= 1;
                      }
                      if(yesCount >= Global.ProfanityVoteThreshold)
                      {
                        profanity.Valid = true;
                        var guild = Global.Bot.GetGuild(Global.ServerID);
                        var profaneMessage = await guild.GetTextChannel(profanity.ChannelId).GetMessageAsync(profanity.MessageId);
                        if(profaneMessage != null)
                        {
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
                        }
                        Global.ReportedProfanities.Remove(Global.ReportedProfanities.Where(g => g.ProfanityId == profanity.ProfanityId).FirstOrDefault());
                      }
                      if(noCount >= Global.ProfanityVoteThreshold)
                      {
                         Global.ReportedProfanities.Remove(Global.ReportedProfanities.Where(g => g.ProfanityId == profanity.ProfanityId).FirstOrDefault());
                      }
                    }
                  }
                  db.SaveChanges();
                }
            await Task.CompletedTask;
        }
    }
}