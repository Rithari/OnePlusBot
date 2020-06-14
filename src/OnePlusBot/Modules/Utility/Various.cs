using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using OnePlusBot.Base;
using OnePlusBot.Helpers;
using OnePlusBot.Data.Models;

namespace OnePlusBot.Modules.Utility
{
  public partial class Utility : ModuleBase<SocketCommandContext>
  {
    /// <summary>
    /// Searches for all of the custom emoji in the parameters and posts a bigger image and the direct link
    /// </summary>
    /// <param name="_">Text containing the emoji with the URLs </param>
    /// <returns>Task</returns>
    [
      Command("se"),
      Summary("Shows bigger version of am emote."),
      CommandDisabledCheck
    ]
    public async Task Showemojis([Remainder] string _) // need to have the parameter so that the message.tags gets populated
    {
      var tags = Context.Message.Tags.Where(t => t.Type == TagType.Emoji).Select(t => (Emote)t.Value);
        
      var result = string.Join("\n", tags.Select(m => "**Name:** " + m + " **Link:** " + m.Url));

      if (string.IsNullOrWhiteSpace(result)) 
      {
        await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription("No special emojis found."));
      }
      else 
      {
        await Context.Channel.SendMessageAsync(result);
      }
    }

      
    [
      Command("echo"),
      Summary("Echoes back the remainder argument of the command."),
      RequireRole("staff"),
      CommandDisabledCheck
    ]
    public Task EchoAsync([Remainder] string text)
    {
      return ReplyAsync(text);
    }

    [
      Command("ping"),
      Summary("Standard ping command."),
      CommandDisabledCheck
    ]
    public async Task PingAsync()
    {
      const string reply = "Pong....\nWithin {0} ms";
      await ReplyAsync(string.Format(reply, Context.Client.Latency));
    }
  }
}
