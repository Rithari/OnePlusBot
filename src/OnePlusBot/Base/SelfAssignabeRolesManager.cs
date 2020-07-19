using System.Threading.Tasks;
using OnePlusBot.Data.Models;
using Discord;
using OnePlusBot.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
   
namespace OnePlusBot.Base
{
  public class SelfAssignabeRolesManager 
  {
    /// <summary>
    /// Creates the post containing the fields for the different roles, reacts with the proper reactions to the post and deletes the old post.
    /// </summary>
    /// <returns>Task</returns>
    public async Task SetupInfoPost()
    {
      var infoChannelId = Global.PostTargets[PostTarget.INFO];
      var guild = Global.Bot.GetGuild(Global.ServerID);
   
      var embedBuilder = new EmbedBuilder();
      embedBuilder.WithTitle("__Assignable Roles__");
      embedBuilder.WithColor(15258703);
      embedBuilder.WithDescription("You can assign yourself one of the following roles by reacting to its corresponding emote. \n Removing your reaction will remove your role.");
      embedBuilder.WithFooter(new EmbedFooterBuilder().WithText("For any problems, contact Rithari#0001."));

      using(var db = new Database())
      {
        var roles = db.ReactionRoles.Include(ro => ro.EmoteReference).Include(ro => ro.RoleReference).Where(ro => ro.Area == "info").OrderBy(ro => ro.Position).ToList();
        var reactions = new List<IEmote>();
        foreach(var role in roles)
        {
          var roleInGuild = guild.GetRole(role.RoleReference.RoleID);
          var emote = role.EmoteReference.GetAsEmote();
          embedBuilder.AddField(emote + " " + roleInGuild.Name, "\u200B", true);
          reactions.Add(emote);
        }
      
        var oldPostId = Global.InfoRoleManagerMessageId;
        var infoChannel = guild.GetTextChannel(infoChannelId);
        var oldMeessage = await infoChannel.GetMessageAsync(oldPostId);
        if(oldMeessage != null) 
        {
          await oldMeessage.DeleteAsync();
        }
        var message = await infoChannel.SendMessageAsync(embed: embedBuilder.Build());
        Global.InfoRoleManagerMessageId = message.Id;
        // TODO refactor
        db.PersistentData.AsQueryable().Where(dt => dt.Name == "rolemanager_message_id").First().Value = message.Id;
        await message.AddReactionsAsync(reactions.ToArray());
        db.SaveChanges();
      }
    }
  }
}