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

        public static Dictionary<string, ulong> PostTargets { get; }

        public static Dictionary<ulong, ulong> NewsPosts { get; }
        public static List<Channel> FullChannels { get; }

        public static List<FAQCommandChannel> FAQCommandChannels { get; set;}
        public static List<FAQCommand> FAQCommands { get; set; }

        public static List<ModMailThread> ModMailThreads { get; set; }

        public static List<InviteLink> InviteLinks { get; set; }

        public static ulong CommandExecutorId { get; set; }

        public static ulong StarboardStars { get; set; }

        public static ulong Level2Stars { get; set; }
        public static ulong Level3Stars { get; set; }

        public static ulong ModmailCategoryId { get; set; }

        public static ulong DecayDays { get; set; }

        public static List<StarboardMessage> StarboardPosts { get; set; }

        public static List<Char> IllegalUserNameBeginnings { get; set; }

        public static Dictionary<long, List<ulong>> RuntimeExp { get; set; }

        public static bool XPGainDisabled { get; set; }
        
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

       public static List<ProfanityCheck> ProfanityChecks { get; }

       public static List<UsedProfanity> ReportedProfanities { get; }

        static Global()
        {
            Channels = new Dictionary<string, ulong>();
            PostTargets = new Dictionary<string, ulong>();
            FullChannels = new List<Channel>();
            Random = new Random();
            NewsPosts = new Dictionary<ulong, ulong>();  
            StarboardPosts = new List<StarboardMessage>();  
            Roles = new Dictionary<string, ulong>();
            ProfanityChecks = new List<ProfanityCheck>();
            FAQCommands = new List<FAQCommand>();
            FAQCommandChannels = new List<FAQCommandChannel>();
            InviteLinks = new List<InviteLink>();
            ModMailThreads = new List<ModMailThread>();
            ReportedProfanities = new List<UsedProfanity>();
            RuntimeExp = new Dictionary<long, List<ulong>>();
            LoadGlobal();
        }

        public static void ReloadModmailThreads(){
            using (var db = new Database())
            {
                ModMailThreads.Clear();
                ModMailThreads = db.ModMailThreads
                    .Include(sub => sub.Subscriber)
                    .Include(us => us.ThreadUser)
                    .ToList();
            }
        }

        public static void LoadGlobal(){
            using (var db = new Database())
            {
               
                Channels.Clear();
                FullChannels.Clear();
                if (db.Channels.Any()) 
                {
                    var channelObjects = db.Channels.Include(ch => ch.GroupsChannelIsIn).ThenInclude(groupMembers => groupMembers.Group);
                    foreach (var groupMember in channelObjects)
                    {
                        Channels.Add(groupMember.Name, groupMember.ChannelID);
                        FullChannels.Add(groupMember);
                    }
                }

                PostTargets.Clear();
                foreach(var target in db.PostTargets)
                {
                    PostTargets.Add(target.Name, target.ChannelId);
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
                
                DecayDays = db.PersistentData
                    .First(entry => entry.Name == "decay_days")
                    .Value;

                IllegalUserNameBeginnings = db.PersistentData
                    .First(entry => entry.Name == "user_name_illegal_characters")
                    .StringValue.ToCharArray().ToList();

                XPGainDisabled = db.PersistentData
                    .First(entry => entry.Name == "xp_disabled")
                    .Value == 1;

                InviteLinks.Clear();
                foreach(var link in db.InviteLinks)
                {
                    InviteLinks.Add(link);
                }

                ModmailCategoryId = db.PersistentData
                    .First(entry => entry.Name == "modmail_category_id")
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
                        word.RegexObj = new Regex(word.Word, RegexOptions.Singleline | RegexOptions.Compiled);
                        ProfanityChecks.Add(word);
                    }
                }


                var ReadChannels = db.FAQCommandChannels
                        .Include(faqComand => faqComand.Command)
                        .Include(faqComand => faqComand.ChannelGroupReference)
                        .Include(faqComand => faqComand.CommandChannelEntries)
                        .OrderBy(command => command.Command.ID)
                        .ToList();

                FAQCommands.Clear();
                FAQCommandChannels.Clear();

                FAQCommandChannels = ReadChannels;
                FAQCommands = db.FAQCommands.ToList();
            }

            ReloadModmailThreads();
        }

        public static class OnePlusEmote {
            public static IEmote SUCCESS = Emote.Parse("<:snow_avasnow_avasnow_avasnow_ava:604671718254182411>");
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
