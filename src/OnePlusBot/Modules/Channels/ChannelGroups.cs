using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Discord.Commands;
using OnePlusBot.Base;
using OnePlusBot.Data.Models;
using Discord;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using Discord.Addons.Interactive;

namespace OnePlusBot.Modules.Channels
{
    public partial class Channels : InteractiveBase<SocketCommandContext>
    {
       /// <summary>
        /// Command responsibel for creating a channel group
        /// </summary>
        /// <param name="groupName">The name of the channel group to create</param>
        /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
        [
            Command("createChannelGroup", RunMode = RunMode.Async),
            Summary("Creates a channel group to be used in other areas"),
            RequireRole("staff"),
            Alias("createChGrp"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> CreateChannelGroup(string groupName)
        {
            var configurationStep = new ConfigurationStep("What type of group is this? (🏁 checks, ❓ FAQ, 🤖 command)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var addAction = new ReactionAction(new Emoji("🏁"));
            addAction.Action = async (ConfigurationStep a) => 
            {
                new ChannelManager().createChannelGroup(groupName, ChannelGroupType.CHECKS);
                await Task.CompletedTask;
            };

            var deleteCommandChannelAction = new ReactionAction(new Emoji("❓"));
            deleteCommandChannelAction.Action = async (ConfigurationStep a) => 
            {
                new ChannelManager().createChannelGroup(groupName,  ChannelGroupType.FAQ);
                await Task.CompletedTask;
            };

            var deleteCommandAction = new ReactionAction(new Emoji("🤖"));
            deleteCommandAction.Action = async (ConfigurationStep a) => 
            {
                new ChannelManager().createChannelGroup(groupName,  ChannelGroupType.COMMANDS);
                await Task.CompletedTask;
            };

            configurationStep.Actions.Add(addAction);
            configurationStep.Actions.Add(deleteCommandChannelAction);
            configurationStep.Actions.Add(deleteCommandAction);
            
            await configurationStep.SetupMessage();
            return CustomResult.FromSuccess();
        }

        [
            Command("addGroupChannels"),
            Summary("Adds the mentioned channels to the given channel group"),
            RequireRole("staff"),
            Alias("addGrpCh"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> AddToChannelGroup(string groupName, [Remainder] string text)
        {
            var parts = text.Split(' ');
            if(parts.Length < 1)
            {
                return CustomResult.FromError("syntax <name> <mentioned channels to be added>");
            }
            new ChannelManager().addChannelsToChannelGroup(groupName, Context.Message);
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }

        [
            Command("removeGroupChannels"),
            Summary("Removes the mentioned channels from the given channel group"),
            RequireRole("staff"),
            Alias("rmGrpCh"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> RemoveFromChannelGroup(string groupName, [Remainder] string text)
        {
            var parts = text.Split(' ');
            if(parts.Length < 1)
            {
                return CustomResult.FromError("syntax <name> <mentioned channels to be added>");
            }
            new ChannelManager().removeChannelsFromGroup(groupName, Context.Message);
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }

        [
            Command("channelGroupAttributes"),
            Summary("Enables/disables the attribues in a certain channel group"),
            RequireRole(new string[]{"admin", "founder"}),
            Alias("chGrpAtt"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ToggleChannelGroupAttributes(string groupName, bool xpGain, [Optional] bool? profanityCheck, [Optional] Boolean? inviteCheck)
        {
            new ChannelManager().SetChannelGroupAttributes(groupName, xpGain, inviteCheck, profanityCheck);
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }

        [
            Command("disableChannelGroup"),
            Summary("Enables/disables the profanity/invitecheck/xpgain flags for a group"),
            RequireRole(new string[]{"admin", "founder"}),
            Alias("disableChGrp"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ToggleGroupDisabled(string groupName, bool newValue){
          
            new ChannelManager().setGroupDisabledTo(groupName, newValue);
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }

        [
            Command("renameChannelGroup"),
            Summary("Sets the target of a certain post"),
            RequireRole("staff"),
            Alias("rnChGrp"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> RenameChannelGroup(string oldName, string newName)
        {
            new ChannelManager().RenameChannelGroup(oldName, newName);
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }

        [
            Command("listChannelGroups", RunMode=RunMode.Async),
            Summary("Prints all the channel groups of the server (optional type: COMMANDS, CHECKS, FAQ)"),
            RequireRole("staff"),
            Alias("listChGrp"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ListChannelGroups([Optional] string type)
        {
            await new ChannelManager().ListChannelGroups(Context.Channel, type);
            return CustomResult.FromSuccess();
        }

        [
            Command("showChannelConfig"),
            Summary("Show the configuration (disableXP/invitecheck/profanity) of the given or current channel"),
            RequireRole("staff"),
            Alias("shChCfg"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ShowChannelConfig([Optional] ISocketMessageChannel channel)
        {
            if(channel == null)
            {
                channel = Context.Channel;
            }
            Dictionary<string, bool> bools = new ChannelManager().EvaluateChannelConfiguration(channel);
            var embedBuilder = new EmbedBuilder().WithTitle($"Current configuration for {channel.Name}");
            foreach(var config in bools.Keys)
            {
                embedBuilder.AddField(config, bools[config]);
            }
            await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
            return CustomResult.FromSuccess();
        }

        [
            Command("removeChannelGroup"),
            Summary("Deletes a channel group (will fail if the group is used anywhere)"),
            RequireRole("staff"),
            Alias("rmChGrp"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> RemoveChannelGroup(string name)
        {
            new ChannelManager().RemoveChannelGroup(name);
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }

        /// <summary>
        /// Command used to display the groups of type COMMANDS, and the respective commands configured to be in this group
        /// </summary>
        /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
        [
            Command("listGroupCommands"),
            Summary("Lists the groups which have specific commands defined for them"),
            RequireRole("staff"),
            Alias("lsGrpCmd"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ListCommandsWithGroups()
        {
            await new ChannelManager().ListGroupsWithCommands(Context.Channel);
            return CustomResult.FromSuccess();
        }

        /// <summary>
        /// Command used to change the type of a channel group
        /// </summary>
        /// <param name="groupName">The name of the channel group to change the type for</param>
        /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
        [
            Command("changeGroupType", RunMode = RunMode.Async),
            Summary("Changes the type of a group"),
            RequireRole("staff"),
            Alias("chGrpTyp"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ChangeGropTypeTo(string groupName)
        {
          var configurationStep = new ConfigurationStep("What type should the group have? (🏁 checks, ❓ FAQ, 🤖 command)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
          var addAction = new ReactionAction(new Emoji("🏁"));
          addAction.Action = async (ConfigurationStep a) => 
          {
              new ChannelManager().ChangeChannelGroupTypeTo(groupName, ChannelGroupType.CHECKS);
              await Task.CompletedTask;
          };

          var deleteCommandChannelAction = new ReactionAction(new Emoji("❓"));
          deleteCommandChannelAction.Action = async (ConfigurationStep a) => 
          {
              new ChannelManager().ChangeChannelGroupTypeTo(groupName,  ChannelGroupType.FAQ);
              await Task.CompletedTask;
          };

          var deleteCommandAction = new ReactionAction(new Emoji("🤖"));
          deleteCommandAction.Action = async (ConfigurationStep a) => 
          {
              new ChannelManager().ChangeChannelGroupTypeTo(groupName,  ChannelGroupType.COMMANDS);
              await Task.CompletedTask;
          };

          configurationStep.Actions.Add(addAction);
          configurationStep.Actions.Add(deleteCommandChannelAction);
          configurationStep.Actions.Add(deleteCommandAction);
            
          await configurationStep.SetupMessage();
         
          return CustomResult.FromSuccess();
        }

    }
}