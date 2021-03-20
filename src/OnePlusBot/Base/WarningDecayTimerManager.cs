using System.Text;
using System;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using OnePlusBot.Data;
using Discord.Commands;
using System.Collections.ObjectModel;
using OnePlusBot.Data.Models;
using OnePlusBot.Helpers;
using Discord.Net;

namespace OnePlusBot.Base
{
  public class WarningDecaytimerManager 
  {
      // TODO refactor this and MuteTimerManager to have a common abstract class or at least an interface
    public async Task<RuntimeResult> SetupTimers()
    {
      if(CommandHandler.FeatureFlagDisabled(FeatureFlag.MODERATION)) 
      {
        return CustomResult.FromSuccess();
      }
      TimeSpan thisMidnight = DateTime.Today.AddDays(1) - DateTime.Now;
      int secondsToDelay = (int) thisMidnight.TotalSeconds;
      await Task.Delay(secondsToDelay * 1000);
      await DecayWarnings();
      System.Timers.Timer timer = new System.Timers.Timer(1000 * 60 * 60 * 24);
      timer.Elapsed += new System.Timers.ElapsedEventHandler(TriggerDecay);
      timer.Enabled = true;
      return CustomResult.FromSuccess();
    }

    public async void TriggerDecay(object sender, System.Timers.ElapsedEventArgs e)
    {
      System.Timers.Timer timer = (System.Timers.Timer)sender;
      await DecayWarnings();
    }        

    public async Task DecayWarnings()
    {
      var bot = Global.Bot;
      var guild = bot.GetGuild(Global.ServerID);
      var iGuildObj = (IGuild) guild;
      var decayedWarnings = new Collection<WarnEntry>();
      using (var db = new Database())
      {
        var now = DateTime.Now;
        var maxDate = now.AddDays(-(double)Global.DecayDays);
        var warningsToDecay = db.Warnings.AsQueryable().Where(x => x.Date < maxDate && !x.Decayed).ToList();
        if(warningsToDecay.Any())
        {
          foreach (var warning in warningsToDecay)
          {
            warning.Decayed = true;
            warning.DecayedTime = now;
            decayedWarnings.Add(warning);
          }
        }
        db.SaveChanges();
      }
      await ReportDecay(decayedWarnings);
    }

    /// <summary>
    /// Builds the embed reporting the decays and posts them in towards decaylog. Also sends earch user a notification, that a warning has been decayed.
    /// </summary>
    /// <param name="warnings">The warnings to decay.</param>
    /// <returns>Task</returns>
    private async Task ReportDecay(Collection<WarnEntry> warnings)
    {
      StringBuilder builder = new StringBuilder("");
      var guild = Global.Bot.GetGuild(Global.ServerID);
      var texts = new Collection<string>();
      foreach(var warning in warnings)
      {
        var user = guild.GetUser(warning.WarnedUserID);
        var staff = guild.GetUser(warning.WarnedByID);
        var userText = "";
        if(user != null)
        {
          userText = Extensions.FormatUserNameDetailed(user);
        }
        else
        {
          userText = warning.WarnedUserID + "";
        }
        var staffMemberText = staff != null ? Extensions.FormatUserName(staff) : warning.WarnedByID.ToString();
        var warnText = $"Warning towards {userText} on {Extensions.FormatDateTime(warning.Date)} with the reason '{warning.Reason}' by staff member {staffMemberText}. \n \n";
        if((builder.ToString() + warnText).Length > EmbedBuilder.MaxDescriptionLength)
        {
          texts.Add(builder.ToString());
          builder = new StringBuilder();
          builder.Append(warnText);
        }
        else
        {
          builder.Append(warnText);
        }
        if(user != null)
        {
          var remainingWarnings = 0;
          using(var db = new Database())
          {
            remainingWarnings = db.Warnings.AsQueryable().Where(w => w.WarnedUserID == user.Id && !w.Decayed).Count();
          }
          var message = $"Your warning from {Extensions.FormatDateTime(warning.Date)} with the reason '{warning.Reason}' has been cleared. You have {remainingWarnings} warning(s).";
          try 
          {
            await user.SendMessageAsync(message);
            await Task.Delay(500);
          }
          catch (HttpException)
          {
            Console.WriteLine($"User {user.Id} disabled DMs, unable to send message about decay.");
          }
        }
      }
      if(builder.ToString() == string.Empty)
      {
        builder.Append("No warnings to decay");
      }

      texts.Add(builder.ToString());
      var counter = 1;
      foreach(var text in texts){
        var titleAddition = counter > 1 ? "#" + counter : "";
        var title = $"Warnings have been decayed {titleAddition}";
        var embed = new EmbedBuilder
        {
          Color = Color.Gold,
          Title = title,
          Description = text,
          Timestamp = DateTime.Now
        };

        await guild.GetTextChannel(Global.PostTargets[PostTarget.DECAY_LOG]).SendMessageAsync(embed: embed.Build());
        await Task.Delay(1000);
        counter++;
      }
     
    }

  
  }
}