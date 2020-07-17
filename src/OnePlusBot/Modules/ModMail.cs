using System;
using System.Linq;
using Discord.Commands;
using OnePlusBot.Base;
using System.Threading.Tasks;
using OnePlusBot.Helpers;
using OnePlusBot.Data.Models;
using OnePlusBot.Data;
using Discord;
using System.Runtime.InteropServices;

namespace OnePlusBot.Modules
{
    [
      Summary("Module containing commands to manage modmail threads. Some commands are only available within a modmail thread.")
    ]
    public class ModMail : ModuleBase<SocketCommandContext>
    {

        [
          Command("reply", RunMode = RunMode.Async),
          Summary("Reply to a modmail thread"),
          RequireRole("staff"),
          RequireModMailContext
        ]
        public async Task<RuntimeResult> ReplyToModMail([Remainder] string message)
        {
          await new ModMailManager().CreateModeratorReply(Context.Message, Context.Channel, Context.User, message, false, true);
          return CustomResult.FromIgnored();
        }

        [
          Command("anonreply", RunMode = RunMode.Async),
          Summary("Reply to a modmail thread anonymously"),
          RequireRole("staff"),
          RequireModMailContext
        ]
        public async Task<RuntimeResult> ReplyToModMailAnonymously([Remainder] string message)
        {
          await new ModMailManager().CreateModeratorReply(Context.Message, Context.Channel, Context.User, message, true, true);
          return CustomResult.FromIgnored();
        }

        [
            Command("subscribe", RunMode=RunMode.Async),
            Summary("Get future notifications about replies in a thread"),
            RequireRole("staff"),
            RequireModMailContext
        ]
        public async Task<RuntimeResult> SubscribeToThread()
        {
          var bot = Global.Bot;
          using(var db = new Database())
          {
            var existing = db.ThreadSubscribers.AsQueryable().Where(sub => sub.ModMailThreadId == Context.Channel.Id && sub.UserId == Context.User.Id);
            if(existing.Count() > 0)
            {
                return CustomResult.FromError("You are already subscribed!");
            }
          }
          var subscription = new ThreadSubscriber();
          subscription.ModMailThreadId = Context.Channel.Id;
          subscription.UserId = Context.User.Id;
          using(var db = new Database())
          {
            db.ThreadSubscribers.Add(subscription);
            db.SaveChanges();
          }
          await Task.CompletedTask;
          return CustomResult.FromSuccess();
        }

        [
          Command("unsubscribe", RunMode=RunMode.Async),
          Summary("Do not get further information about a modmail thread"),
          RequireRole("staff"),
          RequireModMailContext
        ]
        public async Task<RuntimeResult> UnsubscribeFromThread()
        {
          var bot = Global.Bot;
          using(var db = new Database())
          {
            var existing = db.ThreadSubscribers.AsQueryable().Where(sub => sub.ModMailThreadId == Context.Channel.Id && sub.UserId == Context.User.Id);
            if(existing.Count() == 0)
            {
              return CustomResult.FromError("You are not subscribed!");
            }
          }
          using(var db = new Database())
          {
            var existing = db.ThreadSubscribers.AsQueryable().Where(sub => sub.ModMailThreadId == Context.Channel.Id && sub.UserId == Context.User.Id).First();
            db.ThreadSubscribers.Remove(existing);
            db.SaveChanges();
          }
          await Task.CompletedTask;
          return CustomResult.FromSuccess();
        }


        /// <summary>
        /// Closes the thread with the given note
        /// </summary>
        /// <param name="note">Note to be used for logging</param>
        /// <returns>Result whether or not the closing was successful</returns>
        [
          Command("close", RunMode = RunMode.Async),
          Summary("Closes the modmail thread"),
          RequireRole("staff"),
          RequireBotPermission(GuildPermission.ManageChannels),
          RequireModMailContext
        ]
        public async Task<RuntimeResult> CloseThread([Optional] [Remainder] string note)
        {
          await new ModMailManager().CloseThread(Context.Channel, note);
          return CustomResult.FromIgnored();
        }

        /// <summary>
        /// Closes the thread silently with the given note
        /// </summary>
        /// <param name="note">Note to be used for logging</param>
        /// <returns>Result whether or not the closing was succesful</returns>
        [
          Command("closeSilently", RunMode = RunMode.Async),
          Summary("Closes the thread without notifying the user"),
          RequireRole("staff"),
          RequireModMailContext
        ]
        public async Task<RuntimeResult> CloseThreadSilently([Optional] [Remainder] string note)
        {
            await new ModMailManager().CloseThreadSilently(Context.Channel, note);
            return CustomResult.FromIgnored();
        }

        [
          Command("edit", RunMode = RunMode.Async),
          Summary("edits your message in the modmail thread"),
          RequireRole("staff"),
          RequireBotPermission(GuildPermission.ManageMessages),
          RequireModMailContext
        ]
        public async Task<RuntimeResult> EditMessage(params string[] parameters)
        {
          if(parameters.Length < 2){
              return CustomResult.FromError("Required parameters: <messageId> <new text>");
          }
          ulong messageId = (ulong) Convert.ToUInt64(parameters[0]);
          string newText = "";

          string[] reasons = new string[parameters.Length -1];
          Array.Copy(parameters, 1, reasons, 0, parameters.Length - 1);
          newText = string.Join(" ", reasons);

          await new  ModMailManager().EditMessage(newText, messageId , Context.Channel, Context.User);
          await Context.Message.DeleteAsync();
          return CustomResult.FromIgnored();
        }

        /// <summary>
        /// Disables modmail for the user the thread is about and closes the thread. This only works within a modmail thread
        /// </summary>
        /// <param name="duration">The duration for which the modmail functionality should be disabled for</param>
        /// <param name="note">Optional note which should accompany the logging</param>
        /// <returns></returns>
        [
          Command("disableThread", RunMode = RunMode.Async),
          Summary("Disables and closes the modmail thread for a certain time period. The user will be notified and will be able to contact modmail after the period again."),
          RequireRole("staff"),
          RequireBotPermission(GuildPermission.ManageChannels),
          RequireModMailContext
        ]
        public async Task<RuntimeResult> DisableCurrentThread(string duration, [Remainder] [Optional] string note)
        {
          TimeSpan mutedTime = Extensions.GetTimeSpanFromString(duration);
          var until = DateTime.Now + mutedTime;
          var manager = new ModMailManager();
          await manager.LogForDisablingAction(Context.Channel, note, until, Context.User);
          manager.DisableModMailForUserWithExistingThread(Context.Channel, until);
          return CustomResult.FromIgnored();
        }


        /// <summary>
        /// Disables modmail for a user outside of a thread. Does not require a note and sends a notification.
        /// </summary>
        /// <param name="user">User to disable modmail for</param>
        /// <param name="duration">The duration to disable modmail for</param>
        /// <returns></returns>
        [
          Command("disableModmail"),
          Summary("Disables modmail for a certain time period. The target user will be able to contact modmail after the time has been reached."),
          RequireRole("staff"),
          CommandDisabledCheck
        ]
        public async Task<RuntimeResult> DisableModMailForUser(IGuildUser user, [Remainder] string duration)
        {
          if(duration is null)
          {
            return CustomResult.FromError("You need to provide a duration.");
          }
          TimeSpan mutedTime = Extensions.GetTimeSpanFromString(duration);
          var until = DateTime.Now + mutedTime;
          new ModMailManager().DisableModmailForUser(user, until);
          await new ModMailManager().SendModmailMuteNofication(user, Context.User, until);
          return CustomResult.FromSuccess();
        }

        [
          Command("enableModmail"),
          Summary("Enables the modmail for a certain user immediatelly. No-op on users who have access."),
          RequireRole("staff"),
          CommandDisabledCheck
        ]
        public async Task<RuntimeResult> EnableModmailForUser(IGuildUser user)
        {
          new  ModMailManager().EnableModmailForUser(user);
          await Task.CompletedTask;
          return CustomResult.FromSuccess();
        }

        /// <summary>
        /// Opens a modmail thread for the given user if it doesnt exist. Posts a message linking to the existing one, if it already exists
        /// </summary>
        /// <param name="user">The <see cref="Discord.IGuildUser"> object to create the modmail thread for</param>
        /// <returns>Result whether or not the closing was succesful</returns>
        [
          Command("contact"),
          Summary("Opens a thread with the specified user"),
          RequireRole("staff"),
          RequireBotPermission(GuildPermission.ManageChannels),
          CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ContactUser(IGuildUser user)
        {
          var existing = ModMailManager.GetOpenModmailForUser(user);
          if(existing != null)
          {
            await Context.Channel.SendMessageAsync(embed: ModMailEmbedHandler.GetThreadAlreadyExistsEmbed(existing));
          }
          else
          {
            await ModMailManager.ContactUser(user, Context.Channel, true);
          }
          return CustomResult.FromSuccess();
        }

        [
            Command("delete"),
            Summary("Deletes your message within a modmail thread"),
            RequireRole("staff"),
            RequireBotPermission(GuildPermission.ManageMessages),
            RequireModMailContext
        ]
        public async Task<RuntimeResult> DeleteMessage(params string[] parameters)
        {
          if(parameters.Length < 1){
            return CustomResult.FromError("Required parameter: <messageId>");
          }
          ulong messageId = (ulong) Convert.ToUInt64(parameters[0]);
          await new  ModMailManager().DeleteMessage(messageId, Context.Channel, Context.User);
          return CustomResult.FromSuccess();
        }

        [
          Command("nicknameRequest"),
          Summary("Responds in a modmail thread with a defined response and start a timer for a defined duration."),
          RequireRole("staff"),
          RequireModMailContext,
          Alias("nickRe")
        ]
        public async Task<RuntimeResult> PostNicknameResponse() {

          await new ModMailManager().RespondWithUsernameTemplateAndSetReminder(Context.Channel, Context.User, Context.Message);
          return CustomResult.FromSuccess();
        }


    }
}