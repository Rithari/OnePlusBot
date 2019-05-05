using System;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using System.Net.Http;
using System.IO;
using Discord.Addons.Interactive;
using System.Linq;

namespace OnePlusBot
{
    public class Core
    {
        public static void Main(string[] args)
             => new Core().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _bot;
        private string token;
        private string mainToken;
        private string betaToken;

        public async Task MainAsync()
        {
            IServiceProvider _services = BuildServices();

            _bot = _services.GetRequiredService<DiscordSocketClient>();

            _bot.Log += Log;

            _bot.ReactionAdded += OnReactionAdded;


            if (!File.Exists("tokens.txt"))
            {
                Console.WriteLine("You need a tokens.txt containing the tokens properly formatted, add it and retry.");
                // File.OpenWrite("tokens.txt");

                Console.ReadKey();
                return;
            }


            Console.WriteLine("Development Branch?\n1: Yes 0: No");
            var userInput = Console.ReadLine();
            Console.WriteLine();
            if (userInput == "1")
            {
                StreamReader reader = new StreamReader("tokens.txt");
                {
                    reader.ReadLine();
                    betaToken = reader.ReadLine();
                    reader.Dispose();
                }

                token = betaToken;
            }
            else if (userInput == "0")
            {
                StreamReader reader = new StreamReader("tokens.txt");
                {
                    mainToken = reader.ReadLine();
                    reader.Dispose();
                }

                token = mainToken;
            }
            else
            {
                Console.WriteLine("Retry I guess.");
                await Task.Delay(1000);
                Environment.Exit(0);
            }


            await _bot.LoginAsync(TokenType.Bot, token);
            await _bot.StartAsync();
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
            var rolehelper = Guild.Roles.FirstOrDefault(x => x.Name == "Helper");
            var rolenews = Guild.Roles.FirstOrDefault(x => x.Name == "News");




            if (reaction.MessageId == Global.ReactBuilderMsgId)
            {

                if (reaction.User.Value.IsBot)
                    return;

                switch (reaction.Emote.Name)
                {
                    case ":1_:574655515586592769":
                        var msg = await channel.SendMessageAsync(user.Mention + " joined OnePlus One.");
                        await (user as IGuildUser).AddRoleAsync(role0);
                        await Task.Delay(1200);
                        await msg.DeleteAsync();
                        break;
                    case ":2_:574655515548844073":
                        msg = await channel.SendMessageAsync(user.Mention + " joined OnePlus 2.");
                        await (user as IGuildUser).AddRoleAsync(role1);
                        await Task.Delay(1200);
                        await msg.DeleteAsync();
                        break;

                    case ":X_:574655515481866251":
                        msg = await channel.SendMessageAsync(user.Mention + " joined OnePlus X");
                        await (user as IGuildUser).AddRoleAsync(role2);
                        await Task.Delay(1200);
                        await msg.DeleteAsync();
                        break;

                    case ":3_:574655515452506132":
                        msg = await channel.SendMessageAsync(user.Mention + " joined OnePlus 3.");
                        await (user as IGuildUser).AddRoleAsync(role3);
                        await Task.Delay(1200);
                        await msg.DeleteAsync();
                        break;

                    case ":3T:574655515846508554":
                        msg = await channel.SendMessageAsync(user.Mention + " joined OnePlus 3T.");
                        await (user as IGuildUser).AddRoleAsync(role4);
                        await Task.Delay(1200);
                        await msg.DeleteAsync();
                        break;

                    case ":5_:574655515745976340":
                        msg = await channel.SendMessageAsync(user.Mention + " joined OnePlus 5.");
                        await (user as IGuildUser).AddRoleAsync(role5);
                        await Task.Delay(1200);
                        await msg.DeleteAsync();
                        break;

                    case ":5T:574655515494318109":
                        msg = await channel.SendMessageAsync(user.Mention + " joined OnePlus 5T.");
                        await (user as IGuildUser).AddRoleAsync(role6);
                        await Task.Delay(1200);
                        await msg.DeleteAsync();
                        break;

                    case ":6_:574655515615952896":
                        msg = await channel.SendMessageAsync(user.Mention + " joined OnePlus 6.");
                        await (user as IGuildUser).AddRoleAsync(role7);
                        await Task.Delay(1200);
                        await msg.DeleteAsync();
                        break;

                    case ":6T:574655515846508573":
                        msg = await channel.SendMessageAsync(user.Mention + " joined OnePlus 6T.");
                        await (user as IGuildUser).AddRoleAsync(role8);
                        await Task.Delay(1200);
                        await msg.DeleteAsync();
                        break;

                    case "❓":
                        msg = await channel.SendMessageAsync(user.Mention + " joined the helpers");
                        await (user as IGuildUser).AddRoleAsync(rolehelper);
                        await Task.Delay(1200);
                        await msg.DeleteAsync();
                        break;

                    case "📰":
                        msg = await channel.SendMessageAsync(user.Mention + " subscribed to the news outlet.");
                        await (user as IGuildUser).AddRoleAsync(rolenews);
                        await Task.Delay(12000);
                        await msg.DeleteAsync();
                        break;

                    default:
                        msg = await channel.SendMessageAsync("invalid emote");
                        await Task.Delay(1200);
                        await msg.DeleteAsync();
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
