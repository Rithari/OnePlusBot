using System.Data.Common;
using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using OnePlusBot.Data;
using OnePlusBot.Helpers;
using Discord.Commands;
using Discord.WebSocket;

namespace OnePlusBot.Base
{
    public class MuteTimer : System.Timers.Timer {
      
      public MuteTimer()
        : base( 1000 * 60 * 2 * 1)
      {

      }

       public DiscordSocketClient Bot { get; set; }

    }
}