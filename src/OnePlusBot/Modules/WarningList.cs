using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using OnePlusBot.Helpers;

namespace OnePlusBot.Modules
{
    public class WarningList : InteractiveBase<SocketCommandContext>
    {
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
                if (_user != null)
                {

                    embed.AddField(new EmbedFieldBuilder()
                        .WithName("Warning #" + counter + " (" + warning.ID + ")")
                        .WithValue($"**Reason**: {warning.Reason}\n" +
                                   $"**Warned by**: {warnedBy?.Mention ?? warning.WarnedBy}\n" + 
                                   $"*{warning.Date.ToString("dd/M/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture)}*"));
                }
                else
                {
                    var warned = Context.Guild.GetUser(warning.WarnedUserID);
                    embed.AddField(new EmbedFieldBuilder()
                        .WithName($"Warning #{counter} - {warning.Date.ToString("dd/M/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture)}")
                        .WithValue($"**Warned user**: {warned?.Mention ?? warning.WarnedUser}\n" +
                                   $"**Reason**: {warning.Reason}\n" +
                                   $"**Warned by**: {warnedBy?.Mention ?? warning.WarnedBy}"))
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

        [Command("warnings")]
        [Summary("Gets all warnings of given user")]
        [RequireUserPermission(GuildPermission.PrioritySpeaker)]
        public async Task GetWarnings([Optional] IGuildUser user)
        {
            _user = user;
            Global.CommandExecutorId = Context.User.Id;
            
            using (var db = new Database())
            {
                IQueryable<WarnEntry> warnings = db.Warnings;
                
                if (user != null)
                    warnings = warnings.Where(x => x.WarnedUserID == user.Id);

                _total = warnings.Count();

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
    }
}