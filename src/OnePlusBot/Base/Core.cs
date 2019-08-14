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
        public static async Task Main()
        {
            var services = BuildServices();
            var bot = services.GetRequiredService<DiscordSocketClient>();
            bot.Log += Log;
            bot.ReactionAdded += OnReactionAdded;
            bot.ReactionRemoved += OnReactionRemoved;
            bot.Ready += OnBotReady;

            bot.Ready += () => 
            {
                return MuteTimerManager.SetupTimers(true);
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

            await Task.Delay(-1);
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            string[] status = { "Use ;help", "Has anyone seen Jisifus? lol", "Made with the Fans™" };
            Random ran = new Random();
            int index = ran.Next(status.Length);

           Global.Bot.SetGameAsync(name: status[index], streamUrl: "https://www.twitch.tv/whatever", ActivityType.Streaming);
        }

        // For future reference: this will be used for tasks to be completed only after downloading guild data.
        private static Task OnBotReady() {return Task.CompletedTask;}

        private static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot)
                return;

            if (reaction.MessageId != Global.RoleManagerMessageId)
                return;
            
            var dict = new Dictionary<string, string> //TODO: Change from string to emote and role IDs
            {
                { "1_", "OnePlus One" },
                { "2_", "OnePlus 2" },
                { "X_", "OnePlus X" },
                { "3_", "OnePlus 3" },
                { "3T", "OnePlus 3T" },
                { "5_", "OnePlus 5" },
                { "5T", "OnePlus 5T" },
                { "6_", "OnePlus 6" },
                { "6T", "OnePlus 6T" },
                { "7_", "OnePlus 7" },
                { "7P", "OnePlus 7 Pro" },
                { "\x2753", "Helper" },
                { "\xD83D\xDCF0", "News" }
            };

            if (dict.TryGetValue(reaction.Emote.Name, out string roleName))
            {
                var guild = ((IGuildChannel) channel).Guild;
                var role = guild.Roles.FirstOrDefault(x => x.Name == roleName);
                if (role != null)
                {
                    var user = (IGuildUser) reaction.User.Value;
                    await user.AddRoleAsync(role);
                }
            }
        }

        private static async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot)
                return;
            
            if (reaction.MessageId != Global.RoleManagerMessageId)
                return;
            
            var dict = new Dictionary<string, string> //TODO: Change from string to emote and role IDs // It won't work with the IDs, iirc. -Rith
            {
                { "1_", "OnePlus One" },
                { "2_", "OnePlus 2" },
                { "X_", "OnePlus X" },
                { "3_", "OnePlus 3" },
                { "3T", "OnePlus 3T" },
                { "5_", "OnePlus 5" },
                { "5T", "OnePlus 5T" },
                { "6_", "OnePlus 6" },
                { "6T", "OnePlus 6T" },
                { "7_", "OnePlus 7" },
                { "7P", "OnePlus 7 Pro" },
                { "\x2753", "Helper" },
                { "\xD83D\xDCF0", "News" }
            };


            if (dict.TryGetValue(reaction.Emote.Name, out string roleName))
            {
                var guild = ((IGuildChannel) channel).Guild;
                var role = guild.Roles.FirstOrDefault(x => x.Name == roleName);
                if (role != null)
                {
                    var user = (IGuildUser) reaction.User.Value;
                    await user.RemoveRoleAsync(role);
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
