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

        public static Regex LegalUserNameRegex { get; set; }

        public static ConcurrentDictionary<long, List<ulong>> RuntimeExp { get; set; }

        public static Dictionary<string, StoredEmote> Emotes { get; set; }

        public static bool XPGainDisabled { get; set; }

        public static int XPGainRangeMin { get; set; }

        public static int XPGainRangeMax { get; set; }

        public static Dictionary<ulong, ulong> UserNameNotifications { get; set; }

        public static int ProfanityVoteThreshold { get; set; }

        
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
            UserNameNotifications = new Dictionary<ulong, ulong>();
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

                ServerID = PersistentData.GetConfiguredInt("server_id", db);
                
                InfoRoleManagerMessageId = PersistentData.GetConfiguredInt("rolemanager_message_id", db);

                StarboardStars = PersistentData.GetConfiguredInt("starboard_stars", db);

                Level2Stars = PersistentData.GetConfiguredInt("level_2_stars", db);

                Level3Stars = PersistentData.GetConfiguredInt("level_3_stars", db);

                Level4Stars = PersistentData.GetConfiguredInt("level_4_stars", db);
                
                DecayDays = PersistentData.GetConfiguredInt("decay_days", db);

                LegalUserNameRegex = new Regex(PersistentData.GetConfiguredString("legal_user_name_regex", db), RegexOptions.Singleline | RegexOptions.Compiled);

                XPGainDisabled = PersistentData.GetConfiguredInt("xp_disabled", db) == 1;
                  
                ProfanityVoteThreshold = (int) PersistentData.GetConfiguredInt("profanity_votes_threshold", db);

                InviteLinks.Clear();
                foreach(var link in db.InviteLinks)
                {
                    InviteLinks.Add(link);
                }

                ModmailCategoryId = PersistentData.GetConfiguredInt("modmail_category_id", db);

                XPGainRangeMin = (int) PersistentData.GetConfiguredInt("xp_gain_range_min", db);

                XPGainRangeMax = (int) PersistentData.GetConfiguredInt("xp_gain_range_max", db);

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

            public static string OPEN_MODMAIL = "OPEN_MODMAIL";

        }

        public static DiscordSocketClient Bot { get; set;}
    }
}
