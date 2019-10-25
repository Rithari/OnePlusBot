using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("ThreadSubscribers")]
    public class ThreadSubscriber
    {

        [Column("channel_id")]
        public ulong ModMailThreadId { get; set; }

        [Column("user_id")]
        public ulong UserId { get; set; }

        [ForeignKey("ModMailThreadId")]
        public virtual ModMailThread Thread { get; set; }
    }
}