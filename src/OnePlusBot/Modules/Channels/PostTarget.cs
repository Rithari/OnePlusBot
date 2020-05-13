using System.Threading.Tasks;
using Discord.Commands;
using OnePlusBot.Base;
using OnePlusBot.Data.Models;
using Discord;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using Discord.Addons.Interactive;

namespace OnePlusBot.Modules.Channels
{
    public partial class Channels : InteractiveBase<SocketCommandContext>
    {

      [
        Command("setPostTarget"),
        Summary("Sets the target of a certain post"),
        RequireRole("staff"),
        Alias("setTarget"),
        CommandDisabledCheck
      ]
      public async Task<RuntimeResult> SetPostTarget([Optional] string channelName, [Optional] ISocketMessageChannel channel)
      {
        if(channelName == null || channel == null)
        {
          await new ChannelManager().PostExistingPostTargets(Context.Channel);
        }
        else
        {
          new ChannelManager().SetPostTarget(channelName, channel);
        }
        await Task.CompletedTask;
        return CustomResult.FromSuccess();
      }
    }
}