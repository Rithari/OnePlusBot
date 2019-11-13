using System.Linq;
using Discord.WebSocket;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;

namespace OnePlusBot.Base
{
    public class ChannelManager
    {
        public void createChannelGroup(string name)
        {
            using(var db = new Database()){
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
                }
                
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
    }
}