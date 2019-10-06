using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Data.Common;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

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

            bot.Ready += async () => 
            {
                await MuteTimerManager.SetupTimers(true);
                await ReminderTimerManger.SetupTimers(true);
            };

            await bot.LoginAsync(TokenType.Bot, Global.Token);
            await bot.StartAsync();

            await bot.SetGameAsync(name: "Made with the Fans™", streamUrl: "https://www.twitch.tv/whatever", ActivityType.Streaming);

            await services.GetRequiredService<CommandHandler>().InstallCommandsAsync();
            Global.Bot = bot;

            Timer t = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds); // Set the time (5 mins in this case)
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            t.Start();

            FillReactionActions();

            await Task.Delay(-1);
        }

        private static void FillReactionActions()
        {
            AddReactionActions = new Collection<IReactionAction>();
            RemoveReactionActions = new Collection<IReactionAction>();
            AddReactionActions.Add(new AddRoleReactionAction());
            AddReactionActions.Add(new StarboardAddedReactionAction());
            RemoveReactionActions.Add(new RemoveRoleReactionAction());
            RemoveReactionActions.Add(new StarboardRemovedReactionAction());
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            string[] status = { "Use ;help", "Made with the Fans™" };
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
