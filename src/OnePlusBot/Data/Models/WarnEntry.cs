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

        [Column("warned_user")]
        public string WarnedUser { get; set; }
        
        [Column("warned_by")]
        public string WarnedBy { get; set; }
        
        [Column("reason")]
        public string Reason { get; set; }
        
        [Column("date")]
        public DateTime Date { get; set; }
    }
}