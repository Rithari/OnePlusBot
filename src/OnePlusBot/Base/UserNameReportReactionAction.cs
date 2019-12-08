using OnePlusBot.Helpers;
using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;

namespace OnePlusBot.Base
{
    public class UserNameReportReactionAction : IReactionAction
    {

        /// <summary>
        /// Only execute the action in case the emote is the post box and when the element is found in the global list of nickname notifications
        /// </summary>
        /// <returns></returns>
        public Boolean ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            return Global.UserNameNotifications.Where(n => n.Key == message.Id).Any() && 
            reaction.Emote.Name == Global.Emotes[Global.OnePlusEmote.OPEN_MODMAIL].GetAsEmote().Name;
        }

        /// <summary>
        /// Opens a modmail thread for the given user causing this nickname notification
        /// </summary>
        /// <returns>Task</returns>
        public virtual async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction) 
        {
          var entry = Global.UserNameNotifications.Where(n => n.Key == message.Id).First();
          var guild = Global.Bot.GetGuild(Global.ServerID);
          await new ModMailManager().ContactUser(guild.GetUser(entry.Value), channel);
          Global.UserNameNotifications.Remove(entry.Key);
        }
    }

}