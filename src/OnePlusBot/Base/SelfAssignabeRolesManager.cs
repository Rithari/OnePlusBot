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
      embedBuilder.WithFooter(new EmbedFooterBuilder().WithText("For any problems, contact modmail by DMing this bot."));

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
      
        var oldPostId = Global.InfoRoleMessageIds[0];
        var secondOldPost = Global.InfoRoleMessageIds[1];
        var infoChannel = guild.GetTextChannel(infoChannelId);
        var oldMeessage = await infoChannel.GetMessageAsync(oldPostId);
        if(oldMeessage != null) 
        {
          await oldMeessage.DeleteAsync();
        }
        var message = await infoChannel.SendMessageAsync(embed: embedBuilder.Build());
        db.PersistentData.AsQueryable().Where(dt => dt.Name == "rolemanager_message_id").First().Value = message.Id;
        Global.InfoRoleMessageIds.Clear();
        Global.InfoRoleMessageIds.Add(message.Id);
        await message.AddReactionsAsync(reactions.ToArray());

        embedBuilder = new EmbedBuilder();
        embedBuilder.WithColor(15258703);

        var secondRoles = db.ReactionRoles.Include(ro => ro.EmoteReference).Include(ro => ro.RoleReference).Where(ro => ro.Area == "info_2").OrderBy(ro => ro.Position).ToList();
        var secondReactions = new List<IEmote>();
        foreach(var role in secondRoles)
        {
          var roleInGuild = guild.GetRole(role.RoleReference.RoleID);
          var emote = role.EmoteReference.GetAsEmote();
          embedBuilder.AddField(emote + " " + roleInGuild.Name, "\u200B", true);
          secondReactions.Add(emote);
        }
      
        
        var secondOldMessage = await infoChannel.GetMessageAsync(secondOldPost);
        if(secondOldMessage != null) 
        {
          await secondOldMessage.DeleteAsync();
        }
        var secondMessage = await infoChannel.SendMessageAsync(embed: embedBuilder.Build());
        Global.InfoRoleMessageIds.Add(secondMessage.Id);
        db.PersistentData.AsQueryable().Where(dt => dt.Name == "rolemanager_message_id_2").First().Value = secondMessage.Id;
        await secondMessage.AddReactionsAsync(secondReactions.ToArray());
        db.SaveChanges();
      }
    }
  }
}