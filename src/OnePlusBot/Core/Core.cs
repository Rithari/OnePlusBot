using System;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using System.Net.Http;
using System.IO;

namespace OnePlusBot
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


            if (!File.Exists("tokens.txt"))
            {
                Console.WriteLine("You need a tokens.txt containing the tokens properly formatted, add it and retry.");
                Console.ReadKey();
                return;
            }


            Console.WriteLine("Development Branch?\n1: Yes 0: No");
            var userInput = Console.ReadLine();
            Console.WriteLine();
            if (userInput == "1")
            {
                string betaToken;
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
                string mainToken;
                StreamReader reader = new StreamReader("tokens.txt");
                {
                    //reader.ReadLine();
                    mainToken = reader.ReadLine();
                    reader.Dispose();
                }

                token = mainToken;
            }
            else
            {
                Console.WriteLine("Retry I guess.");
                await Task.Delay(200);
                Environment.Exit(0);
            }


            await _bot.LoginAsync(TokenType.Bot, token);
            await _bot.StartAsync();
            await _bot.SetGameAsync("Made with the Fans™ | ;help");

           await _services.GetRequiredService<CommandHandler>().InstallCommandsAsync();

            await Task.Delay(-1);
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
