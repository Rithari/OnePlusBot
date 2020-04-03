using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("EmoteHeatMap")]
    public class EmoteHeatMap
    {
        [Column("id")]
        [Key]
        public uint Id { get; set; }

        [Column("emote_id")]
        public uint Emote { get; set; }

        [Column("usage_date")]
        public DateTime UpdateDate { get; set; }

        [Column("usage_count")]
        public uint UsageCount { get; set; }

        [ForeignKey("Emote")]
        public virtual StoredEmote EmoteReference { get; set; }

    }
}