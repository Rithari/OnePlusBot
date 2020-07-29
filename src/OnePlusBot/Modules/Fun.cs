using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using OnePlusBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnePlusBot.Modules
{
    [
      Summary("Module containing some entertainment commands.")
    ]
    public class Fun : ModuleBase<SocketCommandContext>
    {
        [
            Command("choose"),
            Summary("Choose between two or more things!"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> ChooseAsync(params string[] argArray)
        {
            if (argArray == null || argArray.Length == 0)
            {
                return CustomResult.FromError("I can't choose nothingness.");
            }

            var answer = argArray[Global.Random.Next(argArray.Length)];

            if (answer.Contains("@everyone") || answer.Contains("@here") || Context.Message.MentionedRoles.Count > 0)
            {
                return CustomResult.FromError("Your command contained one or more illegal pings!");
            }

            await Context.Channel.SendMessageAsync($"I've chosen {answer}");
            return CustomResult.FromSuccess();
        }

        [
            Command("8ball"),
            Summary("Magic 8Ball for Discord!"),
            CommandDisabledCheck
        ]
        public async Task MagicBallAsync([Remainder] string search)
        {
            var answers = Get8BallAnswers();
            var answer = answers[Global.Random.Next(answers.Length)];

            await ReplyAsync($"🎱 The 8 Ball Says: {answer}");
        }
       

        [
            Command("flip"),
            Summary("Flip a coin!"),
            CommandDisabledCheck
        ]
        public async Task CoinFlip()
        {
            var answers = GetCoinFlip();
            var answer = answers[Global.Random.Next(answers.Length)];

            await ReplyAsync($"📣 You got {answer}!");
        }

        [
            Command("roll"),
            Summary("Roll a dice!"),
            CommandDisabledCheck
        ]
        public async Task DiceRoll()
        {
            var number = Global.Random.Next(1, 7);

            await ReplyAsync($"🎲 You rolled {number}!");
        }

        private static string[] Get8BallAnswers()
        {
            return new[]
            {
                "It is certain.", "It is decidedly so.", "Without a doubt.", "Yes, definitely.", "You may rely on it.",
                "As I see it, yes.", "Most likely.", "Outlook good.", "Yes.", "Signs point to yes.",
                "Reply hazy to try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.",
                "Concentrate and ask again.", "Don't count on it.", "My reply is no.", "Outlook not so good.",
                "Very doubtful.",
                "My sources say no."
            };
        }

        private static string[] GetCoinFlip()
        {
            return new[]
            {
                "Heads", "Tails"
            };
        }

        [
            Command("lovecalc"),
            Summary("lovecalc search for Discord!"),
            CommandDisabledCheck
        ]
        public async Task LoveCalcAsync(string subjectA, [Remainder] string subjectB)
        {
            int rand = Global.Random.Next(0, 101);
            await ReplyAsync($":cupid: Love Chance between {subjectA} and {subjectB} is {rand}%.");
        }

        [
            Command("roulette"),
            Summary("Russian roulette for Discord!"),
            CommandDisabledCheck
        ]
        public async Task RouletteAsync()
        {
            var answers = new[]
            {
                ":gun: *click*, no bullet in there for you this round.\r\n",
                ":gun: :boom:, you died! :skull:\r\n", ":gun: *click*, no bullet in there for you this round.\r\n"
            };

            var answer = answers[Global.Random.Next(3) % 2]; // 1 out of 3 possibility of death
            await ReplyAsync(answer);
        }

        [
            Command("yt"),
            Summary("YouTube search for Discord!"),
            CommandDisabledCheck
        ]
        public async Task YouTubeAsync([Remainder] string parameter)
        {
            try
            {
                string html = new WebClient().DownloadString("https://www.youtube.com/results?search_query=" + Regex.Replace(Regex.Split(" " + Context.Message.Content, @"\;yt\s+")[1], @"\s+", "+"));
                await ReplyAsync("https://www.youtube.com/watch?" + Regex.Split(Regex.Split(html, @"\/watch\?")[1], "\"")[0]);
            }

            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        private readonly HttpClient _clients;

        public Fun(HttpClient clients)
        {
            _clients = clients;
        }


        [
            Command("define"),
            Summary("Grabs the first Urban Dictionary result based on the parameter."),
            CommandDisabledCheck
        ]
        public async Task DefineAsync([Remainder] string searchquery)
        {
            try
            {

                var definition = await ReplyAsync("Searching for definitions...");


                string json = await _clients.GetStringAsync("http://api.urbandictionary.com/v0/define?term=" + searchquery);
                var response = Response.FromJson(json);
                var text = response.List.FirstOrDefault()?.Definition ?? "No definition found";
                foreach(var filtered in Global.FilteredUdWords)
                {
                  var index = text.IndexOf(filtered.Text.ToLower(), StringComparison.CurrentCultureIgnoreCase);
                  if(index != -1)
                  {
                    var stringBuilderForModifying = new StringBuilder(text);
                    stringBuilderForModifying.Remove(index, filtered.Text.ToLower().Length);
                    stringBuilderForModifying.Insert(index, "**redacted**");
                    text = stringBuilderForModifying.ToString();
                  }
                }
                await definition.ModifyAsync(x => x.Content = text);
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [
            Command("starstats"),
            Summary("Shows the statistics for the star board posts"),
            CommandDisabledCheck
        ]
        public async Task<RuntimeResult> PrintStarStats()
        {
            var mostStars = new Dictionary<ulong, ulong>();
            using(var db = new Database())
            {
                var mostStarredMessages = db.StarboardMessages.AsQueryable().Where(p => !p.Ignored).OrderByDescending(p => p.Starcount).Take(3).ToArray();
                var starMessagesCount = db.StarboardMessages.AsQueryable().Where(p => !p.Ignored).Count();
                // we could either store the ignored flag in the relation as well and duplicate the flag, or we could join here...
                var totalStarsCount = db.StarboardPostRelations.Include(message => message.Message).Where(message => !message.Message.Ignored).Count();
                
                var mostStarerUser = db.StarboardPostRelations
                .Include(message => message.Message)
                .Where(message => !message.Message.Ignored)
                .GroupBy(p => p.UserId)
                .Select(g => new { id = g.Key, count = g.Count() })
                .OrderByDescending(p => p.count)
                .Select(p => new KeyValuePair<ulong, int>(p.id, p.count))
                .ToArray();

                var mostStarRecieverUser = db.StarboardMessages
                .AsQueryable().Where(p => !p.Ignored)
                .GroupBy(p => p.AuthorId)
                .Select(g => new { id = g.Key, count = g.Sum(p => p.Starcount)})
                .OrderByDescending(p => p.count)
                .Select(p => new KeyValuePair<ulong, int>(p.id, (int) p.count))
                .ToArray();

                var embed = new EmbedBuilder();
                embed.WithTitle("Server Starboard stats");
                embed.WithDescription($"{starMessagesCount} starred messages with {totalStarsCount} stars in total");
                var badges = new List<String>();
                badges.Add("🥇");
                badges.Add("🥈");
                badges.Add("🥉");
                var firstPostField = new EmbedFieldBuilder()
                .WithName("Top starred posts")
                .WithValue(BuildStringForMessage(mostStarredMessages, badges));

                var secondPostField = new EmbedFieldBuilder()
                .WithName("Top star receiver")
                .WithValue(BuildStringForPoster(mostStarRecieverUser, badges));

                var thirdPostField = new EmbedFieldBuilder()
                .WithName("Top star givers")
                .WithValue(BuildStringForPoster(mostStarerUser, badges));
                embed.WithFields(firstPostField);
                embed.WithFields(secondPostField);
                embed.WithFields(thirdPostField);
                await Context.Channel.SendMessageAsync(embed: embed.Build());
            }
            return CustomResult.FromSuccess();
        }

        private String BuildStringForPoster(KeyValuePair<ulong, int>[] values, List<String> badges)
        {

            var stringBuilder = new StringBuilder();
            for(var index = 0; index < badges.Count(); index++)
            {
                if(index >= values.Count())
                {
                    continue;
                }
                var element = values.ElementAt(index);

                var user = Extensions.GetUserById(element.Key);
                var userReference = user != null ? user.Mention : "User left the server. " + element.Key;

                stringBuilder.Append(badges[index]); 
                stringBuilder.Append(" - ");
                stringBuilder.Append(element.Value);
                stringBuilder.Append(" ");
                stringBuilder.Append(StoredEmote.GetEmote(Global.OnePlusEmote.STAR));
                stringBuilder.Append(" ");
                stringBuilder.Append(userReference);
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }

        private String BuildStringForMessage(Data.Models.StarboardMessage[] values, List<String> badges)
        {
            var starBoardChannelId = Global.PostTargets[PostTarget.STARBOARD];
            var stringBuilder = new StringBuilder();
            for(var index = 0; index < badges.Count(); index++)
            {
                if(index >= values.Count())
                {
                    continue;
                }
                var element = values.ElementAt(index);
                stringBuilder.Append(badges[index]);
                stringBuilder.Append(" - ");
                stringBuilder.Append(element.Starcount);
                stringBuilder.Append(" ");
                stringBuilder.Append(StoredEmote.GetEmote(Global.OnePlusEmote.STAR));
                stringBuilder.Append(" ");
                stringBuilder.Append(Extensions.GetMessageUrl(Global.ServerID, starBoardChannelId, element.StarboardMessageId, "Jump!"));
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }
       
       
    }
}
