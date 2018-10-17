using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

namespace OnePlusBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _bot;
        private readonly CommandService _commands;
        //private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient bot, CommandService commands)
        {
            _commands = commands;
            _bot = bot;
        }


        public async Task InstallCommandsAsync()
        {
            _bot.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }


        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            IReadOnlyCollection<SocketGuild> guilds = _bot.Guilds;
            SocketGuild oneplusGuild = guilds.FirstOrDefault(x => x.Name == "/r/oneplus");
            SocketGuildChannel wallpapersChannel = oneplusGuild.Channels.FirstOrDefault(x => x.Name == "wallpapers");

           if(messageParam.Channel.Id == wallpapersChannel.Id)
            {
                var messageContent = messageParam.Content;

                if (!Regex.IsMatch(messageContent, @"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$") && messageParam.Attachments.Count == 0 && messageParam.Embeds.Count == 0)
                {
                    await messageParam.DeleteAsync();
                }
            }


            if (!(message.HasCharPrefix(';', ref argPos) ||
                message.HasMentionPrefix(_bot.CurrentUser, ref argPos))|| 
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_bot, message);

            var result = await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);

        }
    }


}
