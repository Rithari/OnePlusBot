using System.Text;
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
  public class ReminderTimerManger 
  {
      // TODO refactor this and MuteTimerManager to have a common abstract class or at least an interface
    public static async Task<RuntimeResult> SetupTimers(Boolean startup)
    {
      var bot = Global.Bot;
      var guild = bot.GetGuild(Global.ServerID);
      var iGuildObj = (IGuild) guild;
      using (var db = new Database())
      {
        var maxDate = DateTime.Now.AddHours(1);
        var allusers = await iGuildObj.GetUsersAsync();
        var remindersInFuture = db.Reminders.Where(x => x.TargetDate < maxDate && !x.Reminded).ToList();
        if(remindersInFuture.Any())
        {
          foreach (var futureReminder in remindersInFuture)
          {
            var userObj = allusers.FirstOrDefault(x => x.Id == futureReminder.RemindedUserId);
            if(userObj == null)
            {
              SetRemindersToReminded(futureReminder.RemindedUserId, db);
              continue;
            }
            await Task.Delay(1 * 1000);
            var timeToRemind = futureReminder.TargetDate - DateTime.Now;
            if(timeToRemind.TotalMilliseconds < 0)
            {
              timeToRemind = TimeSpan.FromSeconds(1);
            }
            // the reason why I am dragging the IDs into the function call is to be sure, that the objects are still valid when the remind function is executed
            RemindUserIn(userObj.Id, timeToRemind, futureReminder.ID);
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

    public static async void RemindUserIn(ulong userId, TimeSpan time, ulong muteId)
    {
      await Task.Delay((int)time.TotalMilliseconds);
      await RemindUser(userId, muteId);
    }

    public static async Task RemindUser(ulong userId, ulong reminderId)
    {
      var bot = Global.Bot;
      var guild = bot.GetGuild(Global.ServerID);
      var user = guild.GetUser(userId);
      if(user == null)
      {
        RemoveReminderFromUser(userId);
      }
      using (var db = new Database())
      {
        var reminderObj = db.Reminders.Where(x => x.ID == reminderId).ToList().First();
        if(!reminderObj.Reminded)
        {
          var link = Extensions.GetSimpleMessageUrl(guild.Id, reminderObj.ChannelId, reminderObj.MessageId);
          link = Extensions.MakeLinkNotEmbedded(link);
          var textBuilder = new StringBuilder();
          textBuilder.Append("⏰ ");
          textBuilder.Append(user.Mention);
          textBuilder.Append(" Reminder: ");
          textBuilder.Append(reminderObj.RemindText);
          textBuilder.Append("\n");
          textBuilder.Append(link);
          await guild.GetTextChannel(reminderObj.ChannelId).SendMessageAsync(textBuilder.ToString());
        }
        reminderObj.Reminded = true;
        db.SaveChanges();
      }
    }

    public static void SetRemindersToReminded(ulong userId, Database db)
    {
      var mutedObjs = db.Reminders.Where(x => x.RemindedUserId == userId).ToList();
      foreach(var mutedEl in mutedObjs)
      {
        mutedEl.Reminded = true;
      }
    }

    public static void RemoveReminderFromUser(ulong userId)
    {
      using (var db = new Database())
      {
        SetRemindersToReminded(userId, db);
        db.SaveChanges();
      }
    }
  }
}