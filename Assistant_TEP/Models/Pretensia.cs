using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Претензії до абонентів для суду
    /// </summary>
    public class Pretensia
    {
        public string Cok { get; set; }                 //назва організації
        public string Iban { get; set; }                //р/р організації
        public string Vykonavets { get; set; }          //хто виписав претензію
        public string Nach { get; set; }                //ПІП начальника
        public int AccountId { get; set; }              //айдішка особового
        public string AccountNumber { get; set; }       //особовий абонента
        public string AccountNumberNew { get; set; }    //новий особовий
        public string PIP { get; set; }                 //ПІП абонента
        public string FullAddress { get; set; }         //повна адреса
        public decimal SumaPay { get; set; }            //сума до оплати
        public DateTime DateFrom { get; set; }          //період, дата з
        public DateTime DateTo { get; set; }            //період, дата по
    }
}
