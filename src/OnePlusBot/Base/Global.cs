using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using Microsoft.EntityFrameworkCore;
using Discord.WebSocket;
using System.Text.RegularExpressions;

namespace OnePlusBot.Base
{
    internal static class Global
    {
        private static ulong MessageId;

        public static Random Random { get; }

        public static ulong ServerID { get; set; }
        public static Dictionary<string, ulong> Roles { get; }
        public static Dictionary<string, ulong> Channels { get; }

        public static Dictionary<ulong, ulong> NewsPosts { get; }
        public static List<Channel> FullChannels { get; }

        public static List<FAQCommandChannel> FAQCommandChannels { get; set;}
        public static List<FAQCommand> FAQCommands { get; set; }

        public static ulong CommandExecutorId { get; set; }

        public static ulong StarboardStars { get; set; }

        public static ulong Level2Stars { get; set; }
        public static ulong Level3Stars { get; set; }

        public static List<StarboardMessage> StarboardPosts { get; set; }
        
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

       public static List<Regex> ProfanityChecks { get; }

        static Global()
        {
            Channels = new Dictionary<string, ulong>();
            FullChannels = new List<Channel>();
            Random = new Random();
            NewsPosts = new Dictionary<ulong, ulong>();  
            StarboardPosts = new List<StarboardMessage>();  
            Roles = new Dictionary<string, ulong>();
            ProfanityChecks = new List<Regex>();
            FAQCommands = new List<FAQCommand>();
            FAQCommandChannels = new List<FAQCommandChannel>();
            LoadGlobal();
        }

        public static void LoadGlobal(){
            using (var db = new Database())
            {
               
                Channels.Clear();
                FullChannels.Clear();
                if (db.Channels.Any())
                    foreach (var channel in db.Channels)
                    {
                        Channels.Add(channel.Name, channel.ChannelID);
                        FullChannels.Add(channel);
                    }
                       
                Roles.Clear();
                if (db.Roles.Any())
                    foreach (var role in db.Roles)
                        Roles.Add(role.Name, role.RoleID);

                ServerID = db.PersistentData
                    .First(x => x.Name == "server_id")
                    .Value;
                
                MessageId = db.PersistentData
                    .First(x => x.Name == "rolemanager_message_id")
                    .Value;

                StarboardStars = db.PersistentData
                    .First(entry => entry.Name == "starboard_stars")
                    .Value;

                Level2Stars = db.PersistentData
                    .First(entry => entry.Name == "level_2_stars")
                    .Value;

                Level3Stars = db.PersistentData
                    .First(entry => entry.Name == "level_3_stars")
                    .Value;

                StarboardPosts.Clear();
                if(db.StarboardMessages.Any())
                {
                    foreach(var post in db.StarboardMessages)
                    {
                        StarboardPosts.Add(post);
                    }
                }

                ProfanityChecks.Clear();
                if(db.ProfanityChecks.Any())
                {
                    foreach(var word in db.ProfanityChecks)
                    {
                        ProfanityChecks.Add(new Regex(word.Word, RegexOptions.Singleline | RegexOptions.Compiled));
                    }
                }


                var ReadChannels = db.FAQCommandChannels
                        .Include(faqComand => faqComand.Command)
                        .Include(faqComand => faqComand.Channel)
                        .Include(faqComand => faqComand.CommandChannelEntries)
                        .OrderBy(command => command.Command.ID)
                        .ToList();

                FAQCommands.Clear();
                FAQCommandChannels.Clear();

                FAQCommandChannels = ReadChannels;
                FAQCommands = db.FAQCommands.ToList();
            }
        }

        public static class OnePlusEmote {
            public static IEmote SUCCESS = Emote.Parse("<:success:499567039451758603>");
            public static IEmote FAIL = new Emoji("⚠");
            public static IEmote OP_YES =  Emote.Parse("<:OPYes:426070836269678614>");
            public static IEmote OP_NO = Emote.Parse("<:OPNo:426072515094380555>");

            public static IEmote STAR = new Emoji("⭐");
            public static IEmote LVL_2_STAR = new Emoji("🌟");

            public static IEmote LVL_3_STAR = new Emoji("💫");
        }

        public static DiscordSocketClient Bot { get; set;}
    }
}
