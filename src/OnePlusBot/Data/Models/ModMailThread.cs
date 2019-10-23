using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace OnePlusBot.Data.Models
{
    [Table("ModMailThread")]
    public class ModMailThread
    {
        [Column("user_id")]
        public ulong UserId { get; set; }

        [Key]
        [Column("channel_id")]
        public ulong ChannelId { get; set; }

        [Column("created_date")]
        public DateTime CreateDate { get; set; }

        [Column("update_date")]
        public DateTime UpdateDate { get; set; }

        [Column("closed_date")]
        public DateTime ClosedDate { get; set; }

        [Column("state")]
        public string State { get; set; }


        public virtual ICollection<ThreadSubscriber> Subscriber { get; set; }
    }
}