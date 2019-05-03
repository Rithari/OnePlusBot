using System;
using Discord.Commands;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;

namespace OnePlusBot.Modules
{
   public class YTModule : ModuleBase<SocketCommandContext>
    {
        [Command("yt")]
        [Summary("YouTube search for Discord!")]
        public async Task YouTubeAsync([Remainder] string parameter)
        {
            try
            {
                string html = new WebClient().DownloadString("https://www.youtube.com/results?search_query=" + Regex.Replace(Regex.Split(" " + Context.Message.Content, @"\;yt\s+")[1], @"\s+", "+"));
                await ReplyAsync("https://www.youtube.com/watch?" + Regex.Split(Regex.Split(html, @"\/watch\?")[1], "\"")[0]);
            }

            catch(Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }
    }
}
