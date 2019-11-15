using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using Discord.WebSocket;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using OnePlusBot.Base.Errors;
using Discord;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace OnePlusBot.Base
{
    public class ChannelManager
    {
        public void createChannelGroup(string name)
        {
            using(var db = new Database()){
                var existingGroup = db.ChannelGroups.Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null){
                    throw new ChannelGroupException("Channel group with name already exists.");
                }
                var channelGroup = new ChannelGroup();
                channelGroup.Name = name;
                db.ChannelGroups.Add(channelGroup);
                
                db.SaveChanges();
            }
        }

        public void addChannelsToChannelGroup(string name, SocketMessage message)
        {
            using(var db = new Database())
            {
                var existingGroup = db.ChannelGroups.Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null)
                {
                    foreach(var channel in message.MentionedChannels)
                    {
                        var doesChannelGroupEntryAlreadyExist = db.ChannelGroupMembers.Where(mem => mem.ChannelGroupId == existingGroup.Id && mem.ChannelId == channel.Id).Count() != 0;
                        if(!doesChannelGroupEntryAlreadyExist){
                            var channelGroupMember = new ChannelInGroup();
                            channelGroupMember.ChannelGroupId = existingGroup.Id;
                            channelGroupMember.ChannelId = channel.Id;
                            db.ChannelGroupMembers.Add(channelGroupMember);
                        }
                      
                    }
                    db.SaveChanges();
                } else {
                    throw new ChannelGroupException("Channel group not found.");
                }
                
            }
        }

        public void removeChannelsFromGroup(string name, SocketMessage message)
        {
            using(var db = new Database())
            {
                var existingGroup = db.ChannelGroups.Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null)
                {
                    foreach(var channel in message.MentionedChannels)
                    {
                        var existingChannelEntry = db.ChannelGroupMembers.Where(mem => mem.ChannelGroupId == existingGroup.Id && mem.ChannelId == channel.Id).FirstOrDefault();
                        if(existingChannelEntry != null)
                        {
                            db.ChannelGroupMembers.Remove(existingChannelEntry);
                        }
                    }
                    db.SaveChanges();
                } else {
                    throw new ChannelGroupException("Channel group not found.");
                }
                
            }
        }

        public void setExpDisabledTo(string name, bool newVal){
            using(var db = new Database())
            {
                var existingGroup = db.ChannelGroups.Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null)
                {
                    existingGroup.ExperienceGainExempt = newVal;
                } else {
                    throw new ChannelGroupException("Channel group not found.");
                }
                db.SaveChanges();
            }

        }

        public void setGroupDisabledTo(string name, bool newVal){
            using(var db = new Database())
            {
                var existingGroup = db.ChannelGroups.Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null)
                {
                    existingGroup.Disabled = newVal;
                } else {
                    throw new ChannelGroupException("Channel group not found.");
                }
                db.SaveChanges();
            }

        }

        public void setPostTarget(string name, SocketUserMessage message)
        {
            ulong channelIdToSet = message.MentionedChannels.First().Id;
            using(var db = new Database())
            {
                var existingTarget = db.PostTargets.Where(pt => pt.Name == name).FirstOrDefault();
                PostTarget newPostTarget;
                if(existingTarget != null)
                {
                    newPostTarget = existingTarget;
                    newPostTarget.ChannelId = channelIdToSet;
                }
                else
                {
                    newPostTarget = new PostTarget();
                    newPostTarget.Name = name;
                    newPostTarget.ChannelId = channelIdToSet;
                    db.PostTargets.Add(newPostTarget);
                }
              
                db.SaveChanges();
            }
        }

        public Collection<Embed> GetChannelListEmbed(){
            Collection<Embed> embedsToPost = new Collection<Embed>();
            using(var db = new Database())
            {
                var currentEmbedBuilder = new EmbedBuilder();
                currentEmbedBuilder.WithTitle("Available channel groups");
                var channelGroups = db.ChannelGroups.Include(ch => ch.Channels).ToList();
                var count = 0;
                foreach(var group in channelGroups)
                {
                    count++;
                    currentEmbedBuilder.AddField(group.Name, getChannelsAsMentions(group.Channels));
                    if(((count % EmbedBuilder.MaxFieldCount) == 0) && group != channelGroups.Last()){
                        embedsToPost.Add(currentEmbedBuilder.Build());
                        currentEmbedBuilder = new EmbedBuilder();
                        var currentPage = count / EmbedBuilder.MaxFieldCount + 1;
                        currentEmbedBuilder.WithFooter(new EmbedFooterBuilder().WithText($"Page {currentPage}"));
                    }
                   
                }
                embedsToPost.Add(currentEmbedBuilder.Build());
            }
            return embedsToPost;
        }

        public async Task ListChannelGroups(ISocketMessageChannel channelToRespondIn)
        {
            var embedsToPost = GetChannelListEmbed();
            foreach(Embed embed in embedsToPost){
                await channelToRespondIn.SendMessageAsync(embed: embed);
                await Task.Delay(200);
            }
        }

        private string getChannelsAsMentions(ICollection<ChannelInGroup> channels){
            StringBuilder stringRepresentation = new StringBuilder();
            foreach(ChannelInGroup ch in channels){
                stringRepresentation.Append($"<#{ch.ChannelId}> ");
            }

            return stringRepresentation.ToString() != string.Empty ? stringRepresentation.ToString() : "no channels.";
        }
    }
}