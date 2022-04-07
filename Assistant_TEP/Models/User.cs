using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// таблиця користувачів
    /// </summary>
    [Table("User")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string FullName { get; set; }    //ПІП користувача

        [Required]
        [StringLength(10)]
        public string Login { get; set; }       //логін

        [Required]
        public string Password { get; set; }    //пароль

        [ForeignKey("CokId")]
        public int? CokId { get; set; }         //код організації

        public virtual Organization Cok { get; set; }   //назва організації

        [Required]
        public string IsAdmin { get; set; }     //чи є користувач адміном

        [Required]
        public bool AnyCok { get; set; }    //чи має користувач доступ до всіх організацій
    }
}
