
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("FilteredUDWord")]
    public class FilteredUDWord
    {

        [Key]
        [Column("entry_id", Order=0)]
        public uint EntryId { get; set; }
        
        [Column("word")]
        public string Text { get; set; }

    }
}