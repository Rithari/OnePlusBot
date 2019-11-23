using System.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using OnePlusBot.Data;
using OnePlusBot.Helpers;
using Discord.Commands;
using OnePlusBot.Data.Models;

namespace OnePlusBot.Base
{
  public class ReminderTimerManger 
  {
      // TODO refactor this and MuteTimerManager to have a common abstract class or at least an interface
    public async Task<RuntimeResult> SetupTimers()
    {
      await ExecuteReminderLogic(true);
      await Extensions.DelayUntilNextFullHour();
      await ExecuteReminderLogic(false);
      System.Timers.Timer timer1 = new System.Timers.Timer(1000 * 60 * 60);
      timer1.Elapsed += new System.Timers.ElapsedEventHandler(TriggerTimer);
      timer1.Enabled = true;
      return CustomResult.FromSuccess();
    }

    public async void TriggerTimer(object sender, System.Timers.ElapsedEventArgs e)
    {
      System.Timers.Timer timer = (System.Timers.Timer)sender;
      await ExecuteReminderLogic(false);
    }

    public async Task ExecuteReminderLogic(bool initialStartup)
    {
       var bot = Global.Bot;
      var guild = bot.GetGuild(Global.ServerID);
      var iGuildObj = (IGuild) guild;
      using (var db = new Database())
      {
        var maxDate = DateTime.Now.AddHours(1);
        var allusers = await iGuildObj.GetUsersAsync();
        List<Reminder> remindersInFuture;
        if(initialStartup)
        {
          remindersInFuture =  db.Reminders.Where(x => x.TargetDate < maxDate && !x.Reminded).ToList(); 
        } 
        else 
        {
          remindersInFuture=  db.Reminders.Where(x => x.TargetDate < maxDate && !x.Reminded && !x.ReminderScheduled).ToList(); 
        }
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
    }

    public static async void RemindUserIn(ulong userId, TimeSpan time, ulong reminderId)
    {
      await Task.Delay((int)time.TotalMilliseconds);
      await RemindUser(userId, reminderId);
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
            var builder = new EmbedBuilder();
            builder.Color = new Color(0x3E518);
            builder.WithAuthor(author =>
            {
                author.WithName("⏰ Reminder");
            });
            builder.AddField("Duration", Extensions.FormatTimeSpan(reminderObj.TargetDate - reminderObj.ReminderDate))
                    .AddField("Note", reminderObj.RemindText)
                    .AddField("Link", Extensions.GetMessageUrl(guild.Id, reminderObj.ChannelId, reminderObj.MessageId, "Jump!"));
            var embed = builder.Build();
            var channel = guild.GetTextChannel(reminderObj.ChannelId);
            if(channel != null)
            {
              await channel.SendMessageAsync(user.Mention, embed: embed).ConfigureAwait(false);
            } 
            
        }
        reminderObj.Reminded = true;
        db.SaveChanges();
      }
    }

    public static void SetRemindersToReminded(ulong userId, Database db)
    {
      var reminderObjs = db.Reminders.Where(x => x.RemindedUserId == userId).ToList();
      foreach(var reminderEl in reminderObjs)
      {
        reminderEl.Reminded = true;
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
