using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    public class Pretensia
    {
        public string Cok { get; set; }
        public string Iban { get; set; }
        public string Vykonavets { get; set; }
        public string Nach { get; set; }
        public int AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountNumberNew { get; set; }
        public string PIP { get; set; }
        public string FullAddress { get; set; }
        public decimal SumaPay { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}
