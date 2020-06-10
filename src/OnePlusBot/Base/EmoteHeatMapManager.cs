using Discord;
using Discord.Rest;
using System;
using System.Threading.Tasks;
using System.Linq;
using OnePlusBot.Data.Models;
using OnePlusBot.Data;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Discord.WebSocket;
using OnePlusBot.Base.Errors;
using System.Collections.Generic;

namespace OnePlusBot.Base
{
  public class EmoteHeatMapManager 
  {

    /// <summary>
    /// Starts the timer to be executed at :30 of every minute to be done for the past minute
    /// </summary>
    public async Task<RuntimeResult> SetupTimers()
    {
      TimeSpan sinceMidnight = DateTime.Now.TimeOfDay;
      TimeSpan nextMinute = TimeSpan.FromMinutes(Math.Ceiling(sinceMidnight.TotalMinutes));
      TimeSpan timeSpanToDelay = (nextMinute - sinceMidnight);
      // dont trigger exactly on the zero second, but on the :30 second and do the minute before
      int secondsToDelay = (int) timeSpanToDelay.TotalSeconds + 30;
      await Task.Delay(secondsToDelay * 1000);
      await PersistEmotes();
      System.Timers.Timer timer = new System.Timers.Timer(1000 * 60 * 1);
      timer.Elapsed += new System.Timers.ElapsedEventHandler(TriggerPersitence);
      timer.Enabled = true;
      return CustomResult.FromSuccess();
    }

    /// <summary>
    /// Executor for the persistence
    /// </summary>
    public async void TriggerPersitence(object sender, System.Timers.ElapsedEventArgs e)
    {
      await PersistEmotes();
    }  

    /// <summary>
    /// Goes over the past minutes, and stores the used emotes in the heatmap. This removes the old entires from the runtime.
    /// </summary>
    /// <returns></returns>
    public async Task PersistEmotes()
    {
      using(var db = new Database())
      {
        var minuteToPersist = (long) DateTime.Now.Subtract(DateTime.MinValue).TotalMinutes - 1;
        Console.WriteLine($"Persisting emotes for minute {minuteToPersist} + {DateTime.Now}");
        // if for some reason the elements were not persisted in the past rounds
        // they will in the future, and because they are removed afterwards, this means that there *should* not be anything done twice
        var minutesInThePast = Global.RuntimeEmotes.Keys.Where(minute => minute <= minuteToPersist);
        List<long> toRemove = new List<long>();
        if(minutesInThePast.Any())
        {
          foreach(var processedMinute in minutesInThePast)
          {
            StoreEmotesForMinute(Global.RuntimeEmotes[processedMinute], db);
            toRemove.Add(processedMinute);
          }
        }
        foreach(var minuteToRemove in toRemove)
        {
          Global.RuntimeEmotes.TryRemove(minuteToRemove, out _);
        }
        db.SaveChanges();
      }
    }

    /// <summary>
    /// Stores the given used emotes in the database
    /// </summary>
    /// <param name="emotesToUpdate">List of emoteId/usageCount pairs to be stored</param>
    /// <param name="db">Database context</param>
    public void StoreEmotesForMinute(List<KeyValuePair<uint, uint>> emotesToUpdate, Database db)
    {
      var updateDate = DateTime.Now;
      foreach(var emotePair in emotesToUpdate)
      {
        var emote = db.EmoteHeatMap.AsQueryable().Where(e => e.EmoteReference.ID == emotePair.Key && e.UpdateDate.Date == updateDate.Date);
        if(emote.Any())
        {
          var emoteToChange = emote.First();
          emoteToChange.UsageCount += emotePair.Value;
        }
        else
        {
          var emoteToChange = new EmoteHeatMap();
          emoteToChange.Emote = emotePair.Key;
          emoteToChange.UsageCount = emotePair.Value;
          emoteToChange.UpdateDate = DateTime.Now;
          db.EmoteHeatMap.Add(emoteToChange);
        }
      }
      db.SaveChanges();
    }
  }
}