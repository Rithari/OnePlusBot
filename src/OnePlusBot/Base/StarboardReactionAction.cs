using OnePlusBot.Helpers;
using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;

namespace OnePlusBot.Base
{
    public class StarboardReactionAction : IReactionAction
    {

        protected ulong StarboardPostId;
        protected bool TriggeredThreshold = false;
        protected bool RelationAdded = false;
        public bool ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            return reaction.Emote.Equals(StoredEmote.GetEmote(Global.OnePlusEmote.STAR)) && message.Author.Id != reaction.UserId;
        }
        public virtual async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction) 
        {
            this.TriggeredThreshold = false;
            this.RelationAdded = false;
            var guild = Global.Bot.GetGuild(Global.ServerID);
            
            var currentStarReaction = message.Reactions.Where(re => re.Key.Name == reaction.Emote.Name).DefaultIfEmpty().First();

            var starboardChannel = guild.GetTextChannel(Global.PostTargets[PostTarget.STARBOARD]);
            var existingPostList = Global.StarboardPosts.Where(msg => msg.MessageId == message.Id);

            var starCount = await this.GetTrueStarCount(message, currentStarReaction);
            if(existingPostList.Any())
            {
                var existingPost = existingPostList.First();
                var starboardChannelPostId = existingPost.StarboardMessageId;
                this.StarboardPostId = starboardChannelPostId;
                existingPost.Starcount = (uint) starCount;
                using(var db = new Database())
                {
                    var post = db.StarboardMessages.AsQueryable().Where(p => p.MessageId == message.Id).First();
                    if(post.Ignored)
                    {
                        return;
                    }
                    post.Starcount = (uint) starCount;
                    db.SaveChanges();
                }
                this.RelationAdded = true;
                var starboardChannelPost = await starboardChannel.GetMessageAsync(starboardChannelPostId) as IUserMessage;
                if(starCount >= (int) Global.StarboardStars && starboardChannelPost != null)
                {
                    await starboardChannelPost.ModifyAsync(msg => msg.Content = GetStarboardMessage(message, currentStarReaction, reaction, starCount));
                } 
                else 
                {
                    if(starboardChannelPost != null) {
                        await starboardChannelPost.DeleteAsync();
                    }
                    if(!existingPost.Ignored)
                    {
                      Global.StarboardPosts.Remove(existingPost);
                      using(var db = new Database())
                      {
                          var relationPosts = db.StarboardPostRelations.AsQueryable().Where(post => post.MessageId == message.Id);
                          db.StarboardPostRelations.RemoveRange(relationPosts);
                          db.SaveChanges();
                          var starboardMessage = db.StarboardMessages.AsQueryable().Where(post => post.MessageId == message.Id).First();
                          db.StarboardMessages.Remove(starboardMessage);
                          db.SaveChanges();
                      }
                      this.TriggeredThreshold = true;
                    }
                }
            }
            else
            {   
                if(starCount >= (int) Global.StarboardStars)
                {
                    var starboardMessage = await starboardChannel.SendMessageAsync(GetStarboardMessage(message, currentStarReaction, reaction, starCount), 
                    embed: GetStarboardEmbed(message));
                    using (var db = new Database())
                    {
                        var starboardMessageDto = new StarboardMessage();
                        starboardMessageDto.MessageId = message.Id;
                        starboardMessageDto.StarboardMessageId = starboardMessage.Id;
                        starboardMessageDto.Starcount = (uint) starCount;
                        starboardMessageDto.AuthorId = message.Author.Id;
                        db.StarboardMessages.Add(starboardMessageDto);
                        Global.StarboardPosts.Add(starboardMessageDto);
                        db.SaveChanges();
                    }
                    this.TriggeredThreshold = true;
                    this.StarboardPostId = starboardMessage.Id;
                    this.RelationAdded = true;
                } 
                else
                {
                    this.RelationAdded = false;
                }
            }
        }

        /// <summary>
        /// Sends the notitificatoin about a reached threshold to the starboard log post target.
        /// The notification containsn the user causing the change and a link to the starred message.
        /// </summary>
        /// <param name="text">The text of the notification</param>
        /// <param name="user">The <see cref="Discord.IUser"> adding/removing the reaction</param>
        /// <param name="message">The starred <see cref="Discord.IUserMessage"> for which the reaction was added/removed</param>
        /// <returns></returns>
        protected async Task SendNotification(string text, IUser user, IUserMessage message)
        {
          var notificationBuilder = new EmbedBuilder();
          var userFormatted = Extensions.FormatUserNameDetailed(user);
          var messageLink =  $"[Here]({message.GetJumpUrl()})";
          notificationBuilder.WithDescription(text);
          notificationBuilder.AddField("User", userFormatted);
          notificationBuilder.AddField("Link", messageLink);
          notificationBuilder.WithThumbnailUrl(user.GetAvatarUrl());
          var guild = Global.Bot.GetGuild(Global.ServerID);
          var target = guild.GetTextChannel(Global.PostTargets[PostTarget.STARBOARD_LOG]);
          await target.SendMessageAsync(embed: notificationBuilder.Build());
        }

        /// <summary>
        /// Creates an embed containing the starred message and a field containing al link to the starred message
        /// </summary>
        /// <param name="message">The <see cref="Discord.IUserMessage"> object containing the message which is getting starred.</param>
        /// <returns>The rendered embed containing the desired info</returns>
        private Embed GetStarboardEmbed(IUserMessage message)
        {
            var builder = Extensions.GetMessageAsEmbed(message);
            builder.AddField("Original", OnePlusBot.Helpers.Extensions.FormatLinkWithDisplay("Jump!", message.GetJumpUrl()));
            return builder.Build();
        }

        /// <summary>
        /// Builds the message of a starboard message containing the messageID, the channel of th epost, the star count and an emote.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="reactionInfo"></param>
        /// <param name="reaction"></param>
        /// <param name="starCount"></param>
        /// <returns></returns>
        private string GetStarboardMessage(IUserMessage message, KeyValuePair<IEmote, ReactionMetadata> reactionInfo, IReaction reaction, int starCount)
        {
            var emote = StoredEmote.GetEmote(Global.OnePlusEmote.STAR);
            if(starCount >= (int) Global.Level2Stars)
            {
                emote = StoredEmote.GetEmote(Global.OnePlusEmote.LVL_2_STAR);
            }
            if(starCount >= (int) Global.Level3Stars)
            {
                emote = StoredEmote.GetEmote(Global.OnePlusEmote.LVL_3_STAR);
            }
            if(starCount >= (int) Global.Level4Stars)
            {
                emote = StoredEmote.GetEmote(Global.OnePlusEmote.LVL_4_STAR);
            }
            return $"{emote} {starCount} <#{message.Channel.Id}> ID: {message.Id}"; 
        }

        /// <summary>
        /// Returns the actual star count on a message (the count of star reactions without the author)
        /// </summary>
        /// <param name="message">The <see cref="Discord.IUserMessage"> object to get the star count for</param>
        /// <param name="starReaction">The pair of the reaction with the reaction metadata of the current reaction which was just added</param>
        /// <returns>The amount of reactions besides the original author of the message</returns>
        private async Task<int> GetTrueStarCount(IUserMessage message, KeyValuePair<IEmote, ReactionMetadata> starReaction)
        {
            var reactions = message.GetReactionUsersAsync(StoredEmote.GetEmote(Global.OnePlusEmote.STAR), starReaction.Value.ReactionCount);
            var validReactions = 0;
            await reactions.ForEachAsync(collection =>
            {
                foreach(var user in collection)
                {
                    if(user.Id != message.Author.Id)
                    {
                        validReactions += 1;
                    }
                }
            });
            return validReactions;
        }
    }

    public class StarboardAddedReactionAction : StarboardReactionAction 
    {
         public override async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
         {
           
            await base.Execute(message, channel, reaction);
            using (var db = new Database())
            {  
                if(this.TriggeredThreshold)
                {
                    var currentStarReaction = message.Reactions.Where(re => re.Key.Name == reaction.Emote.Name).DefaultIfEmpty().First();
                    var reactions = message.GetReactionUsersAsync(StoredEmote.GetEmote(Global.OnePlusEmote.STAR), currentStarReaction.Value.ReactionCount);
                    await reactions.ForEachAsync(collection =>
                    {
                        foreach(var user in collection)
                        {
                            var starerRelation = new StarboardPostRelation();
                            starerRelation.UserId = user.Id;
                            starerRelation.MessageId = message.Id;
                            db.StarboardPostRelations.Add(starerRelation);
                        }
                    });
                    
                    if(reaction.User.IsSpecified)
                    {
                      var notificationMessage = "User caused a starboard post to reach the threshold.";
                      await base.SendNotification(notificationMessage, reaction.User.Value, message);
                    }
                }
                else if(this.RelationAdded)
                {
                    var starerRelation = new StarboardPostRelation();
                    starerRelation.UserId = reaction.UserId;
                    starerRelation.MessageId = message.Id;
                    db.StarboardPostRelations.Add(starerRelation);
                }
                db.SaveChanges();
            }
         }
    }

    public class StarboardRemovedReactionAction : StarboardReactionAction
    {
         public override async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
         {
            await base.Execute(message, channel, reaction);
            using (var db = new Database())
            {
                var existing = db.StarboardPostRelations.AsQueryable().Where(rel => rel.MessageId == message.Id && rel.UserId == reaction.UserId)
                .DefaultIfEmpty(null).First();
                if(existing != null)
                {
                    db.StarboardPostRelations.Remove(existing);
                }
                db.SaveChanges();
                if(this.TriggeredThreshold && reaction.User.IsSpecified)
                {
                  var notificationMessage = "User caused a starboard post to fall under the threshold.";
                  await base.SendNotification(notificationMessage, reaction.User.Value, message);
                }
            }
         }
    }
}
