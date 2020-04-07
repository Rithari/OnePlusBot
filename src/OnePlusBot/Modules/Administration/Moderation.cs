using System.Text;
using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Collections.ObjectModel;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Helpers;
using OnePlusBot.Data.Models;
using System.Runtime.InteropServices;
using System.Linq;
using Discord.WebSocket;
using Discord.Net;
using System.Collections.Generic;

namespace OnePlusBot.Modules.Administration
{
    public partial class Administration : ModuleBase<SocketCommandContext>
    {
      /// <summary>
      /// Bans the user by the given id with an optional reason.
      /// </summary>
      /// <param name="name">Id of the user to be banned</param>
      /// <param name="reason">Reason for the ban (optional)</param>
      /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
      [
          Command("banid", RunMode = RunMode.Async),
          Summary("Bans specified user."),
          RequireRole("staff"),
          CommandDisabledCheck
      ]
      public async Task<RuntimeResult> OBanAsync(ulong name, [Remainder] string reason = null)
      {
        var banLogChannel = Context.Guild.GetTextChannel(Global.PostTargets[PostTarget.BAN_LOG]);
        await Context.Guild.AddBanAsync(name, 0, reason);

        MuteTimerManager.UnMuteUserCompletely(name);


        if(reason == null)
        {
            reason = "No reason provided.";
        }

        var banMessage = new EmbedBuilder()
        .WithColor(9896005)
        .WithTitle("⛔️ Banned User")
        .AddField("UserId", name, true)
        .AddField("By", Extensions.FormatUserNameDetailed(Context.User), true)
        .AddField("Reason", reason)
        .AddField("Link", Extensions.FormatLinkWithDisplay("Jump!", Context.Message.GetJumpUrl()));

        await banLogChannel.SendMessageAsync(embed: banMessage.Build());

        return CustomResult.FromSuccess();
      }
  

      /// <summary>
      /// Bans the given user with the given reason (default text if none provided). Also sends a direct message to the user containing the reason and the mail used for appeals
      /// The ban will also be logged in the ban log post target
      /// </summary>
      /// <param name="user"><see ref="Discord.IGuildUser"> object of the user to be banned</param>
      /// <param name="reason">Optional reason for the ban</param>
      /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
      [
          Command("ban", RunMode = RunMode.Async),
          Summary("Bans specified user."),
          RequireRole("staff"),
          RequireBotPermission(GuildPermission.BanMembers),
          CommandDisabledCheck
      ]
      public async Task<RuntimeResult> BanAsync(IGuildUser user, [Remainder] string reason = null)
      {
        if (user.IsBot)
        {
            return CustomResult.FromError("You can't ban bots.");
        }


        if (user.GuildPermissions.PrioritySpeaker)
        {
            return CustomResult.FromError("You can't ban staff.");
        }
            
        
        if(reason == null)
        {
            reason = "No reason provided.";
        }
        try
        {
            try 
            {
                const string banDMMessage = "You were banned on r/OnePlus for the following reason: {0}\n" +
                                      "If you believe this to be a mistake, please send an appeal e-mail with all the details to oneplus.appeals@pm.me";
                await user.SendMessageAsync(string.Format(banDMMessage, reason));
            } catch (HttpException){
              await Context.Channel.SendMessageAsync("Seems like user disabled DMs, cannot send message about the ban.");
            }
            
            await Context.Guild.AddBanAsync(user, 0, reason);

            MuteTimerManager.UnMuteUserCompletely(user.Id);

            var banLogChannel = Context.Guild.GetTextChannel(Global.PostTargets[PostTarget.BAN_LOG]);
            var banlog = new EmbedBuilder()
            .WithColor(9896005)
            .WithTitle("⛔️ Banned User")
            .AddField("User", Extensions.FormatUserNameDetailed(user), true)
            .AddField("By", Extensions.FormatUserNameDetailed(Context.User), true)
            .AddField("Reason", reason)
            .AddField("Link", Extensions.FormatLinkWithDisplay("Jump!", Context.Message.GetJumpUrl()));

            await banLogChannel.SendMessageAsync(embed: banlog.Build());

            return CustomResult.FromSuccess();

        }
        catch (Exception ex)
        {   
            //  may not be needed
            // await Context.Guild.AddBanAsync(user, 0, reason);
            return CustomResult.FromError(ex.Message);
        }
      }

      [
        Command("kick", RunMode = RunMode.Async),
        Summary("Kicks specified user."),
        RequireRole("staff"),
        RequireBotPermission(GuildPermission.KickMembers),
        CommandDisabledCheck
      ]
      public async Task<RuntimeResult> KickAsync(IGuildUser user, [Remainder] string reason = null)
      {
        if (user.IsBot)
            return CustomResult.FromError("You can't kick bots.");


        if (user.GuildPermissions.PrioritySpeaker)
            return CustomResult.FromError("You can't kick staff.");

        await user.KickAsync(reason);
        MuteTimerManager.UnMuteUserCompletely(user.Id);
        return CustomResult.FromSuccess();
      }

      /// <summary>
      /// Mutes the user for a certain timeperiod (effectively gives the user the two (!) configured roles denying the user the ability to send messages)
      /// The user receives a message with the reason and the date at which the mute is lifted automatically. The mute is logged in the mutelog posttarget.
      /// </summary>
      /// <param name="user">The <see cref="Discord.IGuildUser"> user to mute</param>
      /// <param name="arguments">Arguments for muting, including: the duration and the reason</param>
      /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
      [
          Command("mute", RunMode=RunMode.Async),
          Summary("Mutes a specified user for a set amount of time"),
          RequireRole("staff"),
          RequireBotPermission(GuildPermission.ManageRoles),
          CommandDisabledCheck
      ]
      public async Task<RuntimeResult> MuteUser(IGuildUser user,params string[] arguments)
      {
          if (user.IsBot)
              return CustomResult.FromError("You can't mute bots.");

          if (user.GuildPermissions.PrioritySpeaker)
              return CustomResult.FromError("You can't mute staff.");
          
          if(arguments.Length < 1)
              return CustomResult.FromError("The correct usage is `;mute <duration> <reason>`");

          string durationStr = arguments[0];
          TimeSpan span = Extensions.GetTimeSpanFromString(durationStr);
          var now = DateTime.Now;
          DateTime targetTime = now.Add(span);

          string reason;
          if(arguments.Length > 1)
          {
              string[] reasons = new string[arguments.Length -1];
              Array.Copy(arguments, 1, reasons, 0, arguments.Length - 1);
              reason = string.Join(" ", reasons);
          } 
          else 
          {
              return CustomResult.FromError("You need to provide a reason.");
          }

          var author = Context.Message.Author;

          var guild = Context.Guild;
          var mutedRoleName = "textmuted";
          if(!Global.Roles.ContainsKey(mutedRoleName))
          {
              return CustomResult.FromError("Text mute role has not been configured. Check your DB entry!");
          }

          mutedRoleName = "voicemuted";
          if(!Global.Roles.ContainsKey(mutedRoleName))
          {
              return CustomResult.FromError("Voice mute role has not been configured. Check your DB entry!");
          }

          await Extensions.MuteUser(user);
          await user.ModifyAsync(x => x.Channel = null);

          try
          {
            const string muteMessage = "You were muted on r/OnePlus for the following reason: {0} until {1}.";
            await user.SendMessageAsync(string.Format(muteMessage, reason, Extensions.FormatDateTime(targetTime)));
          } 
          catch(HttpException)
          {
            await Context.Channel.SendMessageAsync("Seems like user disabled DMs, cannot send message about the mute.");
          }    

          var muteData = new Mute
          {
              MuteDate = now,
              UnmuteDate = targetTime,
              MutedUser = user.Username + '#' + user.Discriminator,
              MutedUserID = user.Id,
              MutedByID = author.Id,
              MutedBy = author.Username + '#' + author.Discriminator,
              Reason = reason,
              MuteEnded = false
          };

          using (var db = new Database())
          {
              db.Mutes.Add(muteData);
              db.SaveChanges();
          }        

          var builder = new EmbedBuilder();
          builder.Title = "A user has been muted!";
          builder.Color = Color.Red;
          
          builder.Timestamp = Context.Message.Timestamp;
          
          builder.ThumbnailUrl = user.GetAvatarUrl();
          
          const string discordUrl = "https://discordapp.com/channels/{0}/{1}/{2}";
          builder.AddField("Muted User", Extensions.FormatUserNameDetailed(user))
                  .AddField("Muted by", Extensions.FormatUserNameDetailed(author))
                  .AddField("Location of the mute",
                      $"[#{Context.Message.Channel.Name}]({string.Format(discordUrl, Context.Guild.Id, Context.Channel.Id, Context.Message.Id)})")
                  .AddField("Reason", reason ?? "No reason was provided.")
                  .AddField("Mute duration", Extensions.FormatTimeSpan(span))
                  .AddField("Muted until", $"{ Extensions.FormatDateTime(targetTime)}")
                  .AddField("Mute id", muteData.ID);
              
          await guild.GetTextChannel(Global.PostTargets[PostTarget.MUTE_LOG]).SendMessageAsync(embed: builder.Build());
          // in case the mute is shorter than the timer defined in Mutetimer.cs, we better just start the unmuting process directly
          if(targetTime <= DateTime.Now.AddMinutes(60))
          {
              var difference = targetTime - DateTime.Now;
              MuteTimerManager.UnmuteUserIn(user.Id, difference, muteData.ID);
              using (var db = new Database())
              {
                  db.Mutes.Where(m => m.ID == muteData.ID).First().UnmuteScheduled = true;
                  db.SaveChanges();
              }    
          }
          
          return CustomResult.FromSuccess();
      }

      [
          Command("unmute", RunMode=RunMode.Async),
          Summary("Unmutes a specified user"),
          RequireRole("staff"),
          RequireBotPermission(GuildPermission.ManageRoles),
          CommandDisabledCheck
      ]
      public async Task<RuntimeResult> UnMuteUser(IGuildUser user)
      {
          return await MuteTimerManager.UnMuteUser(user.Id, ulong.MaxValue);
      }

        /// <summary>
      /// Changes the nickname of a user on the server (newNickname is optional, if not provided, will reset the nickname)
      /// </summary>
      /// <param name="user">The <see cref="Discord.IGuildUser"> object to change the nickname for</param>
      /// <param name="newNickname">The new nickname, optional, if not provided, it will reset the nickname</param>
      /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
      [
          Command("setNickname"),
          Summary("Changes the nickname of a given user, resets if empty"),
          RequireRole("staff"),
          CommandDisabledCheck
      ]
      public async Task<RuntimeResult> SetNicknameTo(IGuildUser user, [Optional]string newNickname)
      {
        await user.ModifyAsync((user) => user.Nickname = newNickname);
        return CustomResult.FromSuccess();
      }

      /// <summary>
      /// Changes the slow mode of the current channel to the given interval, 'off' for disabling slowmode
      /// </summary>
      /// <param name="slowModeConfig">Time format with the format 'd', 's', 'm', 'h', 'w'</param>
      /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
      [
        Command("slowmode"),
        Summary("Changes the slowmode configuration of the current channel, 'off' to turn off slowmode"),
        RequireRole("staff"),
        CommandDisabledCheck
      ]
      public async Task<RuntimeResult> SetSlowModeTo(string slowModeConfig)
      {
        var channelObj = Context.Guild.GetTextChannel(Context.Channel.Id);
        if(slowModeConfig == "off")
        {
          await channelObj.ModifyAsync(pro => pro.SlowModeInterval = 0);
        }
        else
        {
          var span = Extensions.GetTimeSpanFromString(slowModeConfig);
          if(span > TimeSpan.FromHours(6))
          {
            return CustomResult.FromError("Only values between 1 second and 6 hours allowed.");
          }
          await channelObj.ModifyAsync(pro => pro.SlowModeInterval = (int) span.TotalSeconds);
        }
        return CustomResult.FromSuccess();
      }

      [
        Command("purge", RunMode = RunMode.Async),
        Summary("Deletes specified amount of messages."),
        RequireRole("staff"),
        RequireBotPermission(GuildPermission.ManageMessages),
        CommandDisabledCheck
      ]
      public async Task<RuntimeResult> PurgeAsync([Remainder] double delmsg)
      {
        if (delmsg > 100 || delmsg <= 0)
            return CustomResult.FromError("Use a number between 1-100");

        int delmsgInt = (int)delmsg;
        ulong oldmessage = Context.Message.Id;

        var isInModmailContext = Global.ModMailThreads.Exists(ch => ch.ChannelId == Context.Channel.Id);

        // Download all messages that the user asks for to delete
        var messages = await Context.Channel.GetMessagesAsync(oldmessage, Direction.Before, delmsgInt).FlattenAsync();

        var saveMessagesToDelete = new List<IMessage>();
        if(isInModmailContext){
            var manager = new ModMailManager();
            foreach(var message in messages){
                var messageCanBeDeleted = await manager.DeleteMessageInThread(Context.Channel.Id, message.Id, false);
                if(messageCanBeDeleted){
                    saveMessagesToDelete.Add(message);
                }
            }
        }
        else
        {
            saveMessagesToDelete.AddRange(messages);
        }
        await ((ITextChannel) Context.Channel).DeleteMessagesAsync(saveMessagesToDelete);

        await Context.Message.DeleteAsync();

        return CustomResult.FromIgnored(); 
      }

      [
        Command("profanities"),
        Summary("Shows the actual and false profanities of a user"),
        RequireRole("staff"),
        CommandDisabledCheck
      ]
      public async Task<RuntimeResult> ShowProfanities(IGuildUser user)
      {
        using(var db = new Database()){
          var allProfanities = db.Profanities.Where(pro => pro.UserId == user.Id);
          var actualProfanities = allProfanities.Where(pro => pro.Valid == true).Count();
          var falseProfanities = allProfanities.Where(pro =>pro.Valid == false).Count();
          var builder = new EmbedBuilder();
          builder.AddField(f => {
              f.Name = "Actual";
              f.Value = actualProfanities;
              f.IsInline = true;
          });

            builder.AddField(f => {
              f.Name = "False positives";
              f.Value = falseProfanities;
              f.IsInline = true;
          });

          builder.WithAuthor(new EmbedAuthorBuilder().WithIconUrl(user.GetAvatarUrl()).WithName(Extensions.FormatUserName(user)));
          await Context.Channel.SendMessageAsync(embed: builder.Build());
          return CustomResult.FromSuccess();
        }
      }

      /// <summary>
      /// Warns the passed user with the given reason. The warning is logged in the database and will get decayed. The user receives a DM
      /// containing the reason of the warning. The warning will also be logged in the warn log post target
      /// </summary>
      /// <param name="user">The <see cref="Discord.IGuildUser"> user to be warned</param>
      /// <param name="reason">The reason for the warn</param>
      /// <returns><see ref="Discord.RuntimeResult"> containing the result of the command</returns>
      [
          Command("warn"),
          Summary("Warn someone."),
          RequireRole("staff"),
          RequireBotPermission(GuildPermission.Administrator),
          CommandDisabledCheck
      ]
      public async Task<RuntimeResult> WarnAsync(IGuildUser user, [Optional] [Remainder] string reason)
      {
        var warningsChannel = Context.Guild.GetTextChannel(Global.PostTargets[PostTarget.WARN_LOG]);

        var monitor = Context.Message.Author;

        if(reason == null)
        {
          reason = "No reason provided.";
        }

        var entry = new WarnEntry
        {
          WarnedUserID = user.Id,
          WarnedByID = monitor.Id,
          Reason = reason,
          Date = Context.Message.Timestamp.DateTime,
        };

        using (var db = new Database())
        {
          db.Warnings.Add(entry);
          db.SaveChanges();
        }

        try
        {
          const string muteMessage = "You were warned on r/OnePlus for the following reason: {0}.";
          await user.SendMessageAsync(string.Format(muteMessage, reason));
        }
        catch(HttpException)
        {
          await Context.Channel.SendMessageAsync("Seems like user disabled DMs, cannot send message about the warn.");
        }

        var builder = new EmbedBuilder();
        builder.Title = "...a new warning has emerged from outer space!";
        builder.Color = new Color(0x3E518);
        
        builder.Timestamp = Context.Message.Timestamp;
        
        builder.WithFooter(footer =>
        {
            footer
              .WithText("Case #" + entry.ID)
              .WithIconUrl("https://a.kyot.me/0WPy.png");
        });
        
        builder.ThumbnailUrl = user.GetAvatarUrl();
        
        builder.WithAuthor(author =>
        {
            author
              .WithName("Woah...")
              .WithIconUrl("https://a.kyot.me/cno0.png");
        });

        const string discordUrl = "https://discordapp.com/channels/{0}/{1}/{2}";
        builder.AddField("Warned User", Extensions.FormatMentionDetailed(user))
            .AddField("Warned by", Extensions.FormatMentionDetailed(monitor))
            .AddField(
                "Location of the incident",
                $"[#{Context.Message.Channel.Name}]({string.Format(discordUrl, Context.Guild.Id, Context.Channel.Id, Context.Message.Id)})")
            .AddField("Reason", reason ?? "No reason was provided.");
            
        var embed = builder.Build();
        await warningsChannel.SendMessageAsync(null,embed: embed).ConfigureAwait(false);

        return CustomResult.FromSuccess();
      }

      [
          Command("clearwarn"),
          Summary("Clear warnings."),
          RequireRole("staff"),
          RequireBotPermission(GuildPermission.Administrator),
          CommandDisabledCheck
      ]
      public async Task<RuntimeResult> ClearwarnAsync(uint index)
      {
        var monitor = Context.Message.Author;

        using (var db = new Database())
        {
            IQueryable<WarnEntry> warnings = db.Warnings;
            var selection = warnings.First(x => x.ID == index);
            db.Warnings.Remove(selection);
            await db.SaveChangesAsync();
        }

        return CustomResult.FromSuccess();
      }

      private const int TakeAmount = 6;
      
      private IUserMessage _message;
      private IGuildUser _user;
      private int _total;
      private int _index;
      
      private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> user, ISocketMessageChannel channel, SocketReaction reaction)
      {
        if (reaction.User.Value.IsBot)
            return;
        if (reaction.MessageId != _message.Id)
            return;
        if (reaction.UserId != Global.CommandExecutorId)
            return;

        switch (reaction.Emote.Name)
        {
          case "⬅":
              await reaction.Message.Value.DeleteAsync();
              _index--;
              break;
          
          case "➡":
              await reaction.Message.Value.DeleteAsync();
              _index++;
              break;
          case "🗑":
              await reaction.Message.Value.DeleteAsync();
              return;
        }
        
        var skip = TakeAmount * _index;
        using (var db = new Database())
        {
          IQueryable<WarnEntry> warnings = db.Warnings;
          if (_user != null)
              warnings = warnings.Where(x => x.WarnedUserID == _user.Id);
          warnings = warnings.Skip(skip).Take(TakeAmount);
          _message = await CreateWarnList(warnings.ToArray());
        }
      }

      /// <summary>
      /// Creates the and posts the message containing the given warnings
      /// </summary>
      /// <param name="warns">The warnings to display</param>
      /// <returns>The posted message in the current thread</returns>
      private async Task<IUserMessage> CreateWarnList(IEnumerable<WarnEntry> warns)
      {
        var embed = new EmbedBuilder();
        embed.WithColor(9896005);
        
        if (_user != null)
            embed.Title = $"Listing warnings of user {_user.Username}.";
        else
            embed.Title = $"There are {_total} warnings in total.";
        
        var counter = _index * TakeAmount;
        foreach (var warning in warns)
        {
            ++counter;
            var warnedBy = Context.Guild.GetUser(warning.WarnedByID);

            var decayed = warning.Decayed ? "" : "**Active**";
            if (_user != null)
            {
                var warnedByUserSafe = Extensions.FormatMentionDetailed(warnedBy);
                embed.AddField(new EmbedFieldBuilder()
                    .WithName("Warning #" + counter + " (" + warning.ID + ")")
                    .WithValue($"**Reason**: {warning.Reason}\n" +
                                $"**Warned by**: {warnedByUserSafe}\n" + 
                                $"*{Extensions.FormatDateTime(warning.Date)}* \n" +
                                $"{decayed} \n"
                                ));
            }
            else
            {
                var warned = Context.Guild.GetUser(warning.WarnedUserID);
                var warnedSafe = Extensions.FormatMentionDetailed(warned);
                var warnedBySafe = Extensions.FormatMentionDetailed(warnedBy);
                embed.AddField(new EmbedFieldBuilder()
                    .WithName($"Case #{warning.ID} - {Extensions.FormatDateTime(warning.Date)}")
                    .WithValue($"**Warned user**: {warnedSafe}\n" +
                                $"**Reason**: {warning.Reason} \n" +
                                $"**Warned by**: {warnedBySafe} \n" +
                                $"{decayed}"
                                ))
                    .WithFooter("*For more detailed info, consult the individual lists.*");
            }
          }

          var msg = await Context.Channel.EmbedAsync(embed);

          if (_index > 0)
              await msg.AddReactionAsync(new Emoji("⬅"));
          if (_index * TakeAmount + TakeAmount < _total)
              await msg.AddReactionAsync(new Emoji("➡"));
          await msg.AddReactionAsync(new Emoji("🗑"));

          return msg;
      }

      [
        Command("warnings"),
        Summary("Gets all warnings of given user"),
        CommandDisabledCheck
      ]
      public async Task GetWarnings([Optional] IGuildUser user)
      {
        var requestee = Context.User as SocketGuildUser;
        _user = user;
        Global.CommandExecutorId = Context.User.Id;
        
        using (var db = new Database())
        {
          IQueryable<WarnEntry> warnings = db.Warnings;
          IQueryable<WarnEntry> individualWarnings;
          
          if (user != null) {
            warnings = warnings.Where(x => x.WarnedUserID == user.Id);
          }

          _total = warnings.Count();

          if (!requestee.Roles.Any(x => x.Name == "Staff"))
          {
            individualWarnings = db.Warnings.Where(x => x.WarnedUserID == requestee.Id && !x.Decayed);
            var totalWarnings = db.Warnings.Where(x => x.WarnedUserID == requestee.Id);
            var builder = new EmbedBuilder();
            builder.WithAuthor(new EmbedAuthorBuilder().WithIconUrl(requestee.GetAvatarUrl()).WithName(requestee.Username + '#' + requestee.Discriminator));
            builder.WithDescription($"{requestee.Username + '#' + requestee.Discriminator} has {individualWarnings.Count()} active out of {totalWarnings.Count()} total warnings.");
            await ReplyAsync(embed: builder.Build());

            return;
          }

          if (_total == 0)
          {
            await ReplyAsync("The specified user has no warnings.");
            return;
          }
          
          warnings = warnings.Take(TakeAmount);

          _message = await CreateWarnList(warnings.ToArray());
          
          Context.Client.ReactionAdded += OnReactionAdded;
          _ = Task.Run(async () =>
          {
              await Task.Delay(120_000);
              Context.Client.ReactionAdded -= OnReactionAdded;
              await _message.DeleteAsync();
          });
        }
      }

      /// <summary>
      /// Creates and stores a usernote objet in the database
      /// </summary>
      /// <param name="user">The <see cref="Discord.IGuildUser"> user to create the note for</param>
      /// <param name="text">The text of the note</param>
      /// <returns></returns>
      [
        Command("usernote"),
        Summary("Adds a usernote to a user"),
        RequireRole("staff"),
        CommandDisabledCheck
      ]
      public async Task<RuntimeResult> AddUserNote(IGuildUser user, String text)
      {
        using(var db = new Database())
        {
          var note = new UserNote()
          {
            UserId = user.Id,
            NoteText = text,
            CreatedDate = DateTime.Now
          };
          db.UserNotes.Add(note);
          db.SaveChanges();
        }
        return CustomResult.FromSuccess();
      }

      /// <summary>
      /// Deletes the note by the given id
      /// </summary>
      /// <param name="id">The globally unique id of the note to be deleted. Will not get re-used.</param>
      /// <returns></returns>
      [
        Command("deleteNote"),
        Summary("Delete a usernote"),
        RequireRole("staff"),
        CommandDisabledCheck
      ]
      public async Task<RuntimeResult> RemoveUserNote(ulong id)
      {
        using(var db = new Database())
        {
          var note = db.UserNotes.Where(u => u.ID == id);
          if(note.Any())
          {
            db.UserNotes.Remove(note.First());
          }
          else
          {
            throw new Exception($"Note with id {id} not found.");
          }
          db.SaveChanges();
        }
        return CustomResult.FromSuccess();
      }

      /// <summary>
      /// Lists all usernotes of the given user as embeds. If the text of the embeds are not within the limit, multiple embeds will be posted.
      /// </summary>
      /// <param name="user">The <see cref="Discord.IGuildUser"> user to list the notes for</param>
      /// <returns></returns>
      [
        Command("usernotes"),
        Summary("Lists the currently stored usernotes of a user"),
        RequireRole("staff"),
        CommandDisabledCheck
      ]
      public async Task<RuntimeResult> RemoveUserNote(IGuildUser user)
      {
        StringBuilder currentBuilder = new StringBuilder("");
        var texts = new Collection<string>();
        using(var db = new Database())
        {
          var notes = db.UserNotes.Where(u => u.UserId == user.Id);
          if(notes.Any()) 
          {
            foreach(var note in notes) 
            {
              var noteText = $"Note *{note.ID}* on {Extensions.FormatDateTime(note.CreatedDate)}: {note.NoteText}" + Environment.NewLine;
              if((currentBuilder.ToString().Length + noteText.Length) > EmbedBuilder.MaxDescriptionLength)
              {
                texts.Add(currentBuilder.ToString());
                currentBuilder = new StringBuilder();
              }
              currentBuilder.Append(noteText);
            }
          }
          else
          {
            currentBuilder.Append("User does not have notes.");
          }
          texts.Add(currentBuilder.ToString());
          db.SaveChanges();
        }
        foreach(var text in texts)
        {
          var embedBuilder = new EmbedBuilder();
          embedBuilder.WithDescription(text);
          await ReplyAsync(embed: embedBuilder.Build());
          await Task.Delay(200);
        }
        return CustomResult.FromIgnored();
      }

      [
        Command("emoteStats"),
        Summary("Returns the tracked emote statistics. If no parameter is given, everything is returned."),
        RequireRole("staff")
      ]
      public async Task<RuntimeResult> ShowEmoteStats([Optional] string range)
      {
        TimeSpan fromWhere;
        DateTime startDate;
        if(range != null)
        {
          fromWhere = Extensions.GetTimeSpanFromString(range);
          startDate = DateTime.Now - fromWhere;
        }
        else
        {
          startDate = DateTime.MinValue;
        }

        var embedsToPost = new List<Embed>();
        AddEmbedsOfEmotes(embedsToPost, startDate, false, "Static emotes");
        AddEmbedsOfEmotes(embedsToPost, startDate, true, "Animated emotes");

        foreach(var embed in embedsToPost)
        {
          await Context.Channel.SendMessageAsync(embed: embed);
          await Task.Delay(500);
        }
        return CustomResult.FromSuccess();
      }

      /// <summary>
      /// Creates the embed necessary to display the emotes from the given start date and the given type.
      /// </summary>
      /// <param name="embedsToAddTo">List of embeds where the created embeds should be added to</param>
      /// <param name="startDate">Startdate from which the stats should be created from</param>
      /// <param name="animated">Whether or not the emotes considered should be animated</param>
      /// <param name="title">The title of the first embed</param>
       private void AddEmbedsOfEmotes(List<Embed> embedsToAddTo, DateTime startDate, Boolean animated, string title) 
      {
        using(var db = new Database())
        {
          var unfiltered = db.EmoteHeatMap
          .Where(e => e.UpdateDate.Date >= startDate.Date);
          IQueryable<EmoteHeatMap> filtered;
          if(animated)
          {
            filtered = unfiltered.Where(e => e.EmoteReference.Animated);
          }
          else
          {
            filtered = unfiltered.Where(e => !e.EmoteReference.Animated);
          }
          var emoteStats = filtered
          .GroupBy(e => e.Emote)
          .Select(g => new
                    {
                      g.Key,
                      SUM = g.Sum(s => s.UsageCount)
                    })
          .OrderByDescending(e => e.SUM);
          var currentStringBuilder = new StringBuilder();
          var currentEmbedBuilder = new EmbedBuilder();
          currentEmbedBuilder.WithTitle(title);
          var count = 0;
          foreach(var emoteStat in emoteStats)
          {
            var emoteUsedQuery = db.Emotes.Where(e => e.ID == emoteStat.Key);
            if(emoteUsedQuery.Any())
            {
              var emoteUsed = emoteUsedQuery.First();
              var currentEntry = emoteStat.SUM + "x" + emoteUsed.GetAsEmote().ToString() + "  ";
              if((currentStringBuilder.ToString() + currentEntry).Length > EmbedBuilder.MaxDescriptionLength)
              {
                count++;
                currentEmbedBuilder.WithDescription(currentStringBuilder.ToString());
                embedsToAddTo.Add(currentEmbedBuilder.Build());
                currentEmbedBuilder = new EmbedBuilder();
                currentStringBuilder = new StringBuilder();
                currentEmbedBuilder.WithFooter(new EmbedFooterBuilder().WithText($"Page {count}"));
              }
              else
              {
                currentStringBuilder.Append(currentEntry);
              }
            }
          }
          if(emoteStats.Count() == 0)
          {
            currentEmbedBuilder.WithDescription("No data.");
          }
          else
          {
            currentEmbedBuilder.WithDescription(currentStringBuilder.ToString());
          }

          embedsToAddTo.Add(currentEmbedBuilder.Build());
        }
      }

   
    }
}