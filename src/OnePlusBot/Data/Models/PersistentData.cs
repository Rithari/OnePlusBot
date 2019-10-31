using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("PersistentData")]
    public class PersistentData
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("name")]
        public string Name { get; set; }
        
        [Column("val")]
        public ulong Value { get; set; }

        [Column("string")]
        public string StringValue { get; set; }
    }
}