using System.Collections.ObjectModel;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using OnePlusBot.Data.Models;
using OnePlusBot.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace OnePlusBot.Base
{
    public static class Core
    {

        private static Collection<IReactionAction> AddReactionActions;

        private static Collection<IReactionAction> RemoveReactionActions;
        public static async Task Main()
        {
            var services = BuildServices();
            var bot = services.GetRequiredService<DiscordSocketClient>();
            bot.Log += Log;
            bot.ReactionAdded += OnReactionAdded;
            bot.ReactionRemoved += OnReactionRemoved;
            Func<Task> muteTimer = null;
            muteTimer = () => 
            {
                new MuteTimerManager().SetupTimers().ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
                bot.Ready -= muteTimer;
                return Task.CompletedTask;
            };
            Func<Task> remindTimer = null;
            remindTimer =  () => 
            {
                new ReminderTimerManger().SetupTimers().ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
                bot.Ready -= remindTimer;
                return Task.CompletedTask;
            };

            Func<Task> warnDecayTimer = null;
            warnDecayTimer =  () => 
            {
                new WarningDecaytimerManager().SetupTimers().ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
                bot.Ready -= warnDecayTimer;
                return Task.CompletedTask;
            };

            Func<Task> expPersister = null;
            expPersister = () => 
            {
                new ExpManager().SetupTimers().ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
                bot.Ready -= expPersister;
                return Task.CompletedTask;
            };

            bot.Ready += remindTimer;
            bot.Ready += muteTimer;
            bot.Ready += warnDecayTimer;
            bot.Ready += expPersister;
           

            if(Global.Token == string.Empty)
            {
                Console.WriteLine("Configure the token for the version you are trying to run in order to execute the bot");
                Environment.Exit(0);
            }
            try 
            {
                await bot.LoginAsync(TokenType.Bot, Global.TokenBeta);
            }
            catch(Discord.Net.HttpException)
            {
                Console.WriteLine("Token seems to be invalid. Double check it.");
                Environment.Exit(0);
            }
            await bot.StartAsync();

            await bot.SetGameAsync(name: "Made with the Fans™", streamUrl: "https://www.twitch.tv/whatever", ActivityType.Streaming);

            await services.GetRequiredService<CommandHandler>().InstallCommandsAsync();
            Global.Bot = bot;

            Timer t = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
            Timer t2 = new Timer(TimeSpan.FromHours(new Random().Next(1,12)).TotalMilliseconds);

            t.AutoReset = true;
            t2.AutoReset = true;

            t.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            t2.Elapsed += new ElapsedEventHandler(T2_Elapsed);

            t.Start();
            t2.Start();

            FillReactionActions();

            UpdateCommandsInDb(services.GetRequiredService<CommandService>());

            await Task.Delay(-1);
        }

        private static void T2_Elapsed(object sender, ElapsedEventArgs e)
        {
            var guild = Global.Bot.GetGuild(Global.ServerID);
            var offtopic = guild.GetTextChannel(Global.PostTargets[PostTarget.OFFTOPIC]);
            var Faded = guild.GetUser(167897643131863040);
            offtopic.SendMessageAsync(Faded.Mention + "Pang Pang Pang! 🎆");
        }

        private static void FillReactionActions()
        {
            AddReactionActions = new Collection<IReactionAction>();
            RemoveReactionActions = new Collection<IReactionAction>();
            AddReactionActions.Add(new AddRoleReactionAction());
            AddReactionActions.Add(new StarboardAddedReactionAction());
            AddReactionActions.Add(new ProfanityReportReactionAdded());
            RemoveReactionActions.Add(new RemoveRoleReactionAction());
            RemoveReactionActions.Add(new StarboardRemovedReactionAction());
        }

        /// <summary>
        /// Synchronizes the state of the "Command" and "CommandModule" table in the database with the currently available commands/modules
        /// </summary>
        /// <param name="commandHandler">Reference to the <see cref="Discord.Commands.CommandService"> service containing information about the currently available commands</param>
        private static void UpdateCommandsInDb(CommandService commandHandler)
        {
          using(var db = new Database())
          {
            var modules = commandHandler.Modules;
            foreach(var module in modules)
            {
              var moduleQuery = db.Modules.Where(mo => mo.Name == module.Name);
              CommandModule moduleToUse;
              if(!moduleQuery.Any())
              {
                var newModule = new CommandModule();
                newModule.Name = module.Name;
                db.Modules.Add(newModule);
                db.SaveChanges();
                moduleToUse = newModule; 
              }
              else
              {
                moduleToUse = moduleQuery.First();
              }
              
              foreach(var command in module.Commands)
              {
                if(!db.Commands.Where(com => com.Name == command.Name).Any())
                {
                  var commandToCreate = new Command();
                  commandToCreate.Name = command.Name;
                  commandToCreate.ModuleId = moduleToUse.ID;
                  db.Commands.Add(commandToCreate);
                }
              }
              db.SaveChanges();
              var commandsInDb = db.Modules.Include(mo => mo.Commands).Where(mo => mo.Name == module.Name).FirstOrDefault();
              if(commandsInDb != null)
              {
                var commandsToDelete = new List<Command>();
                foreach(var command in commandsInDb.Commands)
                {
                    // the command exists in the db, but is not found in the module, we need to delete it
                   if(!module.Commands.Where(co => co.Name == command.Name).Any()) 
                   {
                    commandsToDelete.Add(command);
                   }
                }
                foreach(var toDelete in commandsToDelete)
                {
                  // also delete the channel group configuration for this commands, else the foreign keys fail
                  db.CommandInChannelGroups.RemoveRange(db.CommandInChannelGroups.Where(co => co.CommandID == toDelete.ID));
                  commandsInDb.Commands.Remove(toDelete);
                }
              }
              
            }
            db.SaveChanges();
          }
         
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            string[] status = { "Use ;help", "Made with the Fans™", "DM to contact mods" };
            Random ran = new Random();
            int index = ran.Next(status.Length);

           Global.Bot.SetGameAsync(name: status[index], streamUrl: "https://www.twitch.tv/whatever", ActivityType.Streaming);
        }

        private static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot)
                return;
            IUserMessage msg = await cache.DownloadAsync();
            foreach(IReactionAction action in AddReactionActions)
            {
                if(action.ActionApplies(msg, channel, reaction))
                {
                    await action.Execute(msg, channel, reaction);
                    break;
                }
            }
        }

        private static async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot)
                return;
            
            IUserMessage msg = await cache.DownloadAsync();
            foreach(IReactionAction action in RemoveReactionActions)
            {
                if(action.ActionApplies(msg, channel, reaction))
                {
                    await action.Execute(msg, channel, reaction);
                    break;
                }
            }
        }


        private static IServiceProvider BuildServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig {MessageCacheSize = 350 }))
                .AddSingleton<InteractiveService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<CommandService>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
