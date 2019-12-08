using System;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using OnePlusBot.Data.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace OnePlusBot.Data
{
    public class Database : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }
        public DbSet<PersistentData> PersistentData { get; set; }

        public DbSet<ProfanityCheck> ProfanityChecks { get; set; }
        public DbSet<ReferralCode> ReferralCodes { get; set; }

        public DbSet<FAQCommand> FAQCommands { get; set;}
        public DbSet<FAQCommandChannel> FAQCommandChannels { get; set;}

        public DbSet<FAQCommandChannelEntry> FAQCommandChannelEntries { get; set; }
        public DbSet<WarnEntry> Warnings { get; set; }

        public DbSet<Mute> Mutes {get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        public DbSet<StarboardMessage> StarboardMessages { get; set; }

        public DbSet<ModMailThread> ModMailThreads { get; set; }

        public DbSet<ThreadSubscriber> ThreadSubscribers { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<UsedProfanity> Profanities { get; set; }

        public DbSet<ThreadMessage> ThreadMessages { get; set; }
        public DbSet<StarboardPostRelation> StarboardPostRelations { get; set; }

        public DbSet<InviteLink> InviteLinks { get; set; }

        public DbSet<ExperienceLevel> ExperienceLevels { get; set; }

        public DbSet<ExperienceRole> ExperienceRoles { get; set; }

        public DbSet<PostTarget> PostTargets { get; set; }

        public DbSet<ChannelGroup> ChannelGroups { get; set; }

        public DbSet<ChannelInGroup> ChannelGroupMembers { get; set; }

        public DbSet<StoredEmote> Emotes { get; set; }

        public DbSet<ReactionRole> ReactionRoles { get; set; }

        public DbSet<Command> Commands { get; set; }

        public DbSet<CommandModule> Modules { get; set; }

        public DbSet<CommandInChannelGroup> CommandInChannelGroups { get; set; }
        
        // TODO needs to be replaced with proper dependency injection
        [Obsolete]
        public static readonly LoggerFactory LoggerFactory
            = new LoggerFactory(new[] {
                  new ConsoleLoggerProvider((category, level) =>
                    category == DbLoggerCategory.Database.Command.Name &&
                    level == LogLevel.Information, true)
                });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var server = Environment.GetEnvironmentVariable("Server");
            var db = Environment.GetEnvironmentVariable("Database");
            var uid = Environment.GetEnvironmentVariable("Uid");
            var pwd = Environment.GetEnvironmentVariable("Pwd");
            
            if (server == null || db == null || uid == null || pwd == null)
                throw new Exception("Cannot find MySQL connection string in EnvVar.");
            
            var connStr = new MySqlConnectionStringBuilder
            {
                Server = server,
                Database = db,
                UserID = uid,
                Password = pwd
            };
            

            optionsBuilder.UseMySql(connStr.ToString());
            optionsBuilder.UseLoggerFactory(LoggerFactory);
            optionsBuilder.EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StarboardPostRelation>().HasKey(c => new { c.MessageId, c.UserId });
            modelBuilder.Entity<ThreadSubscriber>().HasKey(c => new { c.UserId, c.ModMailThreadId });
            modelBuilder.Entity<ThreadMessage>().HasKey(c => new { c.ChannelId, c.ChannelMessageId });
            modelBuilder.Entity<ChannelInGroup>().HasKey(c => new {c.ChannelId, c.ChannelGroupId});
            modelBuilder.Entity<ReactionRole>().HasKey(c => new {c.EmoteId, c.RoleID});
            modelBuilder.Entity<CommandInChannelGroup>().HasKey(c => new {c.CommandID, c.ChannelGroupId});
            modelBuilder.Entity<PostTarget>().HasIndex(p => p.Name).IsUnique();
        }

    }
}