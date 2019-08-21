using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using OnePlusBot.Data;
using OnePlusBot.Helpers;
using Discord.Commands;
using Discord.WebSocket;

namespace OnePlusBot.Base
{
  public class MuteTimerManager 
  {
    public static async Task<RuntimeResult> SetupTimers(Boolean startup)
    {
      var bot = Global.Bot;
      var guild = bot.GetGuild(Global.ServerID);
      var iGuildObj = (IGuild) guild;
      using (var db = new Database())
      {
        var maxDate = DateTime.Now.AddHours(1);
        var allusers = await iGuildObj.GetUsersAsync();
        var mutesInFuture = db.Mutes.Where(x => x.UnmuteDate < maxDate && !x.MuteEnded).ToList();
        if(mutesInFuture.Any())
        {
          foreach (var futureUnmute in mutesInFuture)
          {
            var userObj = allusers.FirstOrDefault(x => x.Id == futureUnmute.MutedUserID);
            if(userObj == null)
            {
              UnMuteUserCompletely(futureUnmute.MutedUserID, db);
              continue;
            }
            await Task.Delay(1 * 1000);
            var timeToUnmute = futureUnmute.UnmuteDate - DateTime.Now;
            if(timeToUnmute.TotalMilliseconds < 0)
            {
              timeToUnmute = TimeSpan.FromSeconds(1);
            }
            // the reason why I am dragging the IDs into the function call is to be sure, that the objects are still valid when the unmute function is executed
            UnmuteUserIn(userObj.Id, timeToUnmute, futureUnmute.ID);
          }
        }    
        db.SaveChanges();
      }
      if(startup)
      {
        System.Timers.Timer timer1 = new System.Timers.Timer(1000 * 60 * 60);
        timer1.Elapsed += new System.Timers.ElapsedEventHandler(TriggerTimer);
        timer1.Enabled = true;
      }
      return CustomResult.FromSuccess();
    }

    public static async void TriggerTimer(object sender, System.Timers.ElapsedEventArgs e)
    {
      System.Timers.Timer timer = (System.Timers.Timer)sender;
      await SetupTimers(false);
    }        

    public static async void UnmuteUserIn(ulong userId, TimeSpan time, ulong muteId)
    {
      await Task.Delay((int)time.TotalMilliseconds);
      await UnMuteUser(userId, muteId);
    }

    public static async Task<CustomResult> UnMuteUser(ulong userId, ulong muteId)
    {
      var bot = Global.Bot;
      var guild = bot.GetGuild(Global.ServerID);
      var user = guild.GetUser(userId);
      if(user == null)
      {
        UnMuteUserCompletely(userId);
        return CustomResult.FromError("User is not in the server anymore");
      }
      var result = await OnePlusBot.Helpers.Extensions.UnMuteUser(user);
      if(!result.IsSuccess)
      {
        return result;
      }
      using (var db = new Database())
      {
        if(muteId == UInt64.MaxValue)
        {
          // this is in case we directly unmute a person via a command, just set all of the mutes to ended, in case there are more than one
          UnMuteUserCompletely(userId, db);
        }
        else 
        {
          var muteObj = db.Mutes.Where(x => x.ID == muteId).ToList().First();
          if(!muteObj.MuteEnded)
          {
            muteObj.MuteEnded = true;
            var noticeEmbed = new EmbedBuilder();
            noticeEmbed.Color = Color.LightOrange;
            noticeEmbed.Title = "User has been unmuted!";
            noticeEmbed.ThumbnailUrl = user.GetAvatarUrl();

            noticeEmbed.AddField("Unmuted User", OnePlusBot.Helpers.Extensions.FormatUserName(user))
                        .AddField("Mute Id", muteId)
                        .AddField("Muted since", $"{ muteObj.MuteDate:dd.MM.yyyy HH:mm}");
            await guild.GetTextChannel(Global.Channels["modlog"]).SendMessageAsync(embed: noticeEmbed.Build());
          }
        }
        db.SaveChanges();
      }
      return result;
    }

    public static void UnMuteUserCompletely(ulong userId, Database db)
    {
      var mutedObjs = db.Mutes.Where(x => x.MutedUserID == userId).ToList();
      foreach(var mutedEl in mutedObjs)
      {
        mutedEl.MuteEnded = true;
      }
    }

    public static void UnMuteUserCompletely(ulong userId)
    {
      using (var db = new Database())
      {
        UnMuteUserCompletely(userId, db);
        db.SaveChanges();
      }
    }
  }
}