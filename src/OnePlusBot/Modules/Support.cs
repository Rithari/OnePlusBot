using Discord;
using Discord.Commands;
using OnePlusBot.Base;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnePlusBot.Helpers;
using Discord.WebSocket;
using System.Runtime.InteropServices;

namespace OnePlusBot.Modules
{
    [
      Summary("Module containing the commands for help and faq commands")
    ]
    public class Support : ModuleBase<SocketCommandContext>
    {
      private readonly CommandService _commands;
      private readonly IServiceProvider _services;

      public Support(IServiceProvider services, CommandService commands)
      {
        _commands = commands;
        _services = services;
      }

      [
        Command("help"),
        Summary("Lists commands with their respective user facing documentation")
      ]
      public async Task<RuntimeResult> PrintHelp([Optional] string commandOrModule)
      {
        StringBuilder sb = new StringBuilder();
        string footerText;
        if(commandOrModule == null) 
        {
          foreach (var mod in _commands.Modules.Where(m => m.Parent == null))
          {
            sb.Append(HelpBuilder.BuildGeneralModuleDescription(mod));
            sb.Append(Environment.NewLine);
          }
          footerText = "Use 'help <module name>' for a list of commands of this module (case insensitive).";
        } 
        else 
        {
          commandOrModule = commandOrModule.ToLower();
          var probableModule = _commands.Modules.Where(m => m.Name.ToLower().Equals(commandOrModule));
          if(probableModule.Any()) 
          {
            sb.Append(HelpBuilder.BuildDetailedModuleDescription(probableModule.First(), Context, _services));
            footerText = "Use 'help <command name>' for a detailed description of the command.";
          }
          else
          {
            var probableCommand = _commands.Commands.Where(c => c.Name.ToLower().Equals(commandOrModule));
            if(probableCommand.Any()) 
            {
              var command = probableCommand.First();
              sb.Append(HelpBuilder.BuildDetailedCommandDescription(command));
            }
            else
            {
              sb.Append("No module/command found for that name.");
            }
            footerText = "";
          }
        }
        var builder = new EmbedBuilder();
        builder.WithDescription(sb.ToString());
        builder.WithFooter(new EmbedFooterBuilder().WithText(footerText));
        builder.WithTitle("OneplusBot - Help");
        await Context.Channel.SendMessageAsync(embed: builder.Build());
        return CustomResult.FromIgnored();
      }

      [
        Command("faq"),
        Summary("Answers frequently asked questions with a predetermined response."),
        CommandDisabledCheck
      ]
      public async Task FAQAsync([Optional] string parameter, [Optional] ISocketMessageChannel channel)
      {
        var contextChannel = channel ?? Context.Channel;
        if(parameter == null || parameter == string.Empty)
        {
            await PrintAvailableCommands(contextChannel);
            return;
        }
        var commands = Global.FAQCommands;
        parameter = parameter.Trim();
        var appropriateCommand = commands.Where(m => 
        {
            bool usingAlias = Array.Exists(m.IndividualAliases(), alias => alias.Equals(parameter));
            return (m.Name == parameter || usingAlias);
        });
        if(appropriateCommand.Any())
        {
          var matchingCommand = appropriateCommand.First();
          if(matchingCommand.CommandChannels != null) 
          {
            var commandChannels = matchingCommand.CommandChannels.Where(cha => cha.ChannelGroupReference.Channels.Where(grp => grp.ChannelId == contextChannel.Id).FirstOrDefault() != null);
            if(commandChannels.Any())
            {
              if(commandChannels.Count() > 1)
              {
                await Context.Channel.SendMessageAsync("Warning: command has multiple responses for this channel");
              }
              foreach(var commandChannel in commandChannels){
                var entries = commandChannels.First().CommandChannelEntries.OrderBy(entry => entry.Position);
                if(entries.Any())
                {
                  foreach(var entry in entries)
                  {
                    if(!entry.IsEmbed)
                    {
                      await Context.Channel.SendMessageAsync(entry.Text);
                    }
                    else
                    {
                      var embed = Extensions.FaqCommandEntryToBuilder(entry);
                      await Context.Channel.SendMessageAsync(embed: embed.Build());
                    }
                    await Task.Delay(200);
                  }
                }
                else
                {
                  await Context.Channel.SendMessageAsync($"Channel has no posts configured for command {appropriateCommand.First().Name}.");
                }
              }
            }
            else
            {
              await PrintAvailableCommands(contextChannel);
            }
          }
          else
          {
            await Context.Channel.SendMessageAsync($"Channel has no entry configured for command {appropriateCommand.First().Name}.");
          }
        }
        else
        {
          await PrintAvailableCommands(contextChannel);
        }
      }
      public async Task PrintAvailableCommands(ISocketMessageChannel contextChannel)
      {
        var commandsAvailable = Global.FAQCommandChannels.
        Where(ch => ch.ChannelGroupReference.Channels.
            Where(grp => grp.ChannelId == contextChannel.Id).
            FirstOrDefault() != null)
        .ToList();
        if(commandsAvailable.Count() == 0)
        {
            await Context.Channel.SendMessageAsync("No entry available.");
        }
        else
        {
          var stringBuilder = new StringBuilder(" ");
          for(var index = 0; index < commandsAvailable.Count; index++)
          {
            var command = commandsAvailable[index];
            stringBuilder.Append($"`{command.Command.Name}`");
            if(index < commandsAvailable.Count -1 )
            {
                stringBuilder.Append(", ");
            }
          }
          var embedBuilder = new EmbedBuilder().WithTitle("Available entries in this channel").WithDescription(stringBuilder.ToString());
          embedBuilder.WithFooter("You can access them by typing 'faq <entryName>'.");
          await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }
      }
  }   
}
