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
        public ulong UserId { get; set; }

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


        [ForeignKey("Level")]
        public virtual ExperienceLevel CurrentLevel { get; set; }

        [ForeignKey("ExperienceRoleId")]
        public virtual ExperienceRole ExperienceRoleReference { get; set; }

    }
}