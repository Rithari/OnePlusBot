using System;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;

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

            Console.WriteLine("Development branch?\n1: Yes 0: No");
            var value = Console.ReadLine();
            Console.WriteLine();
            if (value == "1")
            {
                token = "NDk5MjIwMDAxMzMzNTc1Njgx.Dp5Ghw.6JXx_JqCenK4jnZZxbl50EP94qU";
            }
            else if (value == "0")
            {
                token = "NDI2MDE1NTYyNTk1MDQxMjgw.Dp1iCA.STkEQT-zCzbfD5KVS5nNBBWmuCM";
            }
            else
            {
                Console.WriteLine("Fuck you");
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
            .BuildServiceProvider();

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

    }


}
