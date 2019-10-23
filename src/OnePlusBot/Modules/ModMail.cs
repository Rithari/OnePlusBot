using System;
using System.Linq;
using Discord.Commands;
using OnePlusBot.Base;
using System.Threading.Tasks;
using Discord;
using OnePlusBot.Data.Models;
using OnePlusBot.Data;

namespace OnePlusBot.Modules
{
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
            await new ModMailManager().CreateModeratorReply(Context.Message, Context.Channel, Context.User, message, false);
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
            await new ModMailManager().CreateModeratorReply(Context.Message, Context.Channel, Context.User, message, true);
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
                var existing = db.ThreadSubscribers.Where(sub => sub.ModMailThreadId == Context.Channel.Id && sub.UserId == Context.User.Id);
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
                var existing = db.ThreadSubscribers.Where(sub => sub.ModMailThreadId == Context.Channel.Id && sub.UserId == Context.User.Id);
                if(existing.Count() == 0)
                {
                    return CustomResult.FromError("You are not subscribed!");
                }
            }
            using(var db = new Database())
            {
                var existing = db.ThreadSubscribers.Where(sub => sub.ModMailThreadId == Context.Channel.Id && sub.UserId == Context.User.Id).First();
                db.ThreadSubscribers.Remove(existing);
                db.SaveChanges();
            }
            return CustomResult.FromSuccess();
        }

        [
            Command("close"),
            Summary("Closes the modmail thread"),
            RequireRole("staff"),
            RequireModMailContext
        ]
        public async Task<RuntimeResult> CloseThread([Remainder] string note)
        {
            await new ModMailManager().CloseThread(Context.Message, Context.Channel, Context.User, note);
            return CustomResult.FromIgnored();
        }

        [
            Command("edit"),
            Summary("edits your *LAST* message in the modmail thread"),
            RequireRole("staff"),
            RequireModMailContext
        ]
        public async Task<RuntimeResult> EditMessage([Remainder] string newText)
        {
            await new  ModMailManager().EditLastMessage(newText, Context.Channel, Context.User);
            await Context.Message.DeleteAsync();
            return CustomResult.FromIgnored();
        }
    
    }
}