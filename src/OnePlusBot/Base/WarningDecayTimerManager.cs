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

using System.Collections.Generic;

namespace OnePlusBot.Base
{
  public class WarningDecaytimerManager 
  {
      // TODO refactor this and MuteTimerManager to have a common abstract class or at least an interface
    public static async Task<RuntimeResult> SetupTimers()
    {
      TimeSpan thisMidnight = DateTime.Today.AddDays(1) - DateTime.Now;
      int secondsToDelay = (int) thisMidnight.TotalSeconds;
      await Task.Delay(secondsToDelay * 1000);
      System.Timers.Timer timer = new System.Timers.Timer(1000 * 60 * 60 * 24);
      timer.Elapsed += new System.Timers.ElapsedEventHandler(TriggerDecay);
      timer.Enabled = true;
      return CustomResult.FromSuccess();
    }

    public static async void TriggerDecay(object sender, System.Timers.ElapsedEventArgs e)
    {
      System.Timers.Timer timer = (System.Timers.Timer)sender;
      await DecayWarnings();
    }        

    public static async Task DecayWarnings()
    {
      var bot = Global.Bot;
      var guild = bot.GetGuild(Global.ServerID);
      var iGuildObj = (IGuild) guild;
      var decayedWarnings = new Collection<WarnEntry>();
      using (var db = new Database())
      {
        var now = DateTime.Now;
        var maxDate = now.AddDays(-(double)Global.DecayDays);
        var warningsToDecay = db.Warnings.Where(x => x.Date < maxDate && !x.Decayed).ToList();
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

    private static async Task ReportDecay(Collection<WarnEntry> warnings)
    {
      StringBuilder builder = new StringBuilder("");
      var guild = Global.Bot.GetGuild(Global.ServerID);
      foreach(var warning in warnings)
      {
        var user = guild.GetUser(warning.WarnedUserID);
        var staff = guild.GetUser(warning.WarnedByID); 
        builder.Append($"Warning towards {Extensions.FormatUserName(user)} on {warning.Date} with the reason '{warning.Reason}' by staff member {Extensions.FormatUserName(staff)}. \n \n");
      }
      if(builder.ToString() == string.Empty)
      {
        builder.Append("No warnings to decay");
      }
                
                
      var embed = new EmbedBuilder
      {
          Color = Color.Blue,
          Title = $"Warnings have been decayed",
          Description = builder.ToString(),
          Timestamp = DateTime.Now
      };

      await guild.GetTextChannel(Global.Channels["modlog"]).SendMessageAsync(embed: embed.Build());
    }

  
  }
}