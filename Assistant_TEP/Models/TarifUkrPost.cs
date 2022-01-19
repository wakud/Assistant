using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assistant_TEP.Models
{
    [Table("TarifUkrPost")]
    public class TarifUkrPost
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

    }
}
