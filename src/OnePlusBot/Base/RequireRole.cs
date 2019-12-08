using OnePlusBot.Helpers;
using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord;


namespace OnePlusBot.Base
{

    public enum ConcatenationMode {
      AND, OR
    }

    public class RequireRole : PreconditionAttribute
    {
        public readonly string[] AllowedRoles;

        public ConcatenationMode mode;

        // this is executed very early in the lifecycle, we dont have a server object or bot object yet
        public RequireRole(params string[] names) : this(names, ConcatenationMode.OR)
        {
        }

        public RequireRole(string[] names, ConcatenationMode mode)
        {
          this.AllowedRoles = names;
          this.mode = mode;
        }

        public RequireRole(string name, ConcatenationMode mode)
        {
          this.AllowedRoles = new string[] { name };
          this.mode = mode;
        }

        public RequireRole(string name) : this(name, ConcatenationMode.OR)
        {
        }


        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var bot = Global.Bot;
            var guild = bot.GetGuild(Global.ServerID);
            var iGuildObj = (IGuild) guild;
            var user = (SocketGuildUser) context.User;
            var allowed = Extensions.UserHasRole(user, AllowedRoles, mode);
            if(allowed)
            {
              return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
              return Task.FromResult(PreconditionResult.FromError("You lack the necessary role(s) to perform this command."));
            }
        }
    }
}