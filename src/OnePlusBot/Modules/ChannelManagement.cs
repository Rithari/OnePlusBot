using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Discord.Commands;
using OnePlusBot.Base;
using System.Linq;
using Discord;
using Discord.WebSocket;
using System.Runtime.InteropServices;

namespace OnePlusBot.Modules
{
    public class Channels : ModuleBase<SocketCommandContext>
    {

        [
            Command("createChannelGroup"),
            Summary("Creates a channel group to be used in other areas"),
            RequireRole("staff"),
            Alias("createChGrp"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> CreateChannelGroup(string groupName)
        {
            new ChannelManager().createChannelGroup(groupName);
            await Task.CompletedTask;
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
            Command("setPostTarget"),
            Summary("Sets the target of a certain post"),
            RequireRole("staff"),
            Alias("setTarget"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> SetPostTarget([Optional] string channelName, [Optional] ISocketMessageChannel channel)
        {
            if(channelName == null || channel == null)
            {
              await new ChannelManager().PostExistingPostTargets(Context.Channel);
            }
            else
            {
              new ChannelManager().SetPostTarget(channelName, channel);
            }
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
            Summary("Prints all the channel groups of the server"),
            RequireRole("staff"),
            Alias("listChGrp"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ListChannelGroups()
        {
            await new ChannelManager().ListChannelGroups(Context.Channel);
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

        [
            Command("listCommands"),
            Summary("Lists the groups a channel is configured to be disabled/enabled"),
            RequireRole("staff"),
            Alias("lsCmd")
        ]
        public async Task<RuntimeResult> ListCommandsWithGroups()
        {
            await new ChannelManager().ListCommandsWithGroups(Context.Channel);
            return CustomResult.FromSuccess();
        }

    }
}