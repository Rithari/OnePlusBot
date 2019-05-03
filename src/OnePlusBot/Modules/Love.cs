using OnePlusBot._Extensions;
using System.Threading.Tasks;
using Discord.Commands;
using System.Text.RegularExpressions;
using System.Net;
using Discord;

namespace OnePlusBot.Modules
{
    public class LoveCalcModule : ModuleBase<SocketCommandContext>
    {
        [Command("lovecalc")]
        [Summary("lovecalc search for Discord!")]
        public async Task LoveCalcAsync([Remainder] string parameter)
        {
            string first, second, cacheWords = Regex.Split(" " + Context.Message.Content, @"\;lovecalc\s+")[1];
            string[] words;
            if (cacheWords.Contains("and")) { words = Regex.Split(cacheWords, @"\s+and\s+|\,\s+"); }
            else { words = Regex.Split(cacheWords, @"\s+"); }
            first = Regex.Replace(words[0], "[^0-9a-zA-Z]", "");
            second = Regex.Replace(words[1], "[^0-9a-zA-Z]", "");
            if(words[0].Contains("@"))
            first = Context.Guild.GetUser(ulong.Parse(first)).Username;
            if(words[1].Contains("@"))
            second = Context.Guild.GetUser(ulong.Parse(second)).Username;
            cacheWords = new WebClient().DownloadString("https://www.lovecalculator.com/love.php?name1=" + first + "&name2=" + second);
            await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription(":cupid: Love chance between " + words[0] + " and " + words[1] + ": `" + Regex.Split(Regex.Split(cacheWords, @"\<div\s+class\=.*?result\s+score.*?\>")[1], @"\<\/div\>")[0] + "`"));
        }
    }
}
