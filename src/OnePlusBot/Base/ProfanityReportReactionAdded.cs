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
        public Boolean ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            return Global.ReportedProfanities.Where(prof => prof.MessageId == message.Id).FirstOrDefault() != null;
        }
        public async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction) 
        {
            var emoteMatched = false;
            using(var db = new Database())
            {
                
                var profanity = db.Profanities.Where(prof => prof.MessageId == message.Id).FirstOrDefault();
                if(profanity != null)
                {
                    if(reaction.Emote.Name == Global.OnePlusEmote.OP_NO.Name)
                    {
                        profanity.Valid = false;
                        emoteMatched = true;
                    }
                    else if(reaction.Emote.Name == Global.OnePlusEmote.OP_YES.Name)
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