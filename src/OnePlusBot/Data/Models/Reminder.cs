using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;

namespace OnePlusBot.Data.Models
{
    [Table("Reminders")]
    public class Reminder
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("reminded_user_id")]
        public ulong RemindedUserId { get; set; }

        [Column("remind_text")]
        public string RemindText { get; set; }

        [Column("reminder_date")]
        public DateTime ReminderDate { get; set; }

        [Column("target_date")]
        public DateTime TargetDate { get; set; }

        [Column("reminded")]
        public Boolean Reminded { get; set; }

        [Column("reminder_scheduled")]
        public Boolean ReminderScheduled { get; set; }

        [Column("channel_id")]
        public ulong ChannelId { get; set; }

        [Column("message_id")]
        public ulong MessageId { get; set; }
    }
}