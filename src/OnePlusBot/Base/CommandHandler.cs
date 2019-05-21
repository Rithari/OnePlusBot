using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnePlusBot.Base
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _bot;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient bot, CommandService commands, IServiceProvider services)
        {
            _commands = commands;
            _bot = bot;
            _services = services;
        }


        public async Task InstallCommandsAsync()
        {
            _bot.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }


        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            
            if (!(messageParam is SocketUserMessage message)) return;

            int argPos = 0;

            // Getting the bot's guilds and setting variables for the OnePlus Guild and their set-ups channel.
            try
            {
                IReadOnlyCollection<SocketGuild> guilds = _bot.Guilds;
                SocketGuild oneplusGuild = guilds.FirstOrDefault(x => x.Id == 500105004082790400);
                SocketGuildChannel setupsChannel = oneplusGuild.Channels.FirstOrDefault(x => x.Id == 573675582278074379);
                var channel = messageParam.Channel as ITextChannel;

                if (channel.GuildId == oneplusGuild.Id)
                {
                    if (messageParam.Channel.Id == setupsChannel.Id)
                    {
                        // Deleting any messages that don't contain an embed, an image or a url.
                        if (!Regex.IsMatch(messageParam.Content, @"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$")
                            && messageParam.Attachments.Count == 0
                            && messageParam.Embeds.Count == 0)
                        {
                            await messageParam.DeleteAsync();
                        }
                    }
                }
            } catch
            {

            }

            if (!(message.HasCharPrefix(';', ref argPos) ||
                message.HasMentionPrefix(_bot.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_bot, message);

            var result = await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services);

        }
    }


}
