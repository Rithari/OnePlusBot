using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("Warnings")]
    public class WarnEntry
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("warned_user_id")]
        public ulong WarnedUserID { get; set; }
        
        [Column("warned_by_id")]
        public ulong WarnedByID { get; set; }
        
        [Column("reason")]
        public string Reason { get; set; }
        
        [Column("date")]
        public DateTime Date { get; set; }

        [Column("decayed")]
        public bool Decayed { get; set; }

        [Column("decayed_date")]
        public DateTime DecayedTime { get; set; }
    }
}