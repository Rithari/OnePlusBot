using System.Collections.Generic;
using System.Linq;
using OnePlusBot.Data;

namespace OnePlusBot.Base
{
    internal static class Global
    {
        public static ulong ServerID { get; }
        public static Dictionary<string, ulong> Channels { get; }
        
        public static string Token
        {
            get
            {
                using (var db = new Database())
                {
                    return db.AuthTokens
                        .Where(x => x.Type == "stable")
                        .Select(x => x.Token)
                        .ToString();
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
                        .Where(x => x.Type == "beta")
                        .Select(x => x.Token)
                        .ToString();
                }
            }
        }
        
        public static ulong RoleManagerId { get; set; }

        static Global()
        {
            using (var db = new Database())
            {
                Channels = new Dictionary<string, ulong>();
                foreach (var channel in db.Channels)
                    Channels.Add(channel.Name, channel.ChannelID);

                ServerID = db.PersistentData
                    .First(x => x.Name == "server_id")
                    .Value;
                
                RoleManagerId = db.PersistentData
                    .First(x => x.Name == "rolemanager_message_id")
                    .Value;
            }
        }
    }
}
