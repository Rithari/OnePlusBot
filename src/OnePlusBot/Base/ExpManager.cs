using Discord;
using Discord.Rest;
using System;
using System.Threading.Tasks;
using System.Linq;
using OnePlusBot.Data.Models;
using OnePlusBot.Data;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Discord.WebSocket;
using OnePlusBot.Base.Errors;
using System.Collections.Generic;

namespace OnePlusBot.Base
{
  public class ExpManager 
  {

    public async Task<RuntimeResult> SetupTimers()
    {
      TimeSpan sinceMidnight = DateTime.Now.TimeOfDay;
      TimeSpan nextMinute = TimeSpan.FromMinutes(Math.Ceiling(sinceMidnight.TotalMinutes));
      TimeSpan timeSpanToDelay = (nextMinute - sinceMidnight);
      // dont trigger exactly on the zero second, but on the :30 second and do the minute before
      int secondsToDelay = (int) timeSpanToDelay.TotalSeconds + 30;
      await Task.Delay(secondsToDelay * 1000);
      await PersistExp();
      System.Timers.Timer timer = new System.Timers.Timer(1000 * 60 * 1);
      timer.Elapsed += new System.Timers.ElapsedEventHandler(TriggerPersitence);
      timer.Enabled = true;
      return CustomResult.FromSuccess();
    }

    public async void TriggerPersitence(object sender, System.Timers.ElapsedEventArgs e)
    {
      System.Timers.Timer timer = (System.Timers.Timer)sender;
      await PersistExp();
    }     

    public async Task PersistExp()
    {
      Random rnd = new Random();
      using(var db = new Database())
      {
        // this works this way, to not leave out any past experience trackings
        var minuteToPersist = (long) DateTime.Now.Subtract(DateTime.MinValue).TotalMinutes - 1;
        Console.WriteLine($"Persisting minute {minuteToPersist} + {DateTime.Now}");
        var peopleToUpdate = new HashSet<User>();
        // if for some reason the elements were not persistet in the past rounds
        // they will in the future, anda because they are removed afterwards, this means that there *should* not be anything done twice
        var minutesInThePast = Global.RuntimeExp.Keys.Where(minute => minute <= minuteToPersist);
        List<long> toRemove = new List<long>();
        if(minutesInThePast.Any())
        {
          foreach(var processedMinute in minutesInThePast)
          {
            UpdateExperienceForMinute(Global.RuntimeExp[processedMinute], db, peopleToUpdate, rnd);
            toRemove.Add(processedMinute);
          }
        }
        foreach(var minuteToRemove in toRemove)
        {
          Global.RuntimeExp.TryRemove(minuteToRemove, out _);
        }
        db.SaveChanges();
        if(peopleToUpdate.Count > 0)
        {
          var peopleWhoChangedLevel = new List<User>();
          foreach(var person in peopleToUpdate)
          {
            var levelSegment = GetAppropriateLevelForExp(person.XP, db);
            if(levelSegment != null && person.Level != levelSegment.Level)
            {
              peopleWhoChangedLevel.Add(person);
              person.Level = levelSegment.Level;
              db.Entry(person).Reference(s => s.CurrentLevel).Load();
            }
          }
          if(peopleWhoChangedLevel.Count > 0)
          {
            var guild = Global.Bot.GetGuild(Global.ServerID);
            var rolesGiven = new Dictionary<ulong, Discord.IRole>();
            foreach(var person in peopleWhoChangedLevel)
            {
              var roleSegment = GetAppropriateRoleForLevel(person.CurrentLevel, db);
              // null in case of first role
              if(roleSegment != null)
              {
                if(roleSegment.ExperienceRoleId != person.ExperienceRoleId)
                {
                  if(!rolesGiven.ContainsKey(roleSegment.RoleReference.RoleID))
                  {
                    rolesGiven.Add(roleSegment.RoleReference.RoleID, guild.GetRole(roleSegment.RoleReference.RoleID));
                  }
                  ulong existingDiscordRoleFromUser = 0;
                  if(person.ExperienceRoleReference != null)
                  {
                    existingDiscordRoleFromUser = person.ExperienceRoleReference.RoleReference.RoleID;
                
                    if(!rolesGiven.ContainsKey(existingDiscordRoleFromUser))
                    {
                      rolesGiven.Add(existingDiscordRoleFromUser, guild.GetRole(existingDiscordRoleFromUser));
                    }
                  }
                  
                
                  person.ExperienceRoleId = roleSegment.Id;
                  var user = guild.GetUser(person.Id);
                  if(user != null) 
                  {
                    if(existingDiscordRoleFromUser != 0)
                    {
                      await user.RemoveRoleAsync(rolesGiven[existingDiscordRoleFromUser]);
                    }
                  
                    await user.AddRoleAsync(rolesGiven[roleSegment.RoleReference.RoleID]);
                  }
                
                }
              }
            }
          }
        }

        db.SaveChanges();
      }
    }
     
    public ExperienceLevel GetAppropriateLevelForExp(ulong xp, Database db)
    {
      return db.ExperienceLevels.Where(lv => lv.NeededExperience <= xp).OrderByDescending(lv => lv.Level).FirstOrDefault();
    }

    public ExperienceRole GetAppropriateRoleForLevel(ExperienceLevel level, Database db)
    {
      return db.ExperienceRoles.Where( ro => ro.Level <= level.Level).Include(ro => ro.RoleReference).OrderByDescending(ro => ro.Level).FirstOrDefault();
    }

    public void UpdateExperienceForMinute(List<ulong> userToUpdate, Database db, HashSet<User> peopleToUpdate, Random r)
    {
      var updateDate = DateTime.Now;
      foreach(var userId in userToUpdate)
      {
        var exp = db.Users.Where(e => e.Id == userId).Include(u => u.ExperienceRoleReference).ThenInclude(u => u.RoleReference).FirstOrDefault();
        var gainedExp = (ulong) r.Next(Global.XPGainRangeMin, Global.XPGainRangeMax);
        if(exp != null)
        {
          if(exp.XPGainDisabled) 
          {
            continue;
          }
          exp.XP += gainedExp;
          exp.MessageCount += 1;
          peopleToUpdate.Add(exp);
          exp.Updated = updateDate;
        } 
        else 
        {
          var userBuilder = new UserBuilder(userId).WithMessageCount(1).WithXP(gainedExp);
          var user = userBuilder.Build();
          db.Users.Add(user);
          peopleToUpdate.Add(user);
        }
      }
    }

    public async Task UpdateLevelOf(IGuildUser user)
    {
        var guild = Global.Bot.GetGuild(Global.ServerID);
        using(var db = new Database())
        {
          User userToUpdate = db.Users.Where(us => us.Id == user.Id).Include(u => u.ExperienceRoleReference).ThenInclude(u => u.RoleReference).FirstOrDefault();
          List<ExperienceRole> rolesUsedInExperience = db.ExperienceRoles.Include(ro => ro.RoleReference).ToList();
          List<ExperienceLevel> levelConfiguration = db.ExperienceLevels.ToList();
          List<SocketRole> experienceRolesInGuild = new List<SocketRole>();
          foreach(ExperienceRole role in rolesUsedInExperience)
          {
            experienceRolesInGuild.Add(guild.GetRole(role.RoleReference.RoleID));
          }
   
          await UpdateLevelsForUser(userToUpdate, db, guild, rolesUsedInExperience, levelConfiguration, experienceRolesInGuild);
          db.SaveChanges();
        }
    }

    private async Task UpdateLevelsForUser(User user, Database db, 
                                          SocketGuild guild,
                                          List<ExperienceRole> rolesUsedInExperience,
                                          List<ExperienceLevel> levelConfiguration,
                                          List<SocketRole> experienceRolesInGuild,
                                          bool delay=false)
                                          {

      var appropriateLevelForExp = levelConfiguration.Where(lv => lv.NeededExperience <= user.XP).OrderByDescending(lv => lv.Level).FirstOrDefault();
      // the role may need to be updated, even if the level did not change, when the role config changed, so we need to always enter this if
      if(appropriateLevelForExp != null)
      {
        user.Level = appropriateLevelForExp.Level;
        db.Entry(user).Reference(s => s.CurrentLevel).Load();
        var appropriateRoleForLevel = rolesUsedInExperience.Where(lv => lv.Level <= user.Level).OrderByDescending(ro => ro.Level).FirstOrDefault();
        if(appropriateRoleForLevel != null && user.ExperienceRoleId != appropriateRoleForLevel.ExperienceRoleId)
        {
          user.ExperienceRoleId = appropriateRoleForLevel.Id;
          db.Entry(user).Reference(s => s.ExperienceRoleReference).Load();
          db.Entry(user.ExperienceRoleReference).Reference(s => s.RoleReference).Load();
        }
      }
      else 
      {
        user.Level = 0;
        user.ExperienceRoleId = null;
        db.Entry(user).Reference(s => s.ExperienceRoleReference).Load();
      }

      var userInGuild = guild.GetUser(user.Id);
      if(userInGuild != null)
      {
        var experienceRolesTheUserHas = userInGuild.Roles.Intersect(experienceRolesInGuild).ToList();
        if(user.ExperienceRoleReference == null)
        {
          await userInGuild.RemoveRolesAsync(experienceRolesTheUserHas);
        } 
        else
        {
          var userHasCorrectRoles = experienceRolesTheUserHas.Count() == 1 && user.ExperienceRoleReference.RoleReference.RoleID == experienceRolesTheUserHas.First().Id;
          if(!userHasCorrectRoles)
          {
            if(experienceRolesTheUserHas.Count() > 0)
            {
              await userInGuild.RemoveRolesAsync(experienceRolesInGuild);
            }
            if(delay)
            {
              await Task.Delay(200);
            }
            if(user.ExperienceRoleId != null)
            {
              var correctExperienceRole = experienceRolesInGuild.Where(role => role.Id == user.ExperienceRoleReference.RoleReference.RoleID).FirstOrDefault();
              if(correctExperienceRole != null)
              {
                await userInGuild.AddRoleAsync(correctExperienceRole);
              }
            }
          
          }
        }
        
      }
    }

    public void UpdateLevelsOfMembers(RestUserMessage updateMessage){
      Task.Run(async () => 
      {
        var guild = Global.Bot.GetGuild(Global.ServerID);
        await updateMessage.ModifyAsync(m => m.Content = "0 % Done");
        using(var db = new Database())
        {
          List<User> users = db.Users.Include(u => u.ExperienceRoleReference).ThenInclude(u => u.RoleReference).ToList();
          var totalUsers = users.Count();
          var userDone = 0;
          List<ExperienceRole> rolesUsedInExperience = db.ExperienceRoles.Include(ro => ro.RoleReference).ToList();
          List<ExperienceLevel> levelConfiguration = db.ExperienceLevels.ToList();
          List<SocketRole> experienceRolesInGuild = new List<SocketRole>();
          foreach(ExperienceRole role in rolesUsedInExperience)
          {
            experienceRolesInGuild.Add(guild.GetRole(role.RoleReference.RoleID));
          }
   
          foreach(var user in users)
          {
            await UpdateLevelsForUser(user, db, guild, rolesUsedInExperience, levelConfiguration, experienceRolesInGuild, true);
           
            userDone++;
            if(userDone % (Math.Floor((double) totalUsers / 10)) == 0)
            {
              await updateMessage.ModifyAsync(m => m.Content = $"{Math.Ceiling(((double)userDone / totalUsers) * 100)}% ({userDone}/{totalUsers}) done");
            }
            db.SaveChanges();
          }
          await updateMessage.ModifyAsync(m => m.Content = "Finished");
        }
      }).ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
     
     
    }

    public void SetRoleToLevel(uint level, ulong roleId)
    {
      using(var db = new Database())
      {
        var existingLevel = db.ExperienceRoles.Where(ro => ro.Level == level).FirstOrDefault();

        var existingRole = db.ExperienceRoles.Where(ro => ro.ExperienceRoleId == roleId).FirstOrDefault();
        var role = db.Roles.Where(r => r.RoleID == roleId).FirstOrDefault();
        if(role == null || !role.XPRole)
        {
          throw new ConfigurationException("Role does not exist or not usable for the xp tracking sytem.");
        }
        if(existingLevel != null)
        {
          existingLevel.ExperienceRoleId = role.RoleID;
        }
        else if(existingRole != null)
        {
          existingRole.Level = level;
        }
        else
        {
          var experienceRole = new ExperienceRole();
          experienceRole.Level = level;
          experienceRole.ExperienceRoleId = role.RoleID;
          db.ExperienceRoles.Add(experienceRole);
        }
        db.SaveChanges();
      }
    }

    public async Task ShowLevelconfiguration(ISocketMessageChannel channelToRespondIn)
    {
      using(var db = new Database())
      {
        var guild = Global.Bot.GetGuild(Global.ServerID);
        var currentEmbedBuilder = new EmbedBuilder().WithTitle("Role level configuration");
        var embeds = new List<Embed>();
        var roles = db.ExperienceRoles.OrderBy(ro => ro.Level).Include(ro => ro.RoleReference).ToList();
        var count = 0;
        foreach(var role in roles)
        {
          count++;
          currentEmbedBuilder.AddField(guild.GetRole(role.RoleReference.RoleID).Name, role.Level + "", true);
            if(((count % EmbedBuilder.MaxFieldCount) == 0) && role != roles.Last())
            {
              embeds.Add(currentEmbedBuilder.Build());
              currentEmbedBuilder = new EmbedBuilder();
              var currentPage = count / EmbedBuilder.MaxFieldCount + 1;
              currentEmbedBuilder.WithFooter(new EmbedFooterBuilder().WithText($"Page {currentPage}"));
            }
        }

        embeds.Add(currentEmbedBuilder.Build());

        foreach(var embed in embeds)
        {
          await channelToRespondIn.SendMessageAsync(embed: embed);
          await Task.Delay(200);
        }

      }
    }

    public void SetXPDisabledTo(IGuildUser user, bool newValue){
      using(var db = new Database())
      {
        var userDB = db.Users.Where(us => us.Id == user.Id).FirstOrDefault();
        if(userDB == null)
        {
          throw new NotFoundException("User not found");
        }
        else 
        {
          userDB.XPGainDisabled = newValue;
        }
        db.SaveChanges();
      }

    }
  
  }
}