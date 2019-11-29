using System.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using OnePlusBot.Data.Models;
using Discord.WebSocket;
using OnePlusBot.Data;
using Microsoft.EntityFrameworkCore;
using OnePlusBot.Helpers;
using Discord.Rest;
using System.Globalization;

namespace OnePlusBot.Base
{
  public class ModMailManager 
  {
    public async Task CreateModmailThread(SocketMessage message)
    {
        var userFromCache = Global.ModMailThreads.Where(th => th.UserId == message.Author.Id).FirstOrDefault();
        if(userFromCache != null &&  userFromCache.ThreadUser.ModMailMuted && userFromCache.ThreadUser.ModMailMutedUntil > DateTime.Now)
        {
            if(!userFromCache.ThreadUser.ModMailMutedReminded) {
                await message.Channel.SendMessageAsync($"You are unable to contact modmail until {userFromCache.ThreadUser.ModMailMutedUntil:dd.MM.yyyy HH:mm} {TimeZoneInfo.Local}.");
                using(var db = new Database())
                {
                    db.Users.Where(us => us.Id == message.Author.Id).First().ModMailMutedReminded = true;
                    db.SaveChanges();
                }

                Global.ReloadModmailThreads();
            }
            return;
        }

        using(var db = new Database())
        {
            var user = db.Users.Where(us => us.Id == message.Author.Id).FirstOrDefault();
            if(user == null)
            {
                var newUser = new User();
                newUser.Id = message.Author.Id;
                newUser.ModMailMuted = false;
                db.Users.Add(newUser);
                db.SaveChanges();
            }
        }
        int pastThreads = 0;
        using(var db = new Database())
        {
            pastThreads =  db.ModMailThreads.Where(ch => ch.UserId == message.Author.Id).Count();
        }
        // when I tried to load the channel via getTextChannel, I got null in return, my only guess is that the channel
        // did not get peristent completely, so it did not find it
        // if we return the directly returned channel it worked
        var channel = await CreateModMailThread(message.Author);
        var guild = Global.Bot.GetGuild(Global.ServerID);
        await channel.SendMessageAsync(embed: ModMailEmbedHandler.GetUserInformation(pastThreads, message.Author));
        var channelMessage = await channel.SendMessageAsync(embed: ModMailEmbedHandler.GetReplyEmbed(message, "Initial message from user"));
        AddModMailMessage(channel.Id, channelMessage, null, message.Author.Id);
        await message.Author.SendMessageAsync(embed: ModMailEmbedHandler.GetInitialUserReply(message));

        ModMailThread modmailThread;
        using(var db = new Database())
        {
            modmailThread = db.ModMailThreads.Where(th => th.ChannelId == channel.Id).First(); 
        }

        var modQueue = guild.GetTextChannel(Global.PostTargets[PostTarget.MODMAIL_NOTIFICATION]);
        var staffRole = guild.GetRole(Global.Roles["staff"]);
        await staffRole.ModifyAsync(x => x.Mentionable = true);
        try 
        {
            await modQueue.SendMessageAsync(staffRole.Mention, embed: ModMailEmbedHandler.GetModqueueNotificationEmbed(message.Author, modmailThread));
        }
        finally 
        {
            await staffRole.ModifyAsync(x => x.Mentionable = false);
        }
      
    }

    private async Task<RestTextChannel> CreateModMailThread(IUser targetUser){
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        var modmailCategory = guild.GetCategoryChannel(Global.ModmailCategoryId);
        var channel = await guild.CreateTextChannelAsync(targetUser.Username + targetUser.Discriminator, (TextChannelProperties prop) => {
            prop.CategoryId = modmailCategory.Id;
        });
        var thread = new ModMailThread();
        thread.CreateDate = DateTime.Now;
        thread.ChannelId = channel.Id;
        thread.UserId = targetUser.Id;
        thread.State = "INITIAL";
        using(var db = new Database())
        {
            db.ModMailThreads.Add(thread);
            db.SaveChanges();
        }
        Global.ReloadModmailThreads();
        return channel;
    }

    public async Task HandleModMailUserReply(SocketMessage message)
    {
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        ModMailThread modMailThread;
        using(var db = new Database())
        {
            modMailThread = db.ModMailThreads
                    .Include(sub => sub.Subscriber)
                    .Where(th => th.UserId == message.Author.Id && th.State != "CLOSED")
                    .FirstOrDefault();
        }
        
        if(modMailThread != null)
        {
            UpdateModThreadUpdatedField(modMailThread.ChannelId, "USER_REPLIED");
            var mentions = new StringBuilder("");
            foreach(var subscriber in modMailThread.Subscriber)
            {
                var subscriberUser = guild.GetUser(subscriber.UserId);
                if(subscriberUser != null){
                    mentions.Append(subscriberUser.Mention);
                }
            }
            var channel = guild.GetTextChannel(modMailThread.ChannelId);
            if(channel != null)
            {
                var msg = await channel.SendMessageAsync(mentions.ToString(), embed: ModMailEmbedHandler.GetReplyEmbed(message));
                AddModMailMessage(modMailThread.ChannelId, msg, null, message.Author.Id);
            }
            var userMsg = await message.Channel.GetMessageAsync(message.Id);
            if(userMsg is RestUserMessage)
            {
                var currentMessageInUserDmChannel = userMsg as RestUserMessage;
                await currentMessageInUserDmChannel.AddReactionAsync(new Emoji("✅"));
            }
            else if(userMsg is SocketUserMessage)
            {
                var currentMessageInUserDmChannel = userMsg as SocketUserMessage;
                await currentMessageInUserDmChannel.AddReactionAsync(new Emoji("✅"));
            }
        }
    }

    public void UpdateModThreadUpdatedField(ulong channelId, string newState)
    {
        using(var db = new Database())
        {
            var threadObj = db.ModMailThreads.Where(ch => ch.ChannelId == channelId).FirstOrDefault();
            if(threadObj != null)
            {
                threadObj.UpdateDate = DateTime.Now;
                threadObj.State = newState;
            }
            db.SaveChanges();
        }
    }

    public async Task CreateModeratorReply(SocketMessage message, ISocketMessageChannel channel, SocketUser moderatorUser, string clearMessage, bool anonymous)
    {
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        var userObj = Global.ModMailThreads.Where(th => th.ChannelId == channel.Id).DefaultIfEmpty(null).First();
        UpdateModThreadUpdatedField(channel.Id, "MOD_REPLIED");
        var threadUser = guild.GetUser(userObj.UserId);
        if(threadUser != null)
        {
            var replyEmbed = ModMailEmbedHandler.GetModeratorReplyEmbed(clearMessage, "Moderator posted", message, anonymous ? null : moderatorUser);
            await AnswerUserAndLogEmbed(threadUser, channel, replyEmbed, message, anonymous);
        }
        else
        {
            await channel.SendMessageAsync("User left the server. The bot message will not reach.");
        }
      
    }

    private ModMailThread CloseThreadInDb(ISocketMessageChannel channel){
        ModMailThread threadObj;
        using(var db = new Database())
        {
            threadObj = db.ModMailThreads.Where(ch => ch.ChannelId == channel.Id).FirstOrDefault();
            if(threadObj != null)
            {
                threadObj.ClosedDate = DateTime.Now;
                threadObj.State = "CLOSED";
            }
            db.SaveChanges();
            Global.ReloadModmailThreads();
        }
        return threadObj;
    }

    /// <summary>
    /// Logs the messages between staff and user (only interactions) from the given modmail thread towards the given channel with a delay of 500
    /// </summary>
    /// <param name="modMailThread">The <see cref"OnePlusBot.Data.Models.ModMailThread"/> object containing the information of the thread</param>
    /// <param name="messagesToLog">List containing the messages between the moderators and the user which should be logged.</param>
    /// <param name="targetChannel">The <see cref"Discord.WebSocket.SocketTextChannel"/> object these messages should be logged to</param>
    /// <returns>Task</returns>
    private async Task LogModMailThreadMessagesToModmailLog(ModMailThread modMailThread, List<ThreadMessage> messagesToLog, SocketTextChannel targetChannel)
    {
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        var channel = guild.GetTextChannel(modMailThread.ChannelId);
        if(channel != null)
        {
            foreach(var msg in messagesToLog)
            {
                var msgToLog = await channel.GetMessageAsync(msg.ChannelMessageId);
                var messageUser = bot.GetGuild(Global.ServerID).GetUser(msg.UserId);
                var msgText = msg.Anonymous && messageUser != null ? Extensions.FormatUserNameDetailed(messageUser) : "";
                await targetChannel.SendMessageAsync(msgText, embed: msgToLog.Embeds.First().ToEmbedBuilder().Build());
                await Task.Delay(500);
            }
        }
       
    }

    private async Task LogClosingHeader(ModMailThread modMailThread, int messageCount, string note, SocketTextChannel modMailLogChannel, SocketUser modmailUser){
       
        var closingEmbed = ModMailEmbedHandler.GetClosingSummaryEmbed(modMailThread, messageCount, modmailUser, note);       
        await modMailLogChannel.SendMessageAsync(embed: closingEmbed);
    }

    private async Task LogDisablingHeader(ModMailThread modMailThread, int messageCount, string note, SocketTextChannel modMailLogChannel, SocketUser modmailUser, DateTime until){
       
        var closingEmbed = ModMailEmbedHandler.GetMutingSummaryEmbed(modMailThread, messageCount, modmailUser, note, until);       
        await modMailLogChannel.SendMessageAsync(embed: closingEmbed);
    }

    /// <summary>
    /// Retrieves the interactions between the moderators and staff, calls the methods reponsible for logging the interactions and deletes the channel
    /// </summary>
    /// <param name="closedThread">The <see cref"OnePlusBot.Data.Models.ModMailThread"/> object of the thread being closed</param>
    /// <param name="channel">The <see cref"Discord.WebSocket.ISocketMessageChannel"/> object in which the modmail interactions happened</param>
    /// <param name="note">Optional note used when closing the thread</param>
    /// <returns>Task containing the number of logged messages.</returns>
    private async Task<int> DeleteChannelAndLogThread(ModMailThread closedThread, ISocketMessageChannel channel, string note){
      var bot = Global.Bot;
      var guild = bot.GetGuild(Global.ServerID);
      SocketGuildUser userObj = guild.GetUser(closedThread.UserId);
      List<ThreadMessage> messagesToLog;
      using(var db = new Database())
      {
          messagesToLog = db.ThreadMessages.Where(ch => ch.ChannelId == closedThread.ChannelId).ToList();
      }
      var modMailLogChannel = guild.GetTextChannel(Global.PostTargets[PostTarget.MODMAIL_LOG]);
      await LogClosingHeader(closedThread, messagesToLog.Count(), note, modMailLogChannel, userObj);

      await LogModMailThreadMessagesToModmailLog(closedThread, messagesToLog, modMailLogChannel);
      await (channel as SocketTextChannel).DeleteAsync();
      return messagesToLog.Count();
    }

    /// <summary>
    /// Calls the methods for closing the thread in the db and logging the modmail thread. 
    /// Also sends a message to the user, in case there were interactions 
    /// </summary>
    /// <param name="channel">The <see cref"Discord.WebSocket.ISocketMessageChannel"/> object of the channel which is getting closed</param>
    /// <param name="note">The optional note which is used when closing the thread</param>
    /// <returns>Task</returns>
    public async Task CloseThread(ISocketMessageChannel channel, string note)
    { 
        var closedThread = CloseThreadInDb(channel);  
        int messagesToLogCount = await DeleteChannelAndLogThread(closedThread, channel, note);
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        SocketGuildUser userObj = guild.GetUser(closedThread.UserId);
        // only send the user a dm, in case the user initiated, if we use contact it should not happen
        if(userObj != null && messagesToLogCount > 0)
        {
            await userObj.SendMessageAsync(embed: ModMailEmbedHandler.GetClosingEmbed());
        }
    }

    /// <summary>
    /// Calls the methods for closing the thread in the db and logging the modmail thread. 
    /// </summary>
    /// <param name="channel">The <see cref"Discord.WebSocket.ISocketMessageChannel"/> object of the channel which is getting closed</param>
    /// <param name="note">The optional note which is used when closing the thread</param>
    /// <returns>Task</returns>
    public async Task CloseThreadSilently(ISocketMessageChannel channel, string note)
    { 
      var closedThread = CloseThreadInDb(channel);  
      await DeleteChannelAndLogThread(closedThread, channel, note);
    }

    

    private async Task AnswerUserAndLogEmbed(SocketUser messageTarget, ISocketMessageChannel channel, Embed embed, SocketMessage message, bool anonymous=false)
    {
        var userMsg = await messageTarget.SendMessageAsync(embed: embed);
        await message.DeleteAsync();
        var channelMsg = await channel.SendMessageAsync(embed: embed);
        AddModMailMessage(channel.Id, channelMsg, userMsg, message.Author.Id, anonymous);
    }

    public async Task EditMessage(string newText, ulong messageId, ISocketMessageChannel channel, SocketUser personEditing)
    {
        ThreadMessage messageToEdit;
        ModMailThread thread;
        using(var db = new Database())
        {
            messageToEdit =  db.ThreadMessages.Where(msg => msg.ChannelId == channel.Id && msg.UserId == personEditing.Id && msg.ChannelMessageId == messageId).FirstOrDefault();
            thread = db.ModMailThreads.Where(th => th.ChannelId == channel.Id).First();
        }
        if(messageToEdit == null)
        {
            throw new Exception("No message found to edit"); 
        }
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        var user = guild.GetUser(thread.UserId);
        if(user == null)
        {
            throw new Exception("User was not found. Probably left the guild.");
        }
        IDMChannel userDmChannel = await user.GetOrCreateDMChannelAsync();

        var channelObj = guild.GetTextChannel(channel.Id);
        if(channelObj != null)
        {
            IMessage rawChannelMessage =  await channelObj.GetMessageAsync(messageToEdit.ChannelMessageId);
            Embed newEmbed = rawChannelMessage.Embeds.First().ToEmbedBuilder().WithDescription(newText).Build();
            // these ifs are needed, because when the message is cached it remains a socket message, when its older (or the bot was restarted) its a restmessage
            if(rawChannelMessage is RestUserMessage)
            {
                var currentMessageInChannel = rawChannelMessage as RestUserMessage;
                await currentMessageInChannel.ModifyAsync(msg => msg.Embed = newEmbed);
            }
            else if(rawChannelMessage is SocketUserMessage)
            {
                var currentMessageInChannel = rawChannelMessage as SocketUserMessage;
                await currentMessageInChannel.ModifyAsync(msg => msg.Embed = newEmbed);
            }

             IMessage rawDmMessage = await userDmChannel.GetMessageAsync(messageToEdit.UserMessageId);
            if(rawDmMessage is RestUserMessage)
            {
                var currentMessageInUserDmChannel = rawDmMessage as RestUserMessage;
                await currentMessageInUserDmChannel.ModifyAsync(msg => msg.Embed = newEmbed);
            }
            else if(rawDmMessage is SocketUserMessage)
            {
                var currentMessageInUserDmChannel = rawDmMessage as SocketUserMessage;
                await currentMessageInUserDmChannel.ModifyAsync(msg => msg.Embed = newEmbed);
            }
        }
    }

       


    private void AddModMailMessage(ulong channelId, RestUserMessage channelMessageId, IUserMessage userMessage, ulong userId, bool anonymous=false)
    {
        var msg = new ThreadMessage();
        msg.ChannelId = channelId;
        msg.ChannelMessageId = channelMessageId.Id;
        msg.UserMessageId = userMessage != null ? userMessage.Id : 0;
        msg.UserId = userId;
        msg.Anonymous = anonymous;
        using(var db = new Database())
        {
            db.ThreadMessages.Add(msg);
            db.SaveChanges();
        }
    }

    public void DisableModMailForUserWithExistingThread(ISocketMessageChannel channel, DateTime until){
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        using(var db = new Database()){
            var thread = db.ModMailThreads.Where(ch => ch.ChannelId == channel.Id).First();
            var user = db.Users.Where(us => us.Id == thread.UserId).First();
            user.ModMailMuted = true;
            user.ModMailMutedReminded = false;
            user.ModMailMutedUntil = until;
            db.SaveChanges();
        }
        Global.ReloadModmailThreads();

    }

    public void DisableModmailForUser(IUser user, DateTime until){
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        using(var db = new Database()){
            var userInDb = db.Users.Where(us => us.Id == user.Id).FirstOrDefault();
            if(userInDb == null){
                var newUser = new User();
                newUser.Id = user.Id;
                newUser.ModMailMuted = true;
                newUser.ModMailMutedReminded = false;
                newUser.ModMailMutedUntil = until;
                db.Users.Add(newUser);
            } else {
                userInDb.ModMailMuted = true;
                userInDb.ModMailMutedReminded = false;
                userInDb.ModMailMutedUntil = until;
            }
            db.SaveChanges();
        }
        Global.ReloadModmailThreads();
    }

    public async Task LogForDisablingAction(ISocketMessageChannel channel, string note, DateTime until)
    {
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
       
        var closedthread = CloseThreadInDb(channel);

        SocketGuildUser userObj = guild.GetUser(closedthread.UserId);
         
        List<ThreadMessage> messagesToLog;
        using(var db = new Database())
        {
            messagesToLog = db.ThreadMessages.Where(ch => ch.ChannelId == closedthread.ChannelId).ToList();
        }
        var modMailLogChannel = guild.GetTextChannel(Global.PostTargets[PostTarget.MODMAIL_LOG]);
        await LogDisablingHeader(closedthread, messagesToLog.Count(), note, modMailLogChannel, userObj, until);

        await LogModMailThreadMessagesToModmailLog(closedthread, messagesToLog, modMailLogChannel);
        await (channel as SocketTextChannel).DeleteAsync();

        if(userObj != null)
        {
            await userObj.SendMessageAsync(embed: ModMailEmbedHandler.GetDisablingEmbed(until));
        }
    }

    public void EnableModmailForUser(IGuildUser user)
    {
        using(var db = new Database())
        {
            var userInDb = db.Users.Where(us => us.Id == user.Id).First();
            userInDb.ModMailMuted = false;
            db.SaveChanges();
        }
        Global.ReloadModmailThreads();
    }


    public async Task ContactUser(IGuildUser user, ISocketMessageChannel channel){
        using(var db = new Database()){
            var exists = db.ModMailThreads.Where(th => th.UserId == user.Id && th.State != "CLOSED");
            if(exists.Count() > 0){
                await channel.SendMessageAsync(embed: ModMailEmbedHandler.GetThreadAlreadyExistsEmbed(exists.First()));
                return;
            }

            var existingUser = db.Users.Where(us => us.Id == user.Id).FirstOrDefault();
            if(existingUser == null)
            {
                var newUser = new User();
                newUser.Id = user.Id;
                newUser.ModMailMuted = false;
                db.Users.Add(newUser);
            }
            else 
            {
                existingUser.ModMailMuted = false;
            }

           
            db.SaveChanges();
        
        }
        var createdChannel = await CreateModMailThread(user);
        ModMailThread createdModMailThread;
        using(var db = new Database()){
            createdModMailThread = db.ModMailThreads.Where(th => th.ChannelId == createdChannel.Id).First();
        }
        var embedContainingLink = ModMailEmbedHandler.GetThreadHasBeendCreatedEmbed(createdModMailThread);
        await channel.SendMessageAsync(embed: embedContainingLink);
    }

    public async Task DeleteMessage(ulong messageId, ISocketMessageChannel channel, SocketUser personDeleting){
        ThreadMessage messageToRemove;
        ModMailThread thread;
        using(var db = new Database())
        {
            messageToRemove =  db.ThreadMessages.Where(msg => msg.ChannelId == channel.Id && msg.UserId == personDeleting.Id && msg.ChannelMessageId == messageId).FirstOrDefault();
            thread = db.ModMailThreads.Where(th => th.ChannelId == channel.Id).First();
        }
        if(messageToRemove == null)
        {
            throw new Exception("No message found to delete"); 
        }
        await DeleteMessageInThread(thread, messageToRemove);
       
    }

    private async Task DeleteMessageInThread(ModMailThread thread, ThreadMessage message, bool deleteMessage=true){
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        var user = guild.GetUser(thread.UserId);
        if(user == null)
        {
            throw new Exception("User was not found. Probably left the guild.");
        }
        IDMChannel userDmChannel = await user.GetOrCreateDMChannelAsync();
        var channelObj = guild.GetTextChannel(thread.ChannelId);
        if(channelObj != null)
        {
            IMessage rawChannelMessage =  await channelObj.GetMessageAsync(message.ChannelMessageId);
            if(deleteMessage)
            {
                await rawChannelMessage.DeleteAsync();
            }
        }
       

        IMessage rawDmMessage = await userDmChannel.GetMessageAsync(message.UserMessageId);

        await rawDmMessage.DeleteAsync();

        using(var db = new Database()){
            db.ThreadMessages.Remove(message);
            db.SaveChanges();
        }
    }

    public async Task<bool> DeleteMessageInThread(ulong channelId, ulong messageId, bool deleteMessage=true){
        ThreadMessage messageToRemove;
        ModMailThread thread;
        using(var db = new Database())
        {
            messageToRemove =  db.ThreadMessages.Where(msg => msg.ChannelId == channelId && msg.ChannelMessageId == messageId).OrderByDescending(msg => msg.UserMessageId).FirstOrDefault();
            thread = db.ModMailThreads.Where(th => th.ChannelId == channelId).First();
        }
        if(messageToRemove != null && messageToRemove.UserMessageId != 0)
        {
            await DeleteMessageInThread(thread, messageToRemove, deleteMessage);
            return true;
        }
        if(messageToRemove == null)
        {
            return true;
        }
        return false;
    }

   

    
  }
}