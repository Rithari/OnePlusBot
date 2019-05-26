using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Rest;
using System.IO;

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

        public async Task RoleReact(IUserMessage message)
        {
            Console.WriteLine("RoleReact called.");
            Global.RoleManagerId = message.Id;
            using (StreamWriter mw = new StreamWriter("messageid.txt"))
            {
                    mw.WriteLine(Global.RoleManagerId);
            }
            
            await message.AddReactionsAsync(new Emoji[]
            {
                new Emoji(":1_:574655515586592769"),
                new Emoji(":2_:574655515548844073"),
                new Emoji(":X_:574655515481866251"),
                new Emoji(":3_:574655515452506132"),
                new Emoji(":3T:574655515846508554"),
                new Emoji(":5_:574655515745976340"),
                new Emoji(":5T:574655515494318109"),
                new Emoji(":6_:574655515615952896"),
                new Emoji(":6T:574655515846508573"),
                new Emoji(":7_:574655515603501077"),
                new Emoji(":7P:574655515230076940"),
                new Emoji("❓"), new Emoji("📰")
            });
        }


        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            
            if (!(messageParam is SocketUserMessage message)) return;

            int argPos = 0;

            // Getting the bot's guilds and setting variables for the OnePlus Guild and their set-ups channel.
            try
            {
                IReadOnlyCollection<SocketGuild> guilds = _bot.Guilds;
                SocketGuild oneplusGuild = guilds.FirstOrDefault(x => x.Id == 378969558574432277);
                SocketGuildChannel setupsChannel = oneplusGuild.Channels.FirstOrDefault(x => x.Id == 473051502022361119);
                SocketGuildChannel infoChannel = oneplusGuild.Channels.FirstOrDefault(x => x.Id == 448846923596562432);
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

                    else if(messageParam.Channel.Id == infoChannel.Id)
                    {
                        Console.WriteLine("We're in info channel");

                        if (messageParam.Embeds.Count == 1)
                        {
                            Console.WriteLine("The message contains check passed.");
                            var userMessage = messageParam as IUserMessage;
                            await RoleReact(userMessage);
                        }
                    }
                }

            } catch
            {}

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
