using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnePlusBot.Base
{
    public class CustomResult : RuntimeResult
    {

        public bool Ignore { get; }
        public CustomResult(CommandError? error, string reason, bool ignore=false) : base(error, reason)
        {
            this.Ignore = ignore;
        }

        public static CustomResult FromError(string reason) 
            => new CustomResult(CommandError.Unsuccessful, reason);

        public static CustomResult FromSuccess(string reason = null)
            => new CustomResult(null, reason);

        public static CustomResult FromIgnored() => new CustomResult(null, null, true);
        
    }
}
