using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using OnePlusBot.Data;
using Discord.WebSocket;

namespace OnePlusBot.Base
{
    internal static class Global
    {
        private static readonly ulong MessageId;

        public static Random Random { get; }

        public static ulong ServerID { get; }
        public static Dictionary<string, ulong> Roles { get; }
        public static Dictionary<string, ulong> Channels { get; }

        public static ulong CommandExecutorId { get; set; }
        
        public static string Token
        {
            get
            {
                using (var db = new Database())
                {
                    return db.AuthTokens
                        .First(x => x.Type == "stable")
                        .Token;
                }
            }
        }

        public static string TokenBeta
        {
            get
            {
                using (var db = new Database())
                {
                    return db.AuthTokens
                        .First(x => x.Type == "beta")
                        .Token;
                }
            }
        }

        public static ulong RoleManagerMessageId
        {
            get => MessageId;
            set
            {
                using (var db = new Database())
                {
                    db.PersistentData
                        .First(x => x.Name == "rolemanager_message_id")
                        .Value = value;
                    db.SaveChanges();
                }
            }
        }

        static Global()
        {
            Random = new Random();
            using (var db = new Database())
            {
                Channels = new Dictionary<string, ulong>();
                if (db.Channels.Any())
                    foreach (var channel in db.Channels)
                        Channels.Add(channel.Name, channel.ChannelID);
                
                Roles = new Dictionary<string, ulong>();
                if (db.Roles.Any())
                    foreach (var role in db.Roles)
                        Roles.Add(role.Name, role.RoleID);

                ServerID = db.PersistentData
                    .First(x => x.Name == "server_id")
                    .Value;
                
                MessageId = db.PersistentData
                    .First(x => x.Name == "rolemanager_message_id")
                    .Value;
            }
        }

        public static class OnePlusEmote {
            public static IEmote SUCCESS = Emote.Parse("<:shelAva:404330255025831939>");
            public static IEmote FAIL = new Emoji("⚠");
            public static IEmote OP_YES =  Emote.Parse("<:OPYes:426070836269678614>");
            public static IEmote OP_NO = Emote.Parse("<:OPNo:426072515094380555>");
        }

        public static DiscordSocketClient Bot { get; set;}
    }
}
