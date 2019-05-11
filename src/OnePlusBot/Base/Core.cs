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
        private string mainToken;
        private string betaToken;

        public async Task MainAsync()
        {
            IServiceProvider _services = BuildServices();

            _bot = _services.GetRequiredService<DiscordSocketClient>();

            _bot.Log += Log;

            _bot.ReactionAdded += OnReactionAdded;

            _bot.ReactionRemoved += OnReactionRemoved;


            if (!File.Exists("tokens.txt"))
            {
                Console.WriteLine("Please paste in your bot's token:");

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
            var rolehelper = Guild.Roles.FirstOrDefault(x => x.Name == "Helper");
            var rolenews = Guild.Roles.FirstOrDefault(x => x.Name == "News");
            var rolejournalist = Guild.Roles.FirstOrDefault(x => x.Name == "Journalist");


            if (reaction.MessageId == Global.ReactBuilderMsgId)
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

                    case "❓":
                        await (user as IGuildUser).AddRoleAsync(rolehelper);

                        break;

                    case "📰":
                        await (user as IGuildUser).AddRoleAsync(rolenews);

                        break;
                }
            } else if (reaction.MessageId == Global.ReactBuilderModMsgId)
            {
                switch (reaction.Emote.Name)
                {
                    case "📰":
                        await (user as IGuildUser).AddRoleAsync(rolejournalist);

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
            var rolehelper = Guild.Roles.FirstOrDefault(x => x.Name == "Helper");
            var rolenews = Guild.Roles.FirstOrDefault(x => x.Name == "News");
            var rolejournalist = Guild.Roles.FirstOrDefault(x => x.Name == "Journalist");


            if (reaction.MessageId == Global.ReactBuilderMsgId)
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

                    case "❓":
                        await (user as IGuildUser).RemoveRoleAsync(rolehelper);

                        break;

                    case "📰":
                        await (user as IGuildUser).RemoveRoleAsync(rolenews);

                        break;
                }
            } else if (reaction.MessageId == Global.ReactBuilderModMsgId)
            {
                switch (reaction.Emote.Name)
                {
                    case "📰":
                        await (user as IGuildUser).RemoveRoleAsync(rolejournalist);

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
