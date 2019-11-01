using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("UsedProfanity")]
    public class UsedProfanity
    {

        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("message_id")]
        [Key]
        public ulong MessageId { get; set; }

        [Column("profanity_id")]
        public uint ProfanityId { get; set; }

        [Column("valid")]
        public bool Valid { get; set; }

        [ForeignKey("UserId")]
        public virtual User ProfanityUser { get; set; }

        [ForeignKey("ProfanityId")]
        public virtual ProfanityCheck TriggeredProfanity { get; set; }
    }
}