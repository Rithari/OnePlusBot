using System.Security.AccessControl;
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

    /// <summary>
    /// Creates a modmail thread for a user. This is initiated by the user when the user sends the bot a DM.
    /// This includes:
    /// Creating the channel, creating the records in the db and pinging staff
    /// </summary>
    /// <param name="message">The <see cref="Discord.WebSocket.SocketMessage"> object containing the message send by the user to initiate this</param>
    /// <returns>Task</returns>
    public async Task CreateModmailThread(SocketMessage message)
    {
        var userFromCache = Global.ModMailThreads.Where(th => th.UserId == message.Author.Id).FirstOrDefault();
        if(userFromCache != null &&  userFromCache.ThreadUser.ModMailMuted && userFromCache.ThreadUser.ModMailMutedUntil > DateTime.Now)
        {
            if(!userFromCache.ThreadUser.ModMailMutedReminded) 
            {
                await message.Channel.SendMessageAsync($"You are unable to contact modmail until {Extensions.FormatDateTime(userFromCache.ThreadUser.ModMailMutedUntil)}.");
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
                var newUser = new UserBuilder(message.Author.Id).Build();
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

        ModMailThread modmailThread;
        using(var db = new Database())
        {
          modmailThread = db.ModMailThreads.Where(th => th.ChannelId == channel.Id).First(); 
        }

        await channel.SendMessageAsync(embed: ModMailEmbedHandler.GetUserInfoHeader(modmailThread));
        var channelMessage = await channel.SendMessageAsync(embed: ModMailEmbedHandler.GetReplyEmbed(message, "Initial message from user"));
        AddModMailMessage(channel.Id, channelMessage, null, message.Author.Id);
        await message.Author.SendMessageAsync(embed: ModMailEmbedHandler.GetInitialUserReply(message));


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

    /// <summary>
    /// Creates the channel containing the modmail thread and creates the thread in the database
    /// </summary>
    /// <param name="targetUser">The <see cref="Discord.IUser"> user to create the modmail thread for</param>
    /// <returns>The <see chref="Discord.Rest.RestTextChannel"> newly created channel</returns>
    private static async Task<RestTextChannel> CreateModMailThread(IUser targetUser)
    {
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

    /// <summary>
    /// Handles the user reply: updates the state in the db, posts the message to the modmail thread and mentions subscribers
    /// </summary>
    /// <param name="message">The <see chref="Discord.WebSocket.SocketMessage"> message object coming from the user</param>
    /// <returns></returns>
    public async Task HandleModMailUserReply(SocketMessage message)
    {
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        var modMailThread = ModMailManager.GetOpenModmailForUser(message.Author);
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

    /// <summary>
    /// Posts the given message towards the user, sends the message in the modmail thread and updates the state field in the modmail thread in the db.
    /// </summary>
    /// <param name="message">The <see chref="Discord.WebSocket.SocketMessage"> message object used as a response.</param>
    /// <param name="channel">The <see chref="Discord.ISocketMessageChannel"> channel object representing the modmail thread.</param>
    /// <param name="moderatorUser">The <see chref="Discord.WebSocket.SocketUser"> user object representing the staff member resonding.</param>
    /// <param name="clearMessage">String containing a cleaned version of the response</param>
    /// <param name="anonymous">Boolean to define whether or not the response should be anonymous</param>
    /// <returns>The <see chref="Discord.Rest.RestUserMessage"> message object which was logged in the modmail thread</returns>
    public async Task<RestUserMessage> CreateModeratorReply(SocketMessage message, ISocketMessageChannel channel, SocketUser moderatorUser, string clearMessage, bool anonymous)
    {
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        var userObj = Global.ModMailThreads.Where(th => th.ChannelId == channel.Id).DefaultIfEmpty(null).First();
        UpdateModThreadUpdatedField(channel.Id, "MOD_REPLIED");
        var threadUser = guild.GetUser(userObj.UserId);
        if(threadUser != null)
        {
            var replyEmbed = ModMailEmbedHandler.GetModeratorReplyEmbed(clearMessage, "Moderator posted", message, anonymous ? null : moderatorUser);
            var responseInModmailThread = await AnswerUserAndLogEmbed(threadUser, channel, replyEmbed, message, anonymous);
            return responseInModmailThread;
        }
        else
        {
          throw new Exception("User left the server. The bot message will not reach.");
        }
      
    }

    /// <summary>
    /// Closes the modmail thread which is present in the given channel
    /// </summary>
    /// <param name="channel">The <see cref="Discord.WebSocket.ISocketMessageChannel"> channel object in which the modmail thread is handled</param>
    /// <returns>The <see cref="OnePlusBot.Data.Models.ModMailThread"> being closed</returns>
    private ModMailThread CloseThreadInDb(ISocketMessageChannel channel){
      ModMailThread threadObj;
      using(var db = new Database())
      {
        threadObj = db.ModMailThreads.Where(ch => ch.ChannelId == channel.Id).FirstOrDefault();
        if(threadObj != null)
        {
          if(threadObj.State == "CLOSING") {
            throw new Exception("Thread is already being closed");
          }
          threadObj.ClosedDate = DateTime.Now;
          threadObj.State = "CLOSING";
        }
        else
        {
          throw new Exception("Thread not found in the database.");
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
      using(var db = new Database())
      {
        var threadObj = db.ModMailThreads.Where(ch => ch.ChannelId == channel.Id).FirstOrDefault();
        if(threadObj != null)
        {
          threadObj.ClosedDate = DateTime.Now;
          threadObj.State = "CLOSED";
        }
        db.SaveChanges();
        Global.ReloadModmailThreads();
      }
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

    

    /// <summary>
    /// Sends the given message to the user and the modmail thread. Additionally logs the message id to the database
    /// </summary>
    /// <param name="messageTarget">The <see chref="Discord.WebSocket.SocketUser"> user object to send the message to</param>
    /// <param name="channel">The <see chref="Discord.ISocketMessageChannel"> channel object in which the message is being logged to (the modmail thread)</param>
    /// <param name="embed">The <see chref="Discord.Embed"> embed to be sent</param>
    /// <param name="message">The <see chref="Discord.WebSocket.SocketMessage"> message causing this reply</param>
    /// <param name="anonymous">Boolean to define whether or not the message should be sent anonymous</param>
    /// <returns>The <see chref="Discord.Rest.RestUserMessage"> message object which was logged to the modmail thread</returns>
    private async Task<RestUserMessage> AnswerUserAndLogEmbed(SocketUser messageTarget, ISocketMessageChannel channel, Embed embed, SocketMessage message, bool anonymous=false)
    {
        var userMsg = await messageTarget.SendMessageAsync(embed: embed);
        await message.DeleteAsync();
        var channelMsg = await channel.SendMessageAsync(embed: embed);
        AddModMailMessage(channel.Id, channelMsg, userMsg, message.Author.Id, anonymous);
        return channelMsg;
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
                var newUser = new UserBuilder(user.Id).WithModmailConfig(true, false, until).Build();
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


    /// <summary>
    /// Creates a modmail thread with the given user and responds in the given thread with a link to the newly created channel.
    /// </summary>
    /// <param name="user">The <see cref="Discord.IUser"> to create the channel for</param>
    /// <param name="channel">The <see cref="Discord.ISocketMessageChannel"> in which the response should be posted to</param>
    /// <returns>The <see cref="Discord.Rest.RestChannel"> newly created channel</returns>
    public static async Task<RestTextChannel> ContactUser(IUser user, ISocketMessageChannel channel, bool createNote)
    {
        using(var db = new Database())
        {
            var existingUser = db.Users.Where(us => us.Id == user.Id).FirstOrDefault();
            if(existingUser == null)
            {
                var newUser = new UserBuilder(user.Id).Build();
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
        using(var db = new Database())
        {
          createdModMailThread = db.ModMailThreads.Where(th => th.ChannelId == createdChannel.Id).First();
        }

        await createdChannel.SendMessageAsync(embed: ModMailEmbedHandler.GetUserInfoHeader(createdModMailThread));
        if(createNote)
        {
          var embedContainingLink = ModMailEmbedHandler.GetThreadHasBeendCreatedEmbed(createdModMailThread);
          await channel.SendMessageAsync(embed: embedContainingLink);
        }
        // we need to return the channel, because we *directly* need the channel after wards, and loading the channel by id again
        // resulted in null
        return createdChannel;
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

    /// Finds the currently open modmail thread (if it exists) for a user or return null else
    /// </summary>
    /// <param name="user">The <see cref="Discord.IUser"> user to find the modmail thread for</param>
    /// <returns>The <see cref="OnePlusBot.Data.Models.ModMailThread"> object if it exists for the user, null instead</returns>
    public static ModMailThread GetOpenModmailForUser(IUser user)
    {
        using(var db = new Database())
        {
            return db.ModMailThreads
                    .Include(sub => sub.Subscriber)
                    .Where(th => th.UserId == user.Id && th.State != "CLOSED")
                    .FirstOrDefault();
        }
    }


    /// Finds the currently open modmail thread (if it exists) for a channel or return null else
    /// </summary>
    /// <param name="user">The <see cref="Discord.IChannel"> channel in which the modmail thread happens</param>
    /// <returns>The <see cref="OnePlusBot.Data.Models.ModMailThread"> object if it exists for the channel, null instead</returns>
    public static ModMailThread GetOpenModmail(IChannel channel)
    {
        using(var db = new Database())
        {
            return db.ModMailThreads
                    .Include(sub => sub.Subscriber)
                    .Where(th => th.ChannelId == channel.Id && th.State != "CLOSED")
                    .FirstOrDefault();
        }
    }

    /// <summary>
    /// Responds to the user with a predefined template and starts a reminder with a templated reminder text for a configurable duration.
    /// </summary>
    /// <param name="channel"><see ref="Discord.ISocketMessageChannel"> Channel of the modmail thread</param>
    /// <param name="moderatorUser"><see ref="Discord.WebSocket.SocketUser"> Moderator triggering command and target of the reminder</param>
    /// <param name="message"><see ref="Discord.WebSocket.SocketMessage"> Message triggering the command.</param>
    /// <returns></returns>
    public async Task RespondWithUsernameTemplateAndSetReminder(ISocketMessageChannel channel, SocketUser moderatorUser, SocketMessage message) {
      var text = Extensions.GetTemplatedString(ResponseTemplate.ILLEGAL_NAME_RESPONSE, new object[0]);

      var thread = ModMailManager.GetOpenModmail(channel);
      var user = Global.Bot.GetUser(thread.UserId);

      var reminderText = Extensions.GetTemplatedString(ResponseTemplate.ILLEGAL_NAME_REMINDER, new object[1] { Extensions.FormatUserName(user)});
      var reponseMessageInModmailThread = await new ModMailManager().CreateModeratorReply(message, channel, moderatorUser, text, false);

      ulong seconds = 0;
      using (var db = new Database())
      {
        var point = db.PersistentData.First(entry => entry.Name == "username_reminder_duration");
        seconds = point.Value;

        var targetTime = DateTime.Now.AddSeconds(seconds);

        // TODO move to builder
        var reminder = new Reminder();
        reminder.RemindText = Extensions.RemoveIllegalPings(reminderText);
        reminder.RemindedUserId = moderatorUser.Id;
        reminder.TargetDate = targetTime;
        reminder.ReminderDate = DateTime.Now;
        reminder.ChannelId = channel.Id;
        reminder.MessageId = reponseMessageInModmailThread.Id;
    
        db.Reminders.Add(reminder);
        db.SaveChanges();
        if(targetTime <= DateTime.Now.AddMinutes(60))
        {
            var difference = targetTime - DateTime.Now;
            ReminderTimerManger.RemindUserIn(moderatorUser.Id, difference, reminder.ID);
            reminder.ReminderScheduled = true;
        }
          
        db.SaveChanges();
      }
    }
   

    
  }
}