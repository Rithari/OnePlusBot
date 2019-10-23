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

namespace OnePlusBot.Base
{
  public class ModMailManager 
  {
    public async Task CreateModmailThread(SocketMessage message)
    {
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        var modmailCategory = guild.GetCategoryChannel(Global.ModmailCategoryId);
        var channel = await guild.CreateTextChannelAsync(message.Author.Username + message.Author.Discriminator, (TextChannelProperties prop) => {
            prop.CategoryId = modmailCategory.Id;
        });
        int pastThreads = 0;
        using(var db = new Database())
        {
            pastThreads =  db.ModMailThreads.Where(ch => ch.UserId == message.Author.Id).Count();
        }
        await channel.SendMessageAsync(embed: ModMailEmbedHandler.GetUserInformation(pastThreads, message.Author));
        var channelMessage = await channel.SendMessageAsync(embed: ModMailEmbedHandler.GetReplyEmbed(message, "Initial message from user"));
        AddModMailMessage(channel.Id, channelMessage, null, message.Author.Id);
        await message.Author.SendMessageAsync(embed: ModMailEmbedHandler.GetInitialUserReply(message));
        var thread = new ModMailThread();
        thread.CreateDate = DateTime.Now;
        thread.ChannelId = channel.Id;
        thread.UserId = message.Author.Id;
        thread.State = "INITIAL";
        Global.ModMailThreads.Add(thread);
        using(var db = new Database())
        {
            db.ModMailThreads.Add(thread);
            db.SaveChanges();
        }
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
                    .Where(th => th.UserId == message.Author.Id)
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
        }
        await Task.CompletedTask;
    }

    public void UpdateModThreadUpdatedField(ulong channelId, string newState)
    {
        using(var db = new Database())
        {
            var threadObj = db.ModMailThreads.Where(ch => ch.ChannelId == channelId).FirstOrDefault();
            if(threadObj != null)
            {
                threadObj.ClosedDate = DateTime.Now;
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
        var replyEmbed = ModMailEmbedHandler.GetModeratorReplyEmbed(clearMessage, "Moderator replied", anonymous ? null : moderatorUser);
        await DeleteMessageAndMirrorEmbeds(threadUser, channel, replyEmbed, message, anonymous);
    }

    public async Task CloseThread(SocketMessage message, ISocketMessageChannel channel, SocketUser moderatorUser, string note)
    {
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        SocketGuildUser userObj;
        ModMailThread threadObj;
        using(var db = new Database())
        {
            threadObj = db.ModMailThreads.Where(ch => ch.ChannelId == channel.Id).FirstOrDefault();
            if(threadObj != null)
            {
                threadObj.ClosedDate = DateTime.Now;
                threadObj.State = "CLOSED";
            }
            userObj = guild.GetUser(threadObj.UserId);
            db.SaveChanges();
        }
        
        List<ThreadMessage> messagesToLog;
        using(var db = new Database())
        {
            messagesToLog = db.ThreadMessages.Where(ch => ch.ChannelId == channel.Id).ToList();
        }
        var modMailLogChannel = guild.GetTextChannel(Global.Channels["modmaillog"]);
        await modMailLogChannel.SendMessageAsync(embed: ModMailEmbedHandler.GetClosingSummaryEmbed(threadObj, messagesToLog.Count(), bot.GetUser(threadObj.UserId), note));
        foreach(var msg in messagesToLog)
        {
            var msgToLog = await channel.GetMessageAsync(msg.ChannelMessageId);
            var messageUser = bot.GetUser(msg.UserId);
            var msgText = msg.Anonymous ? Extensions.FormatUserNameDetailed(messageUser) : "";
            await modMailLogChannel.SendMessageAsync(msgText, embed: msgToLog.Embeds.First().ToEmbedBuilder().Build());
            await Task.Delay(100);
        }

        await (channel as SocketTextChannel).DeleteAsync();

        await userObj.SendMessageAsync(embed: ModMailEmbedHandler.GetClosingEmbed(note));
    }

    

    private async Task DeleteMessageAndMirrorEmbeds(SocketUser messageTarget, ISocketMessageChannel channel, Embed embed, SocketMessage message, bool anonymous=false)
    {
        var userMsg = await messageTarget.SendMessageAsync(embed: embed);
        await message.DeleteAsync();
        var channelMsg = await channel.SendMessageAsync(embed: embed);
        AddModMailMessage(channel.Id, channelMsg, userMsg, message.Author.Id, anonymous);
    }

    public async Task EditLastMessage(string newText, ISocketMessageChannel channel, SocketUser personEditing)
    {
        ThreadMessage messageToEdit;
        ModMailThread thread;
        using(var db = new Database())
        {
            messageToEdit =  db.ThreadMessages.Where(msg => msg.ChannelId == channel.Id && msg.UserId == personEditing.Id).OrderByDescending(msg => msg.UserMessageId).First();
            thread = db.ModMailThreads.Where(th => th.ChannelId == channel.Id).First();
        }
        var bot = Global.Bot;
        var guild = bot.GetGuild(Global.ServerID);
        var user = guild.GetUser(thread.UserId);
        var channelObj = guild.GetTextChannel(channel.Id);
        IDMChannel userDmChannel = await user.GetOrCreateDMChannelAsync();
        IMessage rawChannelMessage =  await channelObj.GetMessageAsync(messageToEdit.ChannelMessageId);
        Embed newEmbed = ModMailEmbedHandler.GetModeratorReplyEmbed(newText, "Moderator replied", messageToEdit.Anonymous ? null : personEditing);
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
   

    
  }
}