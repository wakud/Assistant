using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Звірка з УПСЗН пільговики
    /// </summary>
    public class ZvirkaOsPilg
    {
        public int COD { get; set; }
        public long CDPR { get; set; }
        public long NCARD { get; set; }
        public string IDCODE { get; set; }
        public string PASP { get; set; }
        public string FIO { get; set; }
        public string IDPIL { get; set; }
        public string PASPPIL { get; set; }
        public string FIOPIL { get; set; }
        public int INDEX { get; set; }
        public int CDUL { get; set; }
        public string HOUSE { get; set; }
        public string BUILD { get; set; }
        public string APT { get; set; }
        public int LGCODE { get; set; }
        public int KAT { get; set; }
        public int YEARIN { get; set; }
        public int MONTHIN { get; set; }
        public int YEAROUT { get; set; }
        public int MONTHOUT { get; set; }
        public string RAH { get; set; }
        public int RIZN { get; set; }
        public long TARIF { get; set; }
    }
}
