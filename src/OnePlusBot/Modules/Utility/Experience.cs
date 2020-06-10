using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using OnePlusBot.Base;
using OnePlusBot.Helpers;
using System.Runtime.InteropServices;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;

namespace OnePlusBot.Modules.Utility
{

  public partial class Utility : ModuleBase<SocketCommandContext>
  {
    [
      Command("rank"),
      Summary("Shows your/another users experience, level, and rank in the server"),
      CommandDisabledCheck
    ]
    public async Task<RuntimeResult> ShowLevels([Optional] IGuildUser user)
    {
      IUser userToUse = null;
      if(user != null)
      {
          userToUse = user;
      }
      else 
      {
          userToUse = Context.Message.Author;
      }
      if(userToUse.IsBot)
      {
          return CustomResult.FromIgnored();
      }
      var embedBuilder = new EmbedBuilder();
      using(var db = new Database())
      {
        var userInDb = db.Users.AsQueryable().Where(us => us.Id == userToUse.Id).FirstOrDefault();
        if(userInDb != null)
        {
          var rank = db.Users.AsQueryable().OrderByDescending(us => us.XP).ToList().IndexOf(userInDb) + 1;
          var nextLevel = db.ExperienceLevels.AsQueryable().Where(lv => lv.Level == userInDb.Level + 1).FirstOrDefault();
          embedBuilder.WithAuthor(new EmbedAuthorBuilder().WithIconUrl(userToUse.GetAvatarUrl()).WithName(Extensions.FormatUserName(userToUse)));
          embedBuilder.AddField("XP", userInDb.XP, true);
          embedBuilder.AddField("Level", userInDb.Level, true);
          embedBuilder.AddField("Messages", userInDb.MessageCount, true);
          if(nextLevel != null)
          {
              embedBuilder.AddField("XP to next Level", nextLevel.NeededExperience - userInDb.XP, true);
          }
          embedBuilder.AddField("Rank", rank, true);
        }
        else 
        {
            embedBuilder.WithTitle("No experience tracked.").WithDescription("Please check back in a minute.");
        }
      }
      await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
      return CustomResult.FromSuccess();
    }

    [
        Command("leaderboard"),
        Summary("shows the top page of the leaderboard (or a certain page)"),
        CommandDisabledCheck
    ]
    public async Task<RuntimeResult> ShowLeaderboard([Optional] int page)
    {
      var embedBuilder = new EmbedBuilder();
      using(var db = new Database())
      {
        var allUsers = db.Users.AsQueryable().OrderByDescending(us => us.XP).ToList();
        var usersInLeaderboard = db.Users.AsQueryable().OrderByDescending(us => us.XP);
        System.Collections.Generic.List<User> usersToDisplay;
        if(page > 1)
        {
            usersToDisplay = usersInLeaderboard.Skip((page -1) * 10).Take(10).ToList();
        }
        else 
        {
            usersToDisplay = usersInLeaderboard.Take(10).ToList();
        }
        embedBuilder = embedBuilder.WithTitle("Leaderboard of gained experience");
        var description = new StringBuilder();
        if(page * 10 > allUsers.Count())
        {
          description.Append("Page not found. \n");
        }
        else
        {
          description.Append("Rank | Name | Experience | Level | Messages \n");
          foreach(var user in usersToDisplay)
          {
            var rank = allUsers.IndexOf(user) + 1;
            var userInGuild = Context.Guild.GetUser(user.Id);
            var name = userInGuild != null ? Extensions.FormatUserName(userInGuild) : "User left guild " + user.Id;
            description.Append($"[#{rank}] → **{name}**\n");
            description.Append($"XP: {user.XP} Level: {user.Level}: Messages: {user.MessageCount} \n \n");
          }
          description.Append("\n");
        }
        
        description.Append("Your placement: \n");
        var caller = db.Users.AsQueryable().Where(us => us.Id == Context.Message.Author.Id).FirstOrDefault();
        if(caller != null)
        {
          var callRank = allUsers.IndexOf(caller) + 1;
          var userInGuild = Context.Guild.GetUser(caller.Id);
          description.Append($"[#{callRank}] → *{Extensions.FormatUserName(userInGuild)}* XP: {caller.XP} messages: {caller.MessageCount} \n");
          description.Append($"Level: {caller.Level}");
        }
        embedBuilder = embedBuilder.WithDescription(description.ToString());
        embedBuilder.WithFooter(new EmbedFooterBuilder().WithText("Use leaderboard <page> to view more of the leaderboard"));
        await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
        
      }
        
      return CustomResult.FromSuccess();
    }
  }
}