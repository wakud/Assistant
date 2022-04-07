using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// тарифи укрпошти (створено на вимогу замовника)
    /// </summary>
    [Table("TarifUkrPost")]
    public class TarifUkrPost
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }    //назва послуги
        public decimal Price { get; set; }  //вартість послуги

    }
}
