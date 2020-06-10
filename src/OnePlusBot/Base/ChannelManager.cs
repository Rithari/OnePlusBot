using System.Net.Http.Headers;
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
        /// <summary>
        /// Creates the channel group in the database with the given channel group type
        /// </summary>
        /// <param name="name">The name of the channel group which should be created</param>
        /// <param name="type">The type of the channel group to create</param>
        /// <exception cref="OnePlusBot.Base.Errors.ConfigurationException">In case a channel group already exists with this name</exception>
      
        public void createChannelGroup(string name, string type)
        {
            using(var db = new Database())
            {
                var existingGroup = db.ChannelGroups.AsQueryable().Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null)
                {
                    throw new ConfigurationException("Channel group with name already exists.");
                }
                var channelGroup = new ChannelGroup();
                channelGroup.Name = name;
                channelGroup.ChannelGroupType = type;
                db.ChannelGroups.Add(channelGroup);
                
                db.SaveChanges();
            }
        }

        public void addChannelsToChannelGroup(string name, SocketMessage message)
        {
            using(var db = new Database())
            {
                var existingGroup = db.ChannelGroups.AsQueryable().Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null)
                {
                    foreach(var channel in message.MentionedChannels)
                    {
                        var doesChannelGroupEntryAlreadyExist = db.ChannelGroupMembers.AsQueryable().Where(mem => mem.ChannelGroupId == existingGroup.Id && mem.ChannelId == channel.Id).Count() != 0;
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
                var existingGroup = db.ChannelGroups.AsQueryable().Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null)
                {
                    foreach(var channel in message.MentionedChannels)
                    {
                        var existingChannelEntry = db.ChannelGroupMembers.AsQueryable().Where(mem => mem.ChannelGroupId == existingGroup.Id && mem.ChannelId == channel.Id).FirstOrDefault();
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
                var existingGroup = db.ChannelGroups.AsQueryable().Where(grp => grp.Name == name).FirstOrDefault();
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
                var existingGroup = db.ChannelGroups.AsQueryable().Where(grp => grp.Name == name).FirstOrDefault();
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
          if(PostTarget.POST_TARGETS.Count() == 0)
          {
            stringBuilder.Append("no targets available");
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
            var existingGroup = db.ChannelGroups.AsQueryable().Where(grp => grp.Name == oldName).FirstOrDefault();
            if(existingGroup != null)
            {
              var newNameIsused = db.ChannelGroups.AsQueryable().Where(grp => grp.Name == newName).Any();
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
        /// <summary>
        /// Sets the target of the post target defined by the name to the given channel in the database. Does not reload the Global object. 
        /// In case the post target was valid, but not yet defined, this method creates a new one.
        /// </summary>
        /// <param name="name">Name of the post target to change</param>
        /// <param name="channel">The <see cref="Discord.IChannel"/> object the posts of this post target should be posted towards</param>
        /// <exception cref="OnePlusBot.Base.Errors.NotFoundException">In case the desired post target is not found</exception>
        public void SetPostTarget(string name, IChannel channel)
        {
            using(var db = new Database())
            {
                var existingTarget = db.PostTargets.AsQueryable().Where(pt => pt.Name == name);
                PostTarget newPostTarget;
                if(existingTarget.Any())
                {
                    newPostTarget = existingTarget.First();
                    newPostTarget.ChannelId = channel.Id;
                }
                else
                {
                  if(!PostTarget.POST_TARGETS.Where(t => t == name).Any())
                  {
                    throw new NotFoundException("Post target does not exist.");
                  }
                  newPostTarget = new PostTarget();
                  newPostTarget.Name = name;
                  newPostTarget.ChannelId = channel.Id;
                  db.PostTargets.Add(newPostTarget);
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Renders the configured channel groups of the given type (if type is null, all of them) into a collection of embeds.
        /// Included information is: xp disabled, profanity check disabled, invite check disabled, whether or not the group is disabled and the type of the group
        /// </summary>
        /// <param name="type">The type of group you want to render, if null, it will render all</param>
        /// <returns>Collection of <see cref="Discord.Embed"> representing the channel groups </returns>
        public Collection<Embed> GetChannelListEmbed(string type)
        {
            Collection<Embed> embedsToPost = new Collection<Embed>();
            using(var db = new Database())
            {
                var currentEmbedBuilder = new EmbedBuilder();
                currentEmbedBuilder.WithTitle("Available channel groups");
                List<ChannelGroup> channelGroups;
                if(type != null)
                {
                   channelGroups = db.ChannelGroups.AsQueryable().Where(grp => grp.ChannelGroupType == type).Include(ch => ch.Channels).ToList();
                }
                else
                {
                  channelGroups = db.ChannelGroups.Include(ch => ch.Channels).ToList();
                }
               
                var count = 0;
                foreach(var group in channelGroups)
                {
                    count++;
                    var disabledIndicator = group.Disabled ? " (Disabled)" : "";
                    currentEmbedBuilder.AddField($"**{group.Name}** {group.ChannelGroupType} {disabledIndicator}, XP exempt: {group.ExperienceGainExempt}  Profanity check exempt: {group.ProfanityCheckExempt}, InviteCheck exempt: {group.InviteCheckExempt}", getChannelsAsMentions(group.Channels));
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

        /// <summary>
        /// Creates the embeds containing the channels from the given group and posts them towards the given channel
        /// </summary>
        /// <param name="channelToRespondIn">The <see cref="Discord.WebSocket.ISocketMessageChannel"> object where the embeds should be posted towards</param>
        /// <param name="type">The of the channel groups to display</param>
        /// <returns>Task</returns>
        public async Task ListChannelGroups(ISocketMessageChannel channelToRespondIn, string type)
        {
            var embedsToPost = GetChannelListEmbed(type);
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
                    if(!groupMember.Group.Disabled && groupMember.Group.ChannelGroupType == ChannelGroupType.CHECKS)
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
                var existingGroup = db.ChannelGroups.AsQueryable().Where(grp => grp.Name == name).FirstOrDefault();
                if(existingGroup != null)
                {
                  var existingChannelEntry = db.ChannelGroupMembers.AsQueryable().Where(mem => mem.ChannelGroupId == existingGroup.Id).FirstOrDefault();
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

        /// <summary>
        /// Changes the type of the channel group identified by the channel group name to the given channel group type (if its a valid one)
        /// </summary>
        /// <param name="groupName">The name of the group to change the type for</param>
        /// <param name="newType">The type the group should be changed to (needs to be a valid group type)</param>
        /// <exception cref="OnePlusBot.Base.Errors.ConfigurationException">In case the type is not valid</exception>
        /// <exception cref="OnePlusBot.Base.Errors.NotFoundException">In case no channel group with that name is found</exception>
        public void ChangeChannelGroupTypeTo(string groupName, string newType)
        {
          if(!ChannelGroupType.TYPES.Where(n => n == newType).Any())
          {
            throw new ConfigurationException("Group type not valid");
          }
          using(var db = new Database())
          {
            var channelGroup = db.ChannelGroups.AsQueryable().Where(grp => grp.Name == groupName);
            if(!channelGroup.Any())
            {
              throw new NotFoundException("Channel group not found");
            }
            channelGroup.First().ChannelGroupType = newType;
            db.SaveChanges();
          }
        }

        /// <summary>
        /// Creates embed and posts the embeds towards the given channel containing information about all the channel groups of type COMMAND.
        /// In case there are channels configured for this group, they get printed as well.
        /// </summary>
        /// <param name="channelToRespondIn">The <see cref="Discord.WebSocket.ISocketMessageChannel"> object where the embeds should be posted towards</param>
        /// <returns>Task</returns>
        public async Task ListGroupsWithCommands(ISocketMessageChannel channelToRespondIn)
        {
          EmbedBuilder builder = new EmbedBuilder();
          builder.WithTitle("Command channel group overview");
          StringBuilder sb = new StringBuilder();
          using(var db = new Database()){
            var commandGroups = db.ChannelGroups.Include(grp => grp.Commands).ThenInclude(ch => ch.CommandReference);
            foreach(var grp in commandGroups)
            {
              var disabledPart = grp.Disabled ? "(Disabled)" : "";
              sb.Append($"{grp.Name} {grp.ChannelGroupType} {disabledPart}: ");
              if(grp.Commands == null || grp.Commands.Count() == 0){
                sb.Append("No commands.\n");
                continue;
              }
              foreach(var cmd in grp.Commands)
              {
                var commandDisabledPart = cmd.Disabled ? "(Disabled)" : "";
                sb.Append($"`{cmd.CommandReference.Name}` {commandDisabledPart}");
                if(cmd != grp.Commands.Last()){
                  sb.Append(", ");
                }
              }
              sb.Append("\n");
             
            }
          }
          builder.WithDescription(sb.ToString());
          await channelToRespondIn.SendMessageAsync(embed: builder.Build());
        }
    }
}