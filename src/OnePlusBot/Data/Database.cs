using System;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using OnePlusBot.Data.Models;

namespace OnePlusBot.Data
{
    public class Database : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }
        public DbSet<PersistentData> PersistentData { get; set; }
        public DbSet<ReportEntry> Reports { get; set; }
        public DbSet<ReferralCode> ReferralCodes { get; set; }
        public DbSet<WarnEntry> WarnEntries { get; set; }

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
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}