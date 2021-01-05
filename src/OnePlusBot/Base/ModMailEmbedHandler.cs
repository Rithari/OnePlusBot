using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;
using OnePlusBot.Data.Models;
using System;
using OnePlusBot.Helpers;
using OnePlusBot.Base;

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

        /// <summary>
        /// Builds an embed containing the header of a newly (externally) created modmail thread (amount of threads and username)
        /// </summary>
        /// <param name="pastThreadCount">The amount of threads in the past</param>
        /// <param name="user">The <see cref="Discord.WebSocket.SocketUser"> for which this thread was created</param>
        /// <returns><see cref="Discord.Embed"> object containing the information</returns>
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
            builder.WithAuthor(ModMailEmbedHandler.GetUserAuthor(message.Author));
            builder.WithTitle(title);
            if(message.Attachments.Count > 0)
            {
                builder = builder.WithImageUrl(message.Attachments.First().ProxyUrl);
            }

            return builder.Build();
        }

        /// <summary>
        /// Returns the author for anonymous replies
        /// </summary>
        /// <returns><see cref="Discord.EmbedAuthorBuilder"> object representing the oneplus discord author</returns>
        public static EmbedAuthorBuilder GetOneplusAuthor()
        {
            return new EmbedAuthorBuilder().WithIconUrl("https://a.kyot.me/ab-y.png").WithName("r/OnePlus Discord");
        }

        public static EmbedAuthorBuilder GetUserAuthor(SocketUser user)
        {
            return new EmbedAuthorBuilder().WithIconUrl(user.GetAvatarUrl()).WithName(user.Username + "#" + user.Discriminator);
        }

        public static Embed GetInitialUserReply(SocketMessage message)
        {
            var builder = GetBaseEmbed();
            builder.WithAuthor(GetOneplusAuthor());
            builder.WithDescription("Thank you for your inquiry. A moderator will respond soon.");
            return builder.Build();
        }

        /// <summary>
        /// Creates the final embed sent to the user
        /// </summary>
        /// <returns><see cref="Discord.Embed"> object containing the final message to the user</returns>
        public static Embed GetClosingEmbed()
        {
            var embed = GetBaseEmbed();
            embed.WithAuthor(GetOneplusAuthor());
            embed.WithDescription($"Your inquiry has been closed. If you have any further questions please message this bot again.");
            return embed.Build();
        }

        /// <summary>
        /// Creates the embed sent to the user when an open thread was disabled for a certain time period.
        /// </summary>
        /// <param name="date">The <see cref="System.DateTime"> date at which modmail will be available for the user again</param>
        /// <returns>The <see cref="Discord.Embed"> object sent to the user containing the information about when the user can contact modmail again</returns>
        public static Embed GetDisablingEmbed(DateTime date)
        {
          var embed = GetBaseEmbed();
          embed.WithDescription($"Your inquiry has been closed. You will be able to contact modmail again on {Extensions.FormatDateTime(date)}.");
          return embed.Build();
        }

        /// <summary>
        /// Returns the string containing the given parameters formatted as the header used in the 'ModMailLog' embed which is used as the header
        /// </summary>
        /// <param name="thread">The <see cref"OnePlusBot.Data.Models.ModMailThread"/> object which is being closed.</param>
        /// <param name="messageCount">The amount of interactions between the user and the staff</param>
        /// <param name="user">The <see cref"Discord.WebSocket.SocketUser"/> object for who the thread was created</param>
        /// <param name="note">An optional note which is displayed as information in the closing header</param>
        /// <returns>The StringBuilder object containing the formatted information used as closing header.</returns>
        private static StringBuilder GetClosingHeader(ModMailThread thread, int messageCount, SocketUser user, string note){
            var descriptionBuilder = new StringBuilder();
            string defaultedNote = note ?? "No note";
            descriptionBuilder.Append($"A modmail thread has been closed with the note '**{ defaultedNote }**' \n ");
            descriptionBuilder.Append($"There were {messageCount} interactions with the user **{Extensions.FormatUserName(user)}** ({thread.UserId}). \n");
            descriptionBuilder.Append($"It has been opened on {Extensions.FormatDateTime(thread.CreateDate)}");
            descriptionBuilder.Append($" and lasted {Extensions.FormatTimeSpan(DateTime.Now - thread.CreateDate)}.");
            return descriptionBuilder;
        }

        public static Embed GetClosingSummaryEmbed(ModMailThread thread, int messageCount, SocketUser user, string note, Boolean silent)
        {
            var embed = GetBaseEmbed();
            var silentSuffix = silent ? " (silently)" : "";
            embed.WithTitle($"Modmail thread has been closed{silentSuffix}.");
            embed.WithDescription(GetClosingHeader(thread, messageCount, user, note).ToString());
            return embed.Build();
        }

        /// <summary>
        /// Builds the header for the log when the thread has been closed and disabled for a certain timeperiod
        /// </summary>
        /// <param name="thread">The <see cref="OnePlusBot.Data.Models.ModMailThread"> being closed</param>
        /// <param name="messageCount">The amount of messages between the moderators and the user</param>
        /// <param name="user">The <see cref="Discord.WebSocket.SocketUser"> object for which the thread was opened</param>
        /// <param name="note">The note (optional) which was used to close the thread</param>
        /// <param name="until">The <see cref="System.DateTime"> at which modmail will be available again for the user</param>
        /// <returns>The <see cref="Discord.Embed"> containing the information</returns>
        public static Embed GetMutingSummaryEmbed(ModMailThread thread, int messageCount, SocketUser user, string note, DateTime until)
        {
            StringBuilder description = GetClosingHeader(thread, messageCount, user, note);
            description.Append($"\n It has been disabled and will be available again on {Extensions.FormatDateTime(until)}.");
            var embed = GetBaseEmbed();
            embed.WithTitle("Modmail thread has been disabled for user.");
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

        /// <summary>
        /// Builds the embed used to notify moderators about the newly opened modmail thread (initiatied by the user)
        /// </summary>
        /// <param name="user"><see cref="Discord.WebSocket.SocketUser"> object containing information about the user iniating the modmail thread</param>
        /// <param name="thread"><see cref="OnePlusBot.Data.Models.ModMailThread"> object containing information about the modmail thread which has been opened</param>
        /// <returns><see cref="Discord.Embed"> object representing the notification sent to the moderators</returns>
        public static Embed GetModqueueNotificationEmbed(SocketUser user, ModMailThread thread)
        {
            var builder = GetBaseEmbed();
            builder.WithAuthor(GetUserAuthor(user));
            builder.WithTitle("A new modmail thread has been opened.");
            builder.WithDescription($"The thread concerns {Extensions.FormatUserNameDetailed(user)}.");
            builder.AddField("Link", Extensions.GetChannelUrl(Global.ServerID, thread.ChannelId, "Jump!"));
            return builder.Build();
        }

        /// <summary>
        /// Builds the embed used to notify the user using the contact command, that a modmail thread has already been opened for the targe tuser
        /// </summary>
        /// <param name="thread"><see cref="OnePlusBot.Data.Models.ModMailThread"> the already existing modmail thread</param>
        /// <returns><see cref="Discord.Embed"> object containing the information to inform the user about the already exisitng modmail thread</returns>
        public static Embed GetThreadAlreadyExistsEmbed(ModMailThread thread)
        {
            var builder = GetBaseEmbed();
            var bot = Global.Bot;
            var guild = bot.GetGuild(Global.ServerID);
            var user = guild.GetUser(thread.UserId);
            builder.WithDescription("Thread already exists.");
            builder.AddField("Link", Extensions.GetChannelUrl(guild.Id, thread.ChannelId, user.Username + user.Discriminator));
            return builder.Build();
        }

        /// <summary>
        /// Builds the embed used to inform the moderator about the user
        /// </summary>
        /// <param name="thread">The newly created modmail thread</param>
        /// <returns><see cref="Discord.Embed"> object containing information about the user</returns>
        public static Embed GetUserInfoHeader(ModMailThread thread)
        {
          var builder = GetBaseEmbed();
          var bot = Global.Bot;
          var guild = bot.GetGuild(Global.ServerID);
          var user = guild.GetUser(thread.UserId);
          string joinedPart = "";
          if(user.JoinedAt.HasValue)
          {
            joinedPart = $"Join date: {Extensions.FormatDateTime(user.JoinedAt.Value.DateTime)} ";
          }
          StringBuilder rolesPart = new StringBuilder();
          foreach(var role in user.Roles)
          {
            rolesPart.Append(role.Name);
            if(role != user.Roles.Last())
            {
              rolesPart.Append(", ");
            }
          }
          if(user.Roles.Count() == 0)
          {
            rolesPart.Append("no roles");
          }
          builder.WithAuthor(GetOneplusAuthor());
          builder.WithDescription($"User: {Extensions.FormatUserNameDetailed(user)}: {joinedPart} {user.Nickname} \n User has the following roles: {rolesPart.ToString()}");
          return builder.Build();
        }

        public static Embed GetThreadHasBeendCreatedEmbed(ModMailThread thread){
            var builder = GetBaseEmbed();
            var bot = Global.Bot;
            var guild = bot.GetGuild(Global.ServerID);
            var user = guild.GetUser(thread.UserId);
            builder.WithDescription("Thread has been created.");
            builder.AddField("Link", Extensions.GetChannelUrl(guild.Id, thread.ChannelId, user.Username + user.Discriminator));
            return builder.Build();
        }
    }
}
