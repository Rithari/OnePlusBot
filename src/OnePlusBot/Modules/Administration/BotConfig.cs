using System.Threading.Tasks;
using Discord.Commands;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using System.Linq;
using OnePlusBot.Base.Errors;

namespace OnePlusBot.Modules.Administration
{
    public partial class Administration : ModuleBase<SocketCommandContext>
    {

         [
            Command("reloaddb"),
            Summary("Reloades the cached info from db"),
            RequireRole("staff")
        ]
        public async Task<RuntimeResult> ReloadDB()
        {
            await Task.Delay(500);
            Global.LoadGlobal();
            return CustomResult.FromSuccess();
        }

        [
            Command("setstars"),
            Summary("sets the amount required to appear on the starboard"),
            RequireRole("staff"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> SetStars(string input)
        {
            ulong amount = 0;
            await Task.Delay(25);
            if(ulong.TryParse(input, out amount) && amount > 0){
                using (var db = new Database()){
                    var point = db.PersistentData.First(entry => entry.Name == "starboard_stars");
                    point.Value = amount;
                    db.SaveChanges();
                }
                Global.StarboardStars = amount;
                return CustomResult.FromSuccess();
            } 
            else
            {
                return CustomResult.FromError("whole numbers > 0 only");
            }
           
        }

         /// <summary>
        /// Sets the flag of the command identified by the given name in the channel group identified by the given group name to the given value
        /// </summary>
        /// <exception cref="OnePlusBot.Base.Errors.NotFoundException">In case no channel group or command with that name is found</exception>
        /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
        [
            Command("disableCommand"),
            Summary("Disables command in a specified channel group"),
            RequireRole(new string[]{"admin", "founder"})
        ]
        public async Task<RuntimeResult> DisableCommandInGroup(string commandName, string channelGroupName, bool newValue)
        {
            using(var db = new Database())
            {
              var commandInChannelGroup = db.CommandInChannelGroups.Where(co => co.ChannelGroupReference.Name == channelGroupName && co.CommandReference.Name == commandName);
              if(commandInChannelGroup.Any())
              {
                commandInChannelGroup.First().Disabled = newValue;
              }
              else 
              {
                var command = db.Commands.Where(co => co.Name == commandName);
                if(command.Any())
                {
                  var channelGroup = db.ChannelGroups.Where(chgrp => chgrp.Name == channelGroupName);
                  if(channelGroup.Any())
                  {
                    var newCommandInChannelGroup = new CommandInChannelGroup();
                    newCommandInChannelGroup.ChannelGroupId = channelGroup.First().Id;
                    newCommandInChannelGroup.CommandID = command.First().ID; 
                    newCommandInChannelGroup.Disabled = newValue;
                    db.CommandInChannelGroups.Add(newCommandInChannelGroup);
                  } 
                  else 
                  {
                    throw new NotFoundException("Channel group not found");
                  }
                }
                else 
                {
                  throw new NotFoundException("Command not found");
                }
                
              }
              db.SaveChanges();
            }
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }


        /// <summary>
        /// Creates the post in info responsible for managing the roles
        /// </summary>
        /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
        [
            Command("setupInfoPost"),
            Summary("Sets up the info post to let user self assign the roles"),
            RequireRole(new string[]{"admin", "founder"})
        ]
        public async Task<RuntimeResult> SetupInfoPost()
        {
            await new SelfAssignabeRolesManager().SetupInfoPost();
            return CustomResult.FromSuccess();
        }
       

    }
}