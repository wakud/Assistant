using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Останній результат роботи в програмі
    /// </summary>
    public class LastResults
    {
        public int ReportId { get; set; }                           //айді звіту
        public DataTable Result { get; set; }                       //таблиця даних
        public Dictionary<string, string> Params { get; set; }      //параметри
        public int UserId { get; set; }                             //айді користувача
        public int CokId { get; set; }                              //айді організації
    }
}
