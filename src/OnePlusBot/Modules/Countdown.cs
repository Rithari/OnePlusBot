using System;
using Discord.Commands;
using System.Threading.Tasks;
using OnePlusBot._Extensions;
using Discord;


namespace OnePlusBot.Modules
{
    public class TimeLeft : ModuleBase<SocketCommandContext>
    {
        [Command("timeleft")]
        [Summary("How long until the 7 Launch event.")]
        public async Task TimeleftAsync()
        {
            try
            {

                DateTime daysLeft = DateTime.Parse("2019-05-14T15:00:00");
                DateTime startDate = DateTime.UtcNow;

                TimeSpan t = daysLeft - startDate;
                string countDown = string.Format("{0} Days, {1} Hours, {2} Minutes, {3} Seconds until launch.", t.Days, t.Hours, t.Minutes, t.Seconds);
                await Context.Channel.EmbedAsync(new EmbedBuilder().WithColor(9896005).WithDescription(countDown));
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }

        }
    }
}
