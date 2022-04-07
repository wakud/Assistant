using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Модель логування
    /// </summary>
    public class LoginModel
    {
        [Required(ErrorMessage = "Не введений логін")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Не введений пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
