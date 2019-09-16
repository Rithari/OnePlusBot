using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;
using System.Data.Common;
using System;
using Discord;
using Discord.Commands;
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
        public Boolean ActionApplies(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            return reaction.Emote.Equals(Global.OnePlusEmote.STAR) && message.Author.Id != reaction.UserId;
        }
        public virtual async Task Execute(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction) 
        {
            this.TriggeredThreshold = false;
            this.RelationAdded = false;
            var guild = Global.Bot.GetGuild(Global.ServerID);
            
            var currentStarReaction = message.Reactions.Where(re => re.Key.Name == reaction.Emote.Name).DefaultIfEmpty().First();

            var starboardChannelId = Global.Channels["starboard"];
            var starboardChannel = guild.GetTextChannel(starboardChannelId);
            var existingPostList = Global.StarboardPosts.Where(msg => msg.MessageId == message.Id);

            var starCount = await this.GetTrueStarCount(message, currentStarReaction);
            if(existingPostList.Any())
            {
                this.RelationAdded = true;
                var existingPost = existingPostList.First();
                var starboardChannelPostId = existingPost.StarboardMessageId;
                this.StarboardPostId = starboardChannelPostId;
                existingPost.Starcount = (uint) starCount;
                using(var db = new Database())
                {
                    var post = db.StarboardMessages.Where(p => p.MessageId == message.Id).First();
                    post.Starcount = (uint) starCount;
                    db.SaveChanges();
                }
                var starboardChannelPost = await starboardChannel.GetMessageAsync(starboardChannelPostId) as IUserMessage;
                if(starCount >= (int) Global.StarboardStars && starboardChannelPost != null )
                {
                    await starboardChannelPost.ModifyAsync(msg => msg.Content = GetStarboardMessage(message, currentStarReaction, reaction, starCount));
                } 
                else if(starboardChannelPost != null)
                {
                    await starboardChannelPost.DeleteAsync();
                    Global.StarboardPosts.Remove(existingPost);
                    using(var db = new Database())
                    {
                        var relationPosts = db.StarboardPostRelations.Where(post => post.MessageId == message.Id);
                        db.StarboardPostRelations.RemoveRange(relationPosts);
                        db.SaveChanges();
                        var starboardMessage = db.StarboardMessages.Where(post => post.MessageId == message.Id).First();
                        db.StarboardMessages.Remove(starboardMessage);
                        db.SaveChanges();
                    }
                    this.TriggeredThreshold = true;
                }
            }
            else
            {   
                if(starCount >= (int) Global.StarboardStars)
                {
                    var starboardMessage = await starboardChannel.SendMessageAsync(GetStarboardMessage(message, currentStarReaction, reaction, starCount), 
                    embed: GetStarboardEmbed(message, currentStarReaction));
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

        private Embed GetStarboardEmbed(IUserMessage message, KeyValuePair<IEmote, ReactionMetadata> reactionInfo)
        {
            var builder =  new EmbedBuilder()
                .WithColor(9896005)
                .WithAuthor(author => author
                    .WithIconUrl(message.Author.GetAvatarUrl())
                    .WithName(message.Author.Username)
                    )
                    
                .WithDescription(message.Content)
                .AddField(fb => fb
                    .WithName("Original")
                    .WithValue(OnePlusBot.Helpers.Extensions.GetMessageUrl(Global.ServerID, message.Channel.Id, message.Id, "jump"))
                    )
                .WithTimestamp(message.CreatedAt);
            if(message.Attachments.Count > 0)
            {
                builder = builder.WithImageUrl(message.Attachments.First().ProxyUrl);
            }

            return builder.Build();
        }

        private string GetStarboardMessage(IUserMessage message, KeyValuePair<IEmote, ReactionMetadata> reactionInfo, IReaction reaction, int starCount)
        {
            return $"{reaction.Emote.Name} {starCount} <#{message.Channel.Id}> ID: {message.Id}"; 
        }

        private async Task<int> GetTrueStarCount(IUserMessage message, KeyValuePair<IEmote, ReactionMetadata> starReaction)
        {
            var reactions = message.GetReactionUsersAsync(Global.OnePlusEmote.STAR, starReaction.Value.ReactionCount);
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
                    var reactions = message.GetReactionUsersAsync(Global.OnePlusEmote.STAR, currentStarReaction.Value.ReactionCount);
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
                var existing = db.StarboardPostRelations.Where(rel => rel.MessageId == message.Id && rel.UserId == message.Author.Id)
                .DefaultIfEmpty(null).First();
                if(existing != null)
                {
                    db.StarboardPostRelations.Remove(existing);
                }
            }
         }
    }
}