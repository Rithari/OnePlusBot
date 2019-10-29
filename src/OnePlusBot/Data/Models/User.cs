using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("User")]
    public class User
    {

        [Key]
        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("modmail_muted")]
        public bool ModMailMuted { get; set; }

        [Column("modmail_muted_until")]
        public DateTime ModMailMutedUntil { get; set; }

        [Column("modmail_muted_reminded")]
        public Boolean ModMailMutedReminded { get; set; }
    }
}