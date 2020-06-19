using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    public class ObminPerson
    {
        public int COD { get; set; }
        public int CDPR { get; set; }
        public int NCARD { get; set; }
        public string IDPIL { get; set; }
        public string PASPPIL { get; set; }
        public string FIOPIL { get; set; }
        public int INDEX { get; set; }
        public int CDUL { get; set; }
        public string HOUSE { get; set; }
        public string? BUILD { get; set; }
        public string? APT { get; set; }
        public int KAT { get; set; }
        public int LGCODE { get; set; }
        public string DATEIN { get; set; }
        public string DATEOUT { get; set; }
        public int MONTHZV { get; set; }
        public int YEARZV { get; set; }
        public string RAH { get; set; }
        public int MONEY { get; set; }
        public string? EBK { get; set; }
        public decimal SUM_BORG { get; set; }
    }
}
