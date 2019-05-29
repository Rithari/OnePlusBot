using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("Report")]
    public class ReportEntry
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("reported_user_id")]
        public ulong ReportedUserId { get; set; }

        [Column("reported_user")]
        public string ReportedUser { get; set; }
        
        [Column("reported_by_id")]
        public ulong ReportedById { get; set; }
        
        [Column("reported_by")]
        public string ReportedBy { get; set; }
        
        [Column("channel_id")]
        public ulong ChannelID { get; set; }
        
        [Column("message_id")]
        public ulong MessageID { get; set; }
        
        [Column("reason")]
        public string Reason { get; set; }
        
        [Column("date")]
        public DateTime Date { get; set; }
    }
}