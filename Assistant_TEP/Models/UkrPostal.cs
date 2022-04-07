using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// оплати від укрпошти
    /// </summary>
    public class UkrPostal
    {
        public DateTime PAY_DATE { get; set; }
        public string KOD_OPZ { get; set; }
        public string REESTR_NUM { get; set; }
        public string FIO { get; set; }
        public string ADRESS { get; set; }
        public string TELEFON { get; set; }
        public long SENDER_ACC { get; set; }
        public decimal PAY_SUM { get; set; }
        public decimal SEND_SUM { get; set; }
        public string PREV { get; set; }
        public string CURR { get; set; }
        public decimal REESTR_SUM { get; set; }
        public long AccountNumber { get; set; }
    }
}
