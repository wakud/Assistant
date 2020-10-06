using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Assistant_TEP.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string FullName { get; set; }

        [Required]
        [StringLength(10)]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        [ForeignKey("CokId")]
        public int? CokId { get; set; }

        public virtual Organization Cok { get; set; }

        [Required]
        public string IsAdmin { get; set; }

        [Required]
        public bool AnyCok { get; set; }
    }
}
