using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using OnePlusBot.Base;
using OnePlusBot.Helpers;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.IO;


namespace OnePlusBot.Modules
{
    public class Fun : ModuleBase<SocketCommandContext>
    {
        [
            Command("8ball"),
            Summary("Magic 8Ball for Discord!")
        ]
        public async Task MagicBallAsync([Remainder] string search)
        {
            var answers = GetAnswers();
            var answer = answers[Global.Random.Next(answers.Length)];

            await Context.Channel.EmbedAsync(new EmbedBuilder()
                .WithColor(9896005)
                .AddField(efb =>
                {
                    efb.Name = "🎱 The 8 Ball Says:";
                    efb.Value = answer;
                }));
        }
        
        private static string[] GetAnswers()
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

        [
            Command("lovecalc"),
            Summary("lovecalc search for Discord!")
        ]
        public async Task LoveCalcAsync(string subjectA, [Remainder] string subjectB)
        {
            Random loveChance = new Random();
            int rand = loveChance.Next(0, 101);
            await ReplyAsync($":cupid: Love Chance between {subjectA} and {subjectB} is {rand}%.");
        }

        [
            Command("roulette"),
            Summary("Russian roulette for Discord!")
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
            Summary("YouTube search for Discord!")
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
            Summary("Grabs the first Urban Dictionary result based on the parameter.")
        ]
        public async Task DefineAsync([Remainder] string searchquery)
        {
            try
            {

                var definition = await ReplyAsync("Searching for definitions...");


                string json = await _clients.GetStringAsync("http://api.urbandictionary.com/v0/define?term=" + searchquery);
                var response = Response.FromJson(json);
                await definition.ModifyAsync(x => x.Content = response.List.FirstOrDefault()?.Definition ?? "No definition found");
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
            /*   await definition.ModifyAsync(x =>
                {
                   // x.Embed
                    x.Content = ":notebook_with_decorative_cover:" + "`[Urban Dictionary]`" + Regex.Replace(url, @"&#39;|\<.*?\>|&quot;|&amp;|â€™|â€œ|!â€", "\0");
               }); */
        }


         private const string BadgeUrl = "https://badges.steamprofile.com/profile/default/steam/{0}.png";
        
        [
            Command("steamp"),
            Summary("Steam profile banner for Discord!")
        ]
        public async Task<RuntimeResult> SteampAsync([Remainder] string user)
        {
            await Context.Message.AddReactionAsync(Global.OnePlusEmote.SUCCESS);

            var request = (HttpWebRequest) WebRequest.Create(string.Format(BadgeUrl, user));
            
            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                {
                    return CustomResult.FromError("Could not establish a connection to the Steam API");
                }
                
                using (var fs = File.Open("output.png", FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    await stream.CopyToAsync(fs);
                }
                
                await Context.Channel.SendFileAsync("output.png");
            }

            File.Delete("output.png");
            return CustomResult.FromSuccess();
        }

       

       
       
    }
}
