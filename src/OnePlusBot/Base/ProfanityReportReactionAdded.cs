using System;
using Discord;
using Discord.Commands;
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
          return Global.ReportedProfanities.Where(prof => prof.ReportMessageId == message.Id).Any();
        }
        
        /// <summary>
        /// Updates the state of the given profanity report in the database as either valid or not valid, dependingn on which reaction was added
        /// After this is executed once for a given message, it deletes the entry from the global profanity reports list
        /// </summary>
        /// <param name="message">The <see cref="Discord.IUserMessage"> object containing the profanity report at which the reaction was added</param>
        /// <param name="channel">The <see cref="Discord.WebSocket.ISocketMessageChannel"> channel in which the profanity was reported to</param>
        /// <param name="reaction">The <see cref="Discord.WebSocket.SocketReaction"> object containing the added reaction</param>
        /// <returns></returns>
        public async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction) 
        {
            var emoteMatched = false;
            var noEmote = StoredEmote.GetEmote(Global.OnePlusEmote.OP_NO);
            var yesEmote = StoredEmote.GetEmote(Global.OnePlusEmote.OP_YES);
            using(var db = new Database())
            {
                
                var profanity = db.Profanities.Where(prof => prof.ReportMessageId == message.Id).FirstOrDefault();
                if(profanity != null)
                {
                    if(reaction.Emote.Name == noEmote.Name)
                    {
                        profanity.Valid = false;
                        emoteMatched = true;
                    }
                    else if(reaction.Emote.Name == yesEmote.Name)
                    {
                        profanity.Valid = true;
                        emoteMatched = true;
                    }
                }
            
              if(emoteMatched)
              {
                ReactionMetadata yesReaction = message.Reactions.Where(re => re.Key.Name == yesEmote.Name).First().Value;
                ReactionMetadata noReaction = message.Reactions.Where(re => re.Key.Name == noEmote.Name).First().Value;
                var yesCount = yesReaction.ReactionCount;
                var noCount = yesReaction.ReactionCount;
                if(yesReaction.IsMe)
                {
                  yesCount -= 1;
                }
                if(noReaction.IsMe)
                {
                  noCount -= 1;
                }
                var votes = noCount + yesCount;
                if(votes >= Global.ProfanityVoteThreshold && !profanity.PromptPosted)
                {
                  var target = Global.PostTargets[PostTarget.PROFANITY_PROMPT];
                  var profanityPromptChannel = Global.Bot.GetGuild(Global.ServerID).GetTextChannel(target);
                  var builder = new EmbedBuilder();
                  builder.WithTitle("Profanity report has reached vote threshold");
                  builder.WithDescription("Delete the original message?");
                  builder.AddField("Profanity report", Extensions.GetMessageUrl(Global.ServerID, channel.Id, message.Id, "**report**"), true);
                  builder.AddField("Profanity message", Extensions.GetMessageUrl(Global.ServerID, profanity.ChannelId, profanity.MessageId, "**message**"), true);
                  var promptMessage = await profanityPromptChannel.SendMessageAsync(embed: builder.Build());
                  await promptMessage.AddReactionsAsync(new IEmote[]
                  {
                    StoredEmote.GetEmote(Global.OnePlusEmote.OP_NO),
                    StoredEmote.GetEmote(Global.OnePlusEmote.OP_YES)
                  });
                  profanity.PromptPosted = true;
                  profanity.PromptMessagId = promptMessage.Id;
                  profanity.PromptChannelId = promptMessage.Channel.Id;
                  var cached = Global.ReportedProfanities.Where(p => p.ReportMessageId == message.Id).First();
                  cached.PromptPosted = true;
                  cached.PromptMessagId = promptMessage.Id;
                  cached.PromptChannelId = promptMessage.Channel.Id;
                }
              }
              db.SaveChanges();
              
            }
            await Task.CompletedTask;
        }
    }
}