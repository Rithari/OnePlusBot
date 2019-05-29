using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlusBot.Data.Models
{
    [Table("AuthTokens")]
    public class AuthToken
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        
        [Column("type")]
        public string Type { get; set; }
        
        [Column("token")]
        public string Token { get; set; }
    }
}