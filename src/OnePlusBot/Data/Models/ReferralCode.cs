using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("ReferralCodes")]
    public class ReferralCode
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("sender")]
        public ulong Sender { get; set; }
        
        [Column("date")]
        public DateTime Date { get; set; }
    }
}