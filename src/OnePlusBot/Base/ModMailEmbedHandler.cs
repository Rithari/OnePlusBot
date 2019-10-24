using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;
using OnePlusBot.Data.Models;
using System;
using OnePlusBot.Helpers;

namespace OnePlusBot 
{
    class ModMailEmbedHandler
    {
        public static EmbedBuilder GetBaseEmbed()
        {
            var builder = new EmbedBuilder();
            builder.WithCurrentTimestamp();
            return builder;
        }

        public static Embed GetUserInformation(int pastThreadCount, SocketUser user)
        {
            var descriptionText = new StringBuilder();
            descriptionText.Append($"There were {pastThreadCount} threads with {Extensions.FormatUserNameDetailed(user)} in the past.");
            var embed = GetBaseEmbed();
            embed.WithAuthor(GetOneplusAuthor());
            embed.WithDescription(descriptionText.ToString());
            return embed.Build();
        }


        public static Embed GetReplyEmbed(SocketMessage message, string title="User replied")
        {
            var builder = GetBaseEmbed();
            builder.WithDescription(message.Content);
            builder.WithAuthor(new EmbedAuthorBuilder().WithIconUrl(message.Author.GetAvatarUrl()).WithName(message.Author.Username));
            builder.WithTitle(title);
            if(message.Attachments.Count > 0)
            {
                builder = builder.WithImageUrl(message.Attachments.First().ProxyUrl);
            }

            return builder.Build();
        }

        public static EmbedAuthorBuilder GetOneplusAuthor(){
            return new EmbedAuthorBuilder().WithIconUrl("https://a.kyot.me/ab-y.png").WithName("r/Oneplus Discord");
        }

        public static EmbedAuthorBuilder GetUserAuthor(SocketUser user)
        {
            return new EmbedAuthorBuilder().WithIconUrl(user.GetAvatarUrl()).WithName(user.Username);
        }

        public static Embed GetInitialUserReply(SocketMessage message)
        {
            var builder = GetBaseEmbed();
            builder.WithAuthor(GetOneplusAuthor());
            builder.WithDescription("Thank you for your inquiry. A moderator will come back to you.");
            return builder.Build();
        }

        public static Embed GetClosingEmbed(string note){
            var embed = GetBaseEmbed();
            embed.WithAuthor(GetOneplusAuthor());
            embed.WithDescription($"Your inquiry has been closed with the note '{note}'. If you have any further questions please message this bot again.");
            return embed.Build();
        }

         public static Embed GetDisablingEmbed(string additionalInfo, DateTime date){
            var embed = GetBaseEmbed();
            embed.WithAuthor(GetOneplusAuthor());
            embed.WithDescription($"Your inquiry has been closed because: {additionalInfo}. You will be able to contact modmail again at {date:dd.MM.yyyy HH:mm} {TimeZoneInfo.Local}.");
            return embed.Build();
        }

        private static StringBuilder GetClosingHeader(ModMailThread thread, int messageCount, SocketUser user, string note){
            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.Append($"A modmail thread has been closed with the note '{note}' \n ");
            descriptionBuilder.Append($"There were {messageCount} interactions with the user {Extensions.FormatUserNameDetailed(user)}. \n");
            descriptionBuilder.Append($"It has been opened on {thread.CreateDate:dd.MM.yyyy HH:mm} {TimeZoneInfo.Local}");
            descriptionBuilder.Append($" and lasted {Extensions.FormatTimeSpan(DateTime.Now - thread.CreateDate)}.");
            return descriptionBuilder;
        }

        public static Embed GetClosingSummaryEmbed(ModMailThread thread, int messageCount, SocketUser user, string note)
        {
            var embed = GetBaseEmbed();
            embed.WithTitle("Modmail thread has been closed");
            embed.WithDescription(GetClosingHeader(thread, messageCount, user, note).ToString());
            return embed.Build();
        }

        public static Embed GetMutingSummaryEmbed(ModMailThread thread, int messageCount, SocketUser user, string note, DateTime until){
            StringBuilder description = GetClosingHeader(thread, messageCount, user, note);
            description.Append($"\n It has been disabled and will be available again at {until:dd.MM.yyyy HH:mm} {TimeZoneInfo.Local}.");
            var embed = GetBaseEmbed();
            embed.WithTitle("Modmail thread has been disabled for user");
            embed.WithDescription(description.ToString());
            return embed.Build();
        }

        public static Embed GetModeratorReplyEmbed(string message, string title, SocketMessage messageObj,  SocketUser user=null)
        {
            var author = user == null ? GetOneplusAuthor() : GetUserAuthor(user);
            var builder = GetBaseEmbed();
            builder.WithAuthor(author);
            if(messageObj.Attachments.Count > 0)
            {
                builder = builder.WithImageUrl(messageObj.Attachments.First().ProxyUrl);
            }
            builder.WithDescription(message);
            builder.WithTitle(title);
            return builder.Build();
        }
    }
}