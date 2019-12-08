using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("User")]
    public class User
    {

        [Key]
        [Column("id")]
        public ulong Id { get; set; }

        [Column("modmail_muted")]
        public bool ModMailMuted { get; set; }

        [Column("modmail_muted_until")]
        public DateTime ModMailMutedUntil { get; set; }

        [Column("modmail_muted_reminded")]
        public Boolean ModMailMutedReminded { get; set; }

        [Column("xp")]
        public ulong XP { get; set; }

        [Column("xp_updated")]
        public DateTime Updated { get; set; }

        [Column("current_level")]
        public uint Level { get; set; }

        [Column("message_count")]
        public ulong MessageCount { get; set; }

        [Column("current_role_id")]
        public uint? ExperienceRoleId { get; set; }

        [Column("xp_gain_disabled")]
        public bool XPGainDisabled { get; set; }


        [ForeignKey("Level")]
        public virtual ExperienceLevel CurrentLevel { get; set; }

        [ForeignKey("ExperienceRoleId")]
        public virtual ExperienceRole ExperienceRoleReference { get; set; }

    }

    public class UserBuilder
    {
      private User InstanceToBuild;

      public UserBuilder(ulong id)
      {
        this.InstanceToBuild = new User();
        this.InstanceToBuild.Id = id;
        this.InstanceToBuild.ModMailMuted = false;
        this.InstanceToBuild.ModMailMutedReminded = false;
        this.InstanceToBuild.ModMailMutedUntil = DateTime.Now;
        this.InstanceToBuild.XPGainDisabled = false;
        this.InstanceToBuild.Level = 0;
        this.InstanceToBuild.MessageCount = 0;
        this.InstanceToBuild.XP = 0;
        this.InstanceToBuild.Updated = DateTime.Now;
      }

      public UserBuilder WithXP(ulong xp)
      {
        this.InstanceToBuild.XP = xp;
        return this;
      }

      public UserBuilder WithMessageCount(ulong messages){
        this.InstanceToBuild.MessageCount = messages;
        return this;
      }

      public UserBuilder WithModmailConfig(bool muted, bool reminded, DateTime mutedUntil)
      {
        this.InstanceToBuild.ModMailMuted = muted;
        this.InstanceToBuild.ModMailMutedReminded = reminded;
        this.InstanceToBuild.ModMailMutedUntil = mutedUntil;
        return this;
      }

      public User Build()
      {
        return InstanceToBuild;
      }

    }
}