using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;

namespace OnePlusBot.Data.Models
{
    [Table("Mutes")]
    public class Mute
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }

        [Column("muted_user")]
        public string MutedUser { get; set; }
        
        [Column("muted_user_id")]
        public ulong MutedUserID { get; set; }
        
        [Column("muted_by")]
        public string MutedBy { get; set; }
        
        [Column("Muted_by_id")]
        public ulong MutedByID { get; set; }
        
        [Column("reason")]
        public string Reason { get; set; }
        
        [Column("mute_date")]
        public DateTime MuteDate { get; set; }

        [Column("unmute_date")]
        public DateTime UnmuteDate { get; set; }

        [Column("mute_ended")]
        public Boolean MuteEnded { get; set; }

        [Column("unmute_scheduled")]
        public Boolean UnmuteScheduled { get; set; }
    }
}