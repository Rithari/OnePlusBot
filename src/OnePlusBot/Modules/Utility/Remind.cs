using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using OnePlusBot.Base;
using OnePlusBot.Helpers;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using System;
using System.Collections.Generic;

namespace OnePlusBot.Modules.Utility
{
  public partial class Utility {
    [
      Command("remind"),
      Summary("Reminds you of a text after a defined time period."),
      CommandDisabledCheck
    ]
    public async Task<RuntimeResult> HandleRemindInput(params string[] arguments)
    {
      if(arguments.Length < 1)
        return CustomResult.FromError("The correct usage is `;remind <duration> <text>`");

      string durationStr = arguments[0];

      TimeSpan span = Extensions.GetTimeSpanFromString(durationStr);
      DateTime targetTime = DateTime.Now.Add(span);

      string reminderText;
      if(arguments.Length > 1)
      {
        string[] reminderParts = new string[arguments.Length -1];
        Array.Copy(arguments, 1, reminderParts, 0, arguments.Length - 1);
        reminderText = string.Join(" ", reminderParts);
      } 
      else 
      {
        return CustomResult.FromError("You need to provide a text.");
      }

      if(reminderText.Length > 1000) 
      {
        return CustomResult.FromError("Maximum length of reminder text is 1000 characters");
      }

      var author = Context.Message.Author;

      var guild = Context.Guild;

      var reminder = new Reminder();
      reminder.RemindText = Extensions.RemoveIllegalPings(reminderText);
      reminder.RemindedUserId = author.Id;
      reminder.TargetDate = targetTime;
      reminder.ReminderDate = DateTime.Now;
      reminder.ChannelId = Context.Channel.Id;
      reminder.MessageId = Context.Message.Id;
      
      using(var db = new Database())
      {
        db.Reminders.Add(reminder);
        db.SaveChanges();
        if(targetTime <= DateTime.Now.AddMinutes(60))
        {
          var difference = targetTime - DateTime.Now;
          ReminderTimerManger.RemindUserIn(author.Id, difference, reminder.ID);
          reminder.ReminderScheduled = true;
        }
        
        db.SaveChanges();
      }

      await Context.Channel.SendMessageAsync($"{Context.User.Mention} Scheduled reminder id {reminder.ID}.");

      return CustomResult.FromSuccess();
    }

    [
      Command("unremind"),
      Summary("Cancells the reminder by id. Does nothing is reminder is already cancelled."),
      CommandDisabledCheck
    ]
    public async Task<RuntimeResult> HandleUnRemindInput(ulong reminderId)
    {
      await Task.CompletedTask;
      using(var db = new Database())
      {
          var reminder = db.Reminders.Where(re => re.ID == reminderId && re.RemindedUserId == Context.User.Id).FirstOrDefault();
          if(reminder != null)
          {
            reminder.Reminded = true;
            db.SaveChanges();
            return CustomResult.FromSuccess();
          }
          else
          {
              return CustomResult.FromError("Reminder not known or not started by you.");
          }
      }
    }
    /// <summary>
    /// Posts embeds containing the currently active reminders for the user executing the command.
    /// </summary>
    /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
    [
        Command("reminders"),
        Summary("Shows the currently active reminders")
    ]
    public async Task<RuntimeResult> ShowActiveReminders()
    {
      using(var db = new Database())
      {
        var activeReminders = db.Reminders.Where(r => !r.Reminded && r.RemindedUserId == Context.User.Id);
        var currentEmbedBuilder = new EmbedBuilder();
        var embedsToPost = new List<Embed>();
        currentEmbedBuilder.WithTitle("Active reminders");
        if(activeReminders.Count() > 0)
        {
          var count = 0;
          foreach(var reminder in activeReminders)
          {
            var reminderLink = Extensions.GetMessageUrl(Global.ServerID, reminder.ChannelId, reminder.MessageId, $"**Reminder**");
            currentEmbedBuilder.AddField($"Reminder {reminder.ID}", $"{reminderLink} set on {Extensions.FormatDateTime(reminder.ReminderDate)} and due on {Extensions.FormatDateTime(reminder.TargetDate)} with text: " + reminder.RemindText, true);
            count++;
            if(((count % EmbedBuilder.MaxFieldCount) == 0) && reminder != activeReminders.Last())
            {
              embedsToPost.Add(currentEmbedBuilder.Build());
              currentEmbedBuilder = new EmbedBuilder();
              var currentPage = count / EmbedBuilder.MaxFieldCount + 1;
              currentEmbedBuilder.WithFooter(new EmbedFooterBuilder().WithText($"Page {currentPage}"));
            }
          }
          embedsToPost.Add(currentEmbedBuilder.Build());
        }
        else
        {
          currentEmbedBuilder.WithDescription("No active reminders");
          embedsToPost.Add(currentEmbedBuilder.Build());
        }
        foreach(var embed in embedsToPost)
        {
          await Context.Channel.SendMessageAsync(embed: embed);
          await Task.Delay(400);
        }
      }
      return CustomResult.FromSuccess();
    }
   }
}