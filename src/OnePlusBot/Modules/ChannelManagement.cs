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
            Command("chreateChannelGroup"),
            Summary("Creates a channel group to be used in other areas"),
            RequireRole("staff"),
            Alias("createChGrp")
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
            Alias("addGrpCh")
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
            Summary("Remoes the mentioned channels from the given channel group"),
            RequireRole("staff"),
            Alias("rmGrpCh")
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
            Command("disableXP"),
            Summary("Enables/disables the xp gain in a certain channel group"),
            RequireRole("staff")
        ]
        public async Task<RuntimeResult> ToggleExperienceGainInChannelGroup(string groupName, bool newValue)
        {
            new ChannelManager().setExpDisabledTo(groupName, newValue);
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }

        [
            Command("disableChannelGroup"),
            Summary("Enables/disables the profanity/invitecheck/xpgain flags for a group"),
            RequireRole("staff"),
            Alias("disableChGrp")
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
            Alias("setTarget")
        ]
        public async Task<RuntimeResult> SetPostTarget(string channelName, ISocketMessageChannel channel)
        {
            new ChannelManager().setPostTarget(channelName, channel);
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }

        [
            Command("listChannelGroups", RunMode=RunMode.Async),
            Summary("Prints all the channel groups of the server"),
            RequireRole("staff"),
            Alias("listChGrp")
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
            Alias("shChCfg")
        ]
        public async Task<RuntimeResult> ShowChannelConfig([Optional] ISocketMessageChannel channel)
        {
            if(channel == null)
            {
                channel = Context.Channel;
            }
            Dictionary<string, bool> bools = new ChannelManager().EvaluateChannelConfiguration(channel);
            var embedBuilder = new EmbedBuilder().WithTitle($"Current configuration for {channel.Name}");
            foreach(var config in bools.Keys){
                embedBuilder.AddField(config, bools[config]);
            }
            await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
            return CustomResult.FromSuccess();
        }

        [
            Command("removeChannelGroup"),
            Summary("Deletes a channel group (will fail if the group is used anywhere)"),
            RequireRole("staff"),
            Alias("rmChGrp")
        ]
        public async Task<RuntimeResult> RemoveChannelGroup(string name)
        {
            new ChannelManager().RemoveChannelGroup(name);
            await Task.CompletedTask;
            return CustomResult.FromSuccess();
        }

    }
}