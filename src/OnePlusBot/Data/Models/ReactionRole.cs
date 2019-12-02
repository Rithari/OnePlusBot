using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("ReactionRoles")]
    public class ReactionRole
    {
        
        [Column("emote_id")]
        public uint EmoteId { get; set; }
        
        [Column("role_id")]
        public ulong RoleID { get; set; }

        [Column("area")]
        public string Area { get; set; }

        [ForeignKey("RoleID")]
        public virtual Role RoleReference { get; set; }

        [ForeignKey("EmoteId")]
        public virtual StoredEmote EmoteReference { get; set; }

    }
}