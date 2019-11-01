using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace OnePlusBot.Data.Models
{
    [Table("ProfanityChecks")]
    public class ProfanityCheck
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("regex")]
        public string Word { get; set; }

        [Column("label")]
        public string Label { get; set; }

        [NotMapped]
        public Regex RegexObj { get; set; }
    }
}