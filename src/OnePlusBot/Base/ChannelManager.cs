using System;
using System.Threading.Tasks;
using Discord;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using Discord.WebSocket;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using OnePlusBot.Base.Errors;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace OnePlusBot.Base
{
    public class ChannelManager
    {
        public void createChannelGroup(string name)
        {
            using(var db = new Database())
            {
                var existingGroup = db.ChannelGroups.Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null)
                {
                    throw new NotFoundException("Channel group with name already exists.");
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
                        if(!doesChannelGroupEntryAlreadyExist)
                        {
                            var channelGroupMember = new ChannelInGroup();
                            channelGroupMember.ChannelGroupId = existingGroup.Id;
                            channelGroupMember.ChannelId = channel.Id;
                            db.ChannelGroupMembers.Add(channelGroupMember);
                        }
                      
                    }
                    db.SaveChanges();
                }
                else 
                {
                    throw new NotFoundException("Channel group not found.");
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
                else 
                {
                    throw new NotFoundException("Channel group not found.");
                }
            }
        }

        public void SetChannelGroupAttributes(string name, bool newVal, Boolean? inviteCheck, Boolean? profanityCheck)
        {
            using(var db = new Database())
            {
                var existingGroup = db.ChannelGroups.Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null)
                {
                    existingGroup.ExperienceGainExempt = newVal;
                    if(inviteCheck != null)
                    {
                      existingGroup.InviteCheckExempt = inviteCheck.GetValueOrDefault();
                    }
                    if(profanityCheck != null)
                    {
                      existingGroup.ProfanityCheckExempt = profanityCheck.GetValueOrDefault();
                    }
                }
                else 
                {
                    throw new NotFoundException("Channel group not found.");
                }
                db.SaveChanges();
            }
        }

        public void setGroupDisabledTo(string name, bool newVal)
        {
            using(var db = new Database())
            {
                var existingGroup = db.ChannelGroups.Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null)
                {
                    existingGroup.Disabled = newVal;
                }
                else 
                {
                    throw new NotFoundException("Channel group not found.");
                }
                db.SaveChanges();
            }

        }

        public async Task PostExistingPostTargets(ISocketMessageChannel channelToRespondIn) 
        {
          var stringBuilder = new StringBuilder();
          foreach(var target in PostTarget.POST_TARGETS)
          {
            stringBuilder.Append($"`{target}` ");
          }
          var builder = new EmbedBuilder();
          builder.WithTitle("Currently available post targets");
          builder.WithDescription(stringBuilder.ToString());
          await channelToRespondIn.SendMessageAsync(embed: builder.Build());
        }

        public void RenameChannelGroup(string oldName, string newName) 
        {
          using(var db = new Database())
          {
            var existingGroup = db.ChannelGroups.Where(grp => grp.Name == oldName).FirstOrDefault();
            if(existingGroup != null)
            {
              var newNameIsused = db.ChannelGroups.Where(grp => grp.Name == newName).Any();
              if(newNameIsused)
              {
                throw new ConfigurationException("New name is already used by another group.");
              }
              existingGroup.Name = newName;
            }
            else 
            {
                throw new NotFoundException("Channel group not found.");
            }
            db.SaveChanges();
          }
        }

        public void SetPostTarget(string name, IChannel channel)
        {
            using(var db = new Database())
            {
                var existingTarget = db.PostTargets.Where(pt => pt.Name == name).FirstOrDefault();
                PostTarget newPostTarget;
                if(existingTarget != null)
                {
                    newPostTarget = existingTarget;
                    newPostTarget.ChannelId = channel.Id;
                }
                else
                {
                    newPostTarget = new PostTarget();
                    newPostTarget.Name = name;
                    newPostTarget.ChannelId = channel.Id;
                    db.PostTargets.Add(newPostTarget);
                }
                db.SaveChanges();
            }
        }

        public Collection<Embed> GetChannelListEmbed()
        {
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
                    var disabledIndicator = group.Disabled ? " (Disabled)" : "";
                    currentEmbedBuilder.AddField($"**{group.Name}**{disabledIndicator}, XP exempt: {group.ExperienceGainExempt}  Profanity check exempt: {group.ProfanityCheckExempt}, InviteCheck exempt: {group.InviteCheckExempt}", getChannelsAsMentions(group.Channels));
                    if(((count % EmbedBuilder.MaxFieldCount) == 0) && group != channelGroups.Last())
                    {
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
            foreach(Embed embed in embedsToPost)
            {
                await channelToRespondIn.SendMessageAsync(embed: embed);
                await Task.Delay(200);
            }
        }

        private string getChannelsAsMentions(ICollection<ChannelInGroup> channels)
        {
            StringBuilder stringRepresentation = new StringBuilder();
            foreach(ChannelInGroup ch in channels)
            {
                stringRepresentation.Append($"<#{ch.ChannelId}> ");
            }

            return stringRepresentation.ToString() != string.Empty ? stringRepresentation.ToString() : "no channels.";
        }

        public Dictionary<string, bool> EvaluateChannelConfiguration(IChannel channel)
        {
            using(var db = new Database())
            {
                var perms = db.ChannelGroupMembers.Include(ch => ch.Group).Include(ch => ch.ChannelReference).Where(ch => ch.ChannelId == channel.Id).ToList();
                var profanityDisabled = false;
                var inviteLinksDisabled = false;
                var xpDisabled = false;
                foreach(var groupMember in perms)
                {
                    if(!groupMember.Group.Disabled)
                    {
                        profanityDisabled |= groupMember.Group.ProfanityCheckExempt;
                        inviteLinksDisabled |= groupMember.Group.InviteCheckExempt;
                        xpDisabled |= groupMember.Group.ExperienceGainExempt;
                    }
                }
                var result = new Dictionary<string, bool>();
                result.Add("XP Gain disabled", xpDisabled);
                result.Add("Profanity check disabled", profanityDisabled);
                result.Add("Invite check disabled", inviteLinksDisabled);
                return result;
            }
        }

        public void RemoveChannelGroup(string name)
        {
            using(var db = new Database())
            {
                var existingGroup = db.ChannelGroups.Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null)
                {
                  var existingChannelEntry = db.ChannelGroupMembers.Where(mem => mem.ChannelGroupId == existingGroup.Id).FirstOrDefault();
                  if(existingChannelEntry != null)
                  {
                      throw new ConfigurationException("Channel group has configured channels. Remove them before deleting the channel group.");
                  }
                  db.ChannelGroups.Remove(existingGroup);
                }
                else 
                {
                    throw new NotFoundException("Channel group not found.");
                }
                db.SaveChanges();
            }
        }
    }
}