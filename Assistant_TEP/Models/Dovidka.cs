using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    public class Dovidka
    {
        public int Id { get; set; }
        public DataTable Result { get; set; }
        public string Vykonavets { get; set; }
        public string Cok { get; set; }
        public string Nach { get; set; }
        public string FullName { get; set; }
        public string AccountNumber { get; set; }
        public string FullAddress { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<Oplata> Oplats { get; set; }
    }

    public class Oplata
    {
        public string DateOplaty { get; set; }
        public string Suma { get; set; }
    }
}
