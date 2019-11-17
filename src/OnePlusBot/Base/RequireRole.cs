using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace OnePlusBot.Base
{

    public class RequireRole : PreconditionAttribute
    {
        private readonly string AllowedRole;

        // this is executed very early in the lifecycle, we dont have a server object or bot object yet
        public RequireRole(string name) => this.AllowedRole = name;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var bot = Global.Bot;
            var guild = bot.GetGuild(Global.ServerID);
            var iGuildObj = (IGuild) guild;
            var allowedroleObj =  iGuildObj.GetRole(Global.Roles[AllowedRole]);
            var user = (SocketGuildUser) context.User;
            if(user.Roles.Any(role => role.Id == allowedroleObj.Id)){
                return Task.FromResult(PreconditionResult.FromSuccess());
            } 
            return Task.FromResult(PreconditionResult.FromError($"You need the role {this.AllowedRole} to perform this command."));
        }
    }
}