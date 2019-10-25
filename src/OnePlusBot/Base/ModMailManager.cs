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
        using(var db = new Database())
        {
            var user = db.Users.Where(us => us.UserId == message.Author.Id).FirstOrDefault();
            if(user != null)
            {
                if(user.ModMailMuted && user.ModMailMutedUntil > DateTime.Now)
                {
                    await message.Channel.SendMessageAsync($"You are unable to contact modmail until {user.ModMailMutedUntil:dd.MM.yyyy HH:mm} {TimeZoneInfo.Local}.");
                    return;
                }
            }
            else
            {
                var newUser = new User();
                newUser.UserId = message.Author.Id;
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

        var modQueue = guild.GetTextChannel(Global.Channels["modqueue"]);
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
        Global.ModMailThreads.Add(thread);
        using(var db = new Database())
        {
            db.ModMailThreads.Add(thread);
            db.SaveChanges();
        }
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
                mentions.Append(guild.GetUser(subscriber.UserId).Mention);
            }
            var msg = await guild.GetTextChannel(modMailThread.ChannelId).SendMessageAsync(mentions.ToString(), embed: ModMailEmbedHandler.GetReplyEmbed(message));
            AddModMailMessage(modMailThread.ChannelId, msg, null, message.Author.Id);
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
        var replyEmbed = ModMailEmbedHandler.GetModeratorReplyEmbed(clearMessage, "Moderator replied", message, anonymous ? null : moderatorUser);
        await AnswerUserAndLogEmbed(threadUser, channel, replyEmbed, message, anonymous);
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
            Global.ModMailThreads.Where(th => th.ChannelId == channel.Id).First().State  = "CLOSED";
        }
        return threadObj;
    }

    private async Task LogModMailThreadMessagesToModmailLog(ModMailThread modMailThread, string note, List<ThreadMessage> messagesToLog, SocketTextChannel targetChannel)
    {
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        var channel = guild.GetTextChannel(modMailThread.ChannelId);
        foreach(var msg in messagesToLog)
        {
            var msgToLog = await channel.GetMessageAsync(msg.ChannelMessageId);
            var messageUser = bot.GetUser(msg.UserId);
            var msgText = msg.Anonymous ? Extensions.FormatUserNameDetailed(messageUser) : "";
            await targetChannel.SendMessageAsync(msgText, embed: msgToLog.Embeds.First().ToEmbedBuilder().Build());
            await Task.Delay(500);
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

    public async Task CloseThread(SocketMessage message, ISocketMessageChannel channel, SocketUser moderatorUser, string note)
    { 
        var closedthread = CloseThreadInDb(channel);   
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        SocketGuildUser userObj = guild.GetUser(closedthread.UserId);
        List<ThreadMessage> messagesToLog;
        using(var db = new Database())
        {
            messagesToLog = db.ThreadMessages.Where(ch => ch.ChannelId == closedthread.ChannelId).ToList();
        }
        var modMailLogChannel = guild.GetTextChannel(Global.Channels["modmaillog"]);
        await LogClosingHeader(closedthread, messagesToLog.Count(), note, modMailLogChannel, bot.GetUser(closedthread.UserId));

        await LogModMailThreadMessagesToModmailLog(closedthread, note, messagesToLog, modMailLogChannel);
        await (channel as SocketTextChannel).DeleteAsync();

        await userObj.SendMessageAsync(embed: ModMailEmbedHandler.GetClosingEmbed());
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
        var channelObj = guild.GetTextChannel(channel.Id);
        IDMChannel userDmChannel = await user.GetOrCreateDMChannelAsync();
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
            var user = db.Users.Where(us => us.UserId == thread.UserId).First();
            user.ModMailMuted = true;
            user.ModMailMutedUntil = until;
            db.SaveChanges();
        }
    }

    public void DisableModmailForUser(IUser user, DateTime until){
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        using(var db = new Database()){
            var userInDb = db.Users.Where(us => us.UserId == user.Id).FirstOrDefault();
            if(userInDb == null){
                var newUser = new User();
                newUser.UserId = user.Id;
                newUser.ModMailMuted = true;
                newUser.ModMailMutedUntil = until;
                db.Users.Add(newUser);
            } else {
                userInDb.ModMailMuted = true;
                userInDb.ModMailMutedUntil = until;
            }
            db.SaveChanges();
        }
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
        var modMailLogChannel = guild.GetTextChannel(Global.Channels["modmaillog"]);
        await LogDisablingHeader(closedthread, messagesToLog.Count(), note, modMailLogChannel, bot.GetUser(closedthread.UserId), until);

        await LogModMailThreadMessagesToModmailLog(closedthread, note, messagesToLog, modMailLogChannel);
        await (channel as SocketTextChannel).DeleteAsync();

        await userObj.SendMessageAsync(embed: ModMailEmbedHandler.GetDisablingEmbed(until));
    }

    public void EnableModmailForUser(IGuildUser user)
    {
        using(var db = new Database()){
            var userInDb = db.Users.Where(us => us.UserId == user.Id).First();
            userInDb.ModMailMuted = false;
            db.SaveChanges();
        }
    }


    public async Task ContactUser(IGuildUser user, ISocketMessageChannel channel){
        using(var db = new Database()){
            var exists = db.ModMailThreads.Where(th => th.UserId == user.Id && th.State != "CLOSED");
            if(exists.Count() > 0){
                await channel.SendMessageAsync(embed: ModMailEmbedHandler.GetThreadAlreadyExistsEmbed(exists.First()));
                return;
            }

            var existingUser = db.Users.Where(us => us.UserId == user.Id).FirstOrDefault();
            if(existingUser == null)
            {
                var newUser = new User();
                newUser.UserId = user.Id;
                newUser.ModMailMuted = false;
                db.Users.Add(newUser);
            }
            else 
            {
                existingUser.ModMailMuted = false;
            }

           
            db.SaveChanges();
        
        }
        await CreateModMailThread(user);
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
        var channelObj = guild.GetTextChannel(thread.ChannelId);
        IDMChannel userDmChannel = await user.GetOrCreateDMChannelAsync();
        IMessage rawChannelMessage =  await channelObj.GetMessageAsync(message.ChannelMessageId);
        if(deleteMessage)
        {
            await rawChannelMessage.DeleteAsync();
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