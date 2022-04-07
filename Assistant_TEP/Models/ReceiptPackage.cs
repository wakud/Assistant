using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Прийом оплат
    /// </summary>
    public class ReceiptPackage
    {
        public int ReceiptPackageId { get; set; }   //айді пачки
        public string Name { get; set; }            //назва пачки
        public int SourceId { get; set; }           //айді звідки прийшла
        public string PayDate { get; set; }         //дата оплати
        public int Cnt { get;set; }                 //к-ть оплат
        public decimal Summa { get; set; }          //сума оплат
    }
}
