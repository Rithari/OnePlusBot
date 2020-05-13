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
using Discord.Addons.Interactive;

namespace OnePlusBot.Modules.FAQ
{

  public partial class FAQ : InteractiveBase<SocketCommandContext>
  {
      [
      Command("faq"),
      Summary("Answers frequently asked questions with a predetermined response."),
      CommandDisabledCheck
    ]
    public async Task FAQAsync([Optional] [Remainder] string parameter)
    {
      var contextChannel = Context.Channel;
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
              await Context.Channel.SendMessageAsync("Warning command have different responses for this channel");
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
        
        await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
      }
    }
  }
}