using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using Microsoft.EntityFrameworkCore;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace OnePlusBot.Base
{
    internal static class Global
    {
        public static ulong InfoRoleManagerMessageId;

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

        public static List<Command> Commands { get; set; }

        public static ulong CommandExecutorId { get; set; }

        public static ulong StarboardStars { get; set; }

        public static ulong Level2Stars { get; set; }
        public static ulong Level3Stars { get; set; }
        public static ulong Level4Stars { get; set; }

        public static ulong ModmailCategoryId { get; set; }

        public static ulong DecayDays { get; set; }

        public static List<StarboardMessage> StarboardPosts { get; set; }

        public static Regex IllegalUserNameRegex { get; set; }

        public static ConcurrentDictionary<long, List<ulong>> RuntimeExp { get; set; }

        public static Dictionary<string, StoredEmote> Emotes { get; set; }

        public static bool XPGainDisabled { get; set; }

        public static int XPGainRangeMin { get; set; }

        public static int XPGainRangeMax { get; set; }
        
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
            RuntimeExp = new ConcurrentDictionary<long, List<ulong>>();
            Emotes = new Dictionary<string, StoredEmote>();
            Commands = new  List<Command>();
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
                
                InfoRoleManagerMessageId = db.PersistentData
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

                Level4Stars = db.PersistentData
                    .First(entry => entry.Name == "level_4_stars")
                    .Value;
                
                DecayDays = db.PersistentData
                    .First(entry => entry.Name == "decay_days")
                    .Value;

                IllegalUserNameRegex = new Regex(db.PersistentData
                    .First(entry => entry.Name == "illegal_user_name_regex")
                    .StringValue, RegexOptions.Singleline | RegexOptions.Compiled);

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

                XPGainRangeMin = (int) db.PersistentData
                    .First(entry => entry.Name == "xp_gain_range_min")
                    .Value;

                XPGainRangeMax = (int) db.PersistentData
                    .First(entry => entry.Name == "xp_gain_range_max")
                    .Value;

                Emotes.Clear();
                if(db.Emotes.Any())
                {
                    foreach(var post in db.Emotes)
                    {
                      Emotes.Add(post.Key, post);
                    }
                }

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

                Commands = db.Commands
                .Include(co => co.GroupsCommandIsIn).ThenInclude(grp => grp.ChannelGroupReference).ThenInclude(ch => ch.Channels)
                .ToList();
            }

            ReloadModmailThreads();
        }


        /// <summary>
        /// Class containing the keys by which the emotes are stored in the database
        /// </summary>
        public static class OnePlusEmote {
            public static string SUCCESS = "SUCCESS";
            public static string FAIL = "FAIL";
            public static string OP_YES =  "OP_YES";
            public static string OP_NO = "OP_NO";

            public static string STAR = "STAR";
            public static string LVL_2_STAR = "LVL_2_STAR";

            public static string LVL_3_STAR = "LVL_3_STAR";

            public static string LVL_4_STAR = "LVL_4_STAR";

        }

        public static DiscordSocketClient Bot { get; set;}
    }
}
