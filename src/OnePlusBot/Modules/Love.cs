using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using System.Text.RegularExpressions;
using System.Net;

namespace OnePlusBot.Modules
{
    public class LoveCalcModule : ModuleBase<SocketCommandContext>
    {
        [Command("lovecalc")]
        [Summary("lovecalc search for Discord!")]
        public async Task LMGTFYAsync([Remainder] string parameter)
        {
            string cacheWords = Regex.Split(" " + Context.Message.Content, @"\;lovecalc\s+")[1];
            string[] words = Regex.Split(cacheWords, @"\s+and\s+|\,\s+");
            if(words[0].Contains('#')) {words[0]=words[0].Substring(0, words[0].IndexOf('#'));}
            if(words[1].Contains('#')) {words[1]=words[1].Substring(0, words[1].IndexOf('#'));}
            cacheWords = new WebClient().DownloadString("https://www.lovecalculator.com/love.php?name1=" + words[0] + "&name2=" + words[1]);
            await ReplyAsync(":cupid: Love chance between " + words[0] + " and " + words[1] + ": `" + Regex.Split(Regex.Split(cacheWords, @"\<div\s+class\=.*?result\s+score.*?\>")[1], @"\<\/div\>")[0] + "`");
        }
    }
}
