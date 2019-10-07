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
        public DbSet<ReportEntry> Reports { get; set; }
        public DbSet<ReferralCode> ReferralCodes { get; set; }

        public DbSet<FAQCommand> FAQCommands { get; set;}
        public DbSet<FAQCommandChannel> FAQCommandChannels { get; set;}

        public DbSet<FAQCommandChannelEntry> FAQCommandChannelEntries { get; set; }
        public DbSet<WarnEntry> Warnings { get; set; }

        public DbSet<Mute> Mutes {get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        public DbSet<StarboardMessage> StarboardMessages { get; set; }

        public DbSet<StarboardPostRelation> StarboardPostRelations { get; set; }

        // TODO needs to be replaced with proper dependency injection
        public static readonly LoggerFactory LoggerFactory
        = new LoggerFactory(new[] {new ConsoleLoggerProvider((_, __) => true, true)});

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
        }

    }
}