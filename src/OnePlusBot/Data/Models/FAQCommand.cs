
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using OnePlusBot.Helpers;

namespace OnePlusBot.Data.Models
{
    [Table("FaqCommand")]
    public class FAQCommand : IPaginatable
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("name")]
        public string Name { get; set; }

        // comma separated, there is no need for an extra table for just this
        [Column("aliases")]
        public string Aliases{ get; set;}

        public string[] IndividualAliases() {
            var stringToSplit = Aliases ?? string.Empty;
            return stringToSplit.Split(',') ;
        }
        
        public virtual ICollection<FAQCommandChannel> CommandChannels { get; set; }

        public string display(){
            return $"command: {Name}";
        }
    }
}