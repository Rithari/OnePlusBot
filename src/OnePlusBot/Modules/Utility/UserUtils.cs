using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using OnePlusBot.Base;
using OnePlusBot.Helpers;
using System.Globalization;
using System.Runtime.InteropServices;
using OnePlusBot.Data.Models;
namespace OnePlusBot.Modules.Utility
{

  public partial class Utility : ModuleBase<SocketCommandContext>
  {
    [
        Command("showavatar"),
        Summary("Shows avatar of a user."),
        CommandDisabledCheck
    ]
    public async Task Avatar(IGuildUser user = null)
    {
        if (user == null)
          user = (IGuildUser) Context.User;

        var uri = user.RealAvatarUrl(4096).ToString();

        if (uri == null)
        {
          await Context.Channel.EmbedAsync(new EmbedBuilder()
              .WithColor(9896005)
              .WithDescription("User has no avatar."));
          return;
        }

        var embed = new EmbedBuilder();
        embed.WithColor(9896005);
        embed.Url = uri;

        embed.AddField(x =>
        {
          x.Name = "Username";
          x.Value = Extensions.FormatMentionDetailed(user);
          x.IsInline = true;
        });
        embed.AddField(x =>
        {
          x.Name = "Image";
          x.Value = $"[Link]({uri})";
          x.IsInline = true;
        });

        embed.ImageUrl = uri;
        
        await Context.Channel.EmbedAsync(embed);
    }


    /// <summary>
    /// Shows information about the given (or executing in case given is null) user. This information includes registration/join date, current status and nickname.
    /// </summary>
    /// <param name="user">The <see cref="Discord.IGuildUser"> to post the informatoin for</param>
    /// <returns>Task</returns>
    [
        Command("userinfo"),
        Summary("Displays User Information"),
        CommandDisabledCheck
    ]
    public async Task UserInfo([Optional] IGuildUser user)
    {
      user = user ?? (IGuildUser) Context.User;
      
      var embedBuilder = new EmbedBuilder();

      embedBuilder.WithColor(9896005);
      embedBuilder.WithAuthor(x =>
      {
        x.Name = user.Username + "#" + user.Discriminator;
      });

      embedBuilder.ThumbnailUrl = user.GetAvatarUrl();

      embedBuilder.AddField("User id", user.Id, true);
      embedBuilder.AddField("Status", user.Status.ToString(), true);

      if(user.Nickname != null)
      {
        embedBuilder.AddField("Nickname", user.Nickname, true);
      }
      
      embedBuilder.AddField("Activity", user.Activity?.Name ?? "Nothing", true);

      if(user.JoinedAt.HasValue)
      {
        embedBuilder.AddField("Joined", Extensions.FormatDateTime(user.JoinedAt.Value.DateTime), true);
      }

      embedBuilder.AddField("Registered", Extensions.FormatDateTime(user.CreatedAt.DateTime), true);
                  
      await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
    }

  }
}