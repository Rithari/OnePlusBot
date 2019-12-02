using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using OnePlusBot.Data;

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
            return Global.ReportedProfanities.Where(prof => prof.MessageId == message.Id).FirstOrDefault() != null;
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
            using(var db = new Database())
            {
                
                var profanity = db.Profanities.Where(prof => prof.MessageId == message.Id).FirstOrDefault();
                if(profanity != null)
                {
                    if(reaction.Emote.Name == Global.Emotes[Global.OnePlusEmote.OP_NO].GetAsEmote().Name)
                    {
                        profanity.Valid = false;
                        emoteMatched = true;
                    }
                    else if(reaction.Emote.Name == Global.Emotes[Global.OnePlusEmote.OP_YES].GetAsEmote().Name)
                    {
                        profanity.Valid = true;
                        emoteMatched = true;
                    }
                    db.SaveChanges();
                }
            }
            var prof = Global.ReportedProfanities.Where(prof => prof.MessageId == message.Id).FirstOrDefault();
            if(prof != null && emoteMatched)
            {
                Global.ReportedProfanities.Remove(prof);
            }
            await Task.CompletedTask;
        }
    }
}