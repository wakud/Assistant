using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Монетизація субсидій
    /// </summary>
    public class MoneySubsydii
    {
        public int Raj { get; set; }                //район області
        public string Pip { get; set; }             //ПІП абонента
        public string? OsRah { get; set; }          //особовий абонента
        public long NewRah { get; set; }            //новий особовий абонента
        public double? Spogyto { get; set; }        //споживання послуги
        public double? Borg { get; set; }           //заборгованість
        public string NumberUPSZN { get; set; }     //номер УПСЗН
        public string NumberOshad { get; set; }     //рахунок в ощадбанку
        public decimal SumaOplaty { get; set; }     //сума оплати
        public DateTime? DataOplaty { get; set; }   //Дата оплати
        public long AccNumber { get; set; }         //особовий номер
    }
}
