using Discord;
using Discord.Commands;
using OnePlusBot.Base;
using OnePlusBot.Data.Models;
using OnePlusBot.Helpers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OnePlusBot.Data;
using System.Linq;

namespace OnePlusBot.Modules.Utility
{

    public partial class Utility : ModuleBase<SocketCommandContext>
  {

    [
        Command("uptime"),
        Summary("Shows the uptime of the bot."),
        CommandDisabledCheck
        
    ]
    public async Task UptimeAsync()
        {
            var ts = Global.stopwatch.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.TotalHours, ts.Minutes, ts.Seconds,ts.Milliseconds / 10);
            await ReplyAsync($"⚙️ The bot's been running for {elapsedTime}!");
        }
    [
        Command("showavatar"),
        Summary("Shows avatar of a user."),
        CommandDisabledCheck
    ]
    public async Task Avatar(IGuildUser user = null)
    {
        if(CommandHandler.FeatureFlagDisabled(FeatureFlag.UTILITY)) 
        {
            return;
        }
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
      if(CommandHandler.FeatureFlagDisabled(FeatureFlag.UTILITY)) 
      {
          return;
      }
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

      using(var db = new Database())
      {
          var dbUser = db.Users.AsQueryable().Where(us => us.Id == user.Id);
          if(dbUser.Any()) 
          {
            embedBuilder.AddField("Messages", dbUser.First().MessageCount);
          }
      }

      embedBuilder.AddField("Registered", Extensions.FormatDateTime(user.CreatedAt.DateTime), true);
                  
      await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
    }

  }
}