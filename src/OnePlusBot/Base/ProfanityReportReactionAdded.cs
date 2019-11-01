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
            using(var db = new Database())
            {
                var profanity = db.Profanities.Where(prof => prof.MessageId == message.Id).FirstOrDefault();
                if(profanity != null)
                {
                    if(reaction.Emote.Name == Global.OnePlusEmote.OP_NO.Name)
                    {
                        profanity.Valid = false;
                    }
                    else if(reaction.Emote.Name == Global.OnePlusEmote.OP_YES.Name)
                    {
                        profanity.Valid = true;
                    }
                    db.SaveChanges();
                }
            }
            var prof = Global.ReportedProfanities.Where(prof => prof.MessageId == message.Id).FirstOrDefault();
            if(prof != null)
            {
                Global.ReportedProfanities.Remove(prof);
            }
        }
    }
}