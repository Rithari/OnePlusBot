using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnePlusBot.Base
{
    public class CustomResult : RuntimeResult
    {

        public CustomResult(CommandError? error, string reason) : base(error, reason)
        {
        }

        public static MyCustomResult FromError(string reason) 
            => new MyCustomResult(CommandError.Unsuccessful, reason);

        public static MyCustomResult FromSuccess(string reason = null)
            => new MyCustomResult(null, reason);
    }
}
