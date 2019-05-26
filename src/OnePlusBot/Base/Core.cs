using System;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using System.Net.Http;
using System.IO;
using System.Linq;

namespace OnePlusBot.Base
{
    public class Core
    {
        public static void Main(string[] args)
             => new Core().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _bot;
        private string token;

        public async Task MainAsync()
        {
            IServiceProvider _services = BuildServices();

            _bot = _services.GetRequiredService<DiscordSocketClient>();

            _bot.Log += Log;

            _bot.ReactionAdded += OnReactionAdded;

            _bot.ReactionRemoved += OnReactionRemoved;
            
            try
            {
                using (StreamReader mr = new StreamReader("messageid.txt"))
                { Global.RoleManagerId = ulong.Parse(mr.ReadLine()); }
            }

            catch { }
            
            if (!File.Exists("messageid.txt"))
            {
                File.Create("messageid.txt");
            }


            if (!File.Exists("tokens.txt"))
            {
                Console.Write("Please paste in your bot's token: ");

                var inputKey = Console.ReadLine();
                token = inputKey;
            }
            else
            {
                Console.WriteLine("Development Branch?\n1: Yes 0: No");
                var userInput = Console.ReadLine();
                Console.WriteLine();

                if (userInput == "1")
                {
                    StreamReader reader = new StreamReader("tokens.txt");
                    {
                        reader.ReadLine();
                        token = reader.ReadLine();
                        reader.Dispose();
                    }
                }
                else if (userInput == "0")
                {
                    StreamReader reader = new StreamReader("tokens.txt");
                    {
                        token = reader.ReadLine();
                        reader.Dispose();
                    }

                }
                else
                {
                    Console.WriteLine("Retry I guess.");
                    await Task.Delay(1000);
                    Environment.Exit(0);
                }
            }
            try
            {
                await _bot.LoginAsync(TokenType.Bot, token);
                await _bot.StartAsync();
            }
            catch
            {
                Console.WriteLine("You must have input an incorrect key.");
                return;
            }

            await _bot.SetGameAsync("Made with the Fans™ | ;help");

            await _services.GetRequiredService<CommandHandler>().InstallCommandsAsync();

            await Task.Delay(-1);
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var Guild = (channel as IGuildChannel).Guild;
            var user = reaction.User.Value;
            var role0 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus One");
            var role1 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 2");
            var role2 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus X");
            var role3 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 3");
            var role4 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 3T");
            var role5 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 5");
            var role6 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 5T");
            var role7 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 6");
            var role8 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 6T");
            var role9 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 7");
            var role10 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 7 Pro");
            var rolehelper = Guild.Roles.FirstOrDefault(x => x.Name == "Helper");
            var rolenews = Guild.Roles.FirstOrDefault(x => x.Name == "News");




            if (reaction.MessageId == Global.RoleManagerId)
            {

                if (reaction.User.Value.IsBot)
                    return;

                // await reaction.User.Value.SendMessageAsync(reaction.Emote.Name + " is the reaction name");

                switch (reaction.Emote.Name)
                {
                    case "1_":
                        await (user as IGuildUser).AddRoleAsync(role0);

                        break;

                    case "2_":
                        await (user as IGuildUser).AddRoleAsync(role1);

                        break;

                    case "X_":
                        await (user as IGuildUser).AddRoleAsync(role2);

                        break;

                    case "3_":

                        await (user as IGuildUser).AddRoleAsync(role3);
                        break;

                    case "3T":
                        await (user as IGuildUser).AddRoleAsync(role4);

                        break;

                    case "5_":
                        await (user as IGuildUser).AddRoleAsync(role5);

                        break;

                    case "5T":
                        await (user as IGuildUser).AddRoleAsync(role6);

                        break;

                    case "6_":
                        await (user as IGuildUser).AddRoleAsync(role7);

                        break;

                    case "6T":
                        await (user as IGuildUser).AddRoleAsync(role8);

                        break;
                    case "7_":
                        await (user as IGuildUser).AddRoleAsync(role9);

                        break;

                    case "7P":
                        await (user as IGuildUser).AddRoleAsync(role10);

                        break;

                    case "❓":
                        await (user as IGuildUser).AddRoleAsync(rolehelper);

                        break;

                    case "📰":
                        await (user as IGuildUser).AddRoleAsync(rolenews);

                        break;
                }
            }

        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var Guild = (channel as IGuildChannel).Guild;
            var user = reaction.User.Value;
            var role0 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus One");
            var role1 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 2");
            var role2 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus X");
            var role3 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 3");
            var role4 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 3T");
            var role5 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 5");
            var role6 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 5T");
            var role7 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 6");
            var role8 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 6T");
            var role9 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 7");
            var role10 = Guild.Roles.FirstOrDefault(x => x.Name == "OnePlus 7 Pro");
            var rolehelper = Guild.Roles.FirstOrDefault(x => x.Name == "Helper");
            var rolenews = Guild.Roles.FirstOrDefault(x => x.Name == "News");




            if (reaction.MessageId == Global.RoleManagerId)
            {

                if (reaction.User.Value.IsBot)
                    return;

                // await reaction.User.Value.SendMessageAsync(reaction.Emote.Name + " is the reaction name");

                switch (reaction.Emote.Name)
                {
                    case "1_":
                        await (user as IGuildUser).RemoveRoleAsync(role0);

                        break;

                    case "2_":
                        await (user as IGuildUser).RemoveRoleAsync(role1);

                        break;

                    case "X_":
                        await (user as IGuildUser).RemoveRoleAsync(role2);

                        break;

                    case "3_":

                        await (user as IGuildUser).RemoveRoleAsync(role3);
                        break;

                    case "3T":
                        await (user as IGuildUser).RemoveRoleAsync(role4);

                        break;

                    case "5_":
                        await (user as IGuildUser).RemoveRoleAsync(role5);

                        break;

                    case "5T":
                        await (user as IGuildUser).RemoveRoleAsync(role6);

                        break;

                    case "6_":
                        await (user as IGuildUser).RemoveRoleAsync(role7);

                        break;

                    case "6T":
                        await (user as IGuildUser).RemoveRoleAsync(role8);

                        break;

                    case "7_":
                        await (user as IGuildUser).RemoveRoleAsync(role9);

                        break;

                    case "7P":
                        await (user as IGuildUser).RemoveRoleAsync(role10);

                        break;

                    case "❓":
                        await (user as IGuildUser).RemoveRoleAsync(rolehelper);

                        break;

                    case "📰":
                        await (user as IGuildUser).RemoveRoleAsync(rolenews);

                        break;
                }
            }
        }


        public IServiceProvider BuildServices()
            => new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<CommandService>()
            .AddSingleton<HttpClient>()
            .BuildServiceProvider();

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

    }


}
