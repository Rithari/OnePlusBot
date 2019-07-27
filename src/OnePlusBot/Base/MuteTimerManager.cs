using System.Data.Common;
using System.Xml.Linq;
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
    public class MuteTimerManager {
        public static async Task<RuntimeResult> setupTimers(Boolean startup){
          var bot = Global.Bot;
          var guild = bot.GetGuild(Global.ServerID);

          await guild.GetTextChannel(Global.Channels["modlog"]).SendMessageAsync("trying to add new times, if we find any");
          var iGuildObj = (IGuild) guild;
          using (var db = new Database())
          {
            var maxDate = DateTime.Now.AddMinutes(1);
            var allusers = await iGuildObj.GetUsersAsync();
            var mutesInFuture = db.Mutes.Where(x => x.UnmuteDate < maxDate && !x.MuteEnded).ToList();
            if(mutesInFuture.Any()){
              foreach (var futureUnmute in mutesInFuture)
              {
                var userObj = allusers.FirstOrDefault(x => x.Id == futureUnmute.MutedUserID);
                await Task.Delay(1 * 1000);
                await guild.GetTextChannel(Global.Channels["modlog"]).SendMessageAsync("setting up timer to unmute " + userObj.Username);
                var timeToUnmute = futureUnmute.UnmuteDate - DateTime.Now;
                if(timeToUnmute.TotalMilliseconds < 0){
                  timeToUnmute = TimeSpan.FromSeconds(1);
                }
                // the reason why I am dragging the IDs into the function call is to be sure, that the objects are still valid when the unmute function is executed
                UnmuteUserIn(userObj.Id, timeToUnmute, futureUnmute.ID);
              }
            }    
          }
          if(startup){
            MuteTimer timer1 = new MuteTimer();
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(TriggerTimer);
            Console.WriteLine("setting up regular timer");
            timer1.Enabled = true;
          }
          return CustomResult.FromSuccess();
        }

        public static async void TriggerTimer(object sender, System.Timers.ElapsedEventArgs e){
          MuteTimer timer = (MuteTimer)sender;
          var bot = Global.Bot;
          Console.WriteLine("setting up timers");
          var guild = bot.GetGuild(Global.ServerID);
          await setupTimers(false);
        }

        

        public static async void UnmuteUserIn(ulong userId, TimeSpan time, ulong muteId){
          await Task.Delay((int)time.TotalMilliseconds);
          await UnMuteUser(userId, muteId);
        }

        public static async Task<RuntimeResult> UnMuteUser(ulong userId, ulong muteId){
          var bot = Global.Bot;
          var guild = bot.GetGuild(Global.ServerID);
          var user = guild.GetUser(userId);
          await OnePlusBot.Helpers.Extensions.UnMuteUser(user);
          using (var db = new Database())
          {
            if(muteId == UInt64.MaxValue){
              var mutedObjs = db.Mutes.Where(x => x.MutedUserID == userId).ToList();
              foreach(var mutedEl in mutedObjs){
                mutedEl.MuteEnded = true;
              }
            } else {
              var muteObj = db.Mutes.Where(x => x.ID == muteId).ToList().First();
              muteObj.MuteEnded = true;
            }
            db.SaveChanges();
            
          }
          await guild.GetTextChannel(Global.Channels["modlog"]).SendMessageAsync("Unmuting user " + user.Username);
          return null;
        }
      }
}