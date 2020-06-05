using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    public class LastResults
    {
        public int ReportId { get; set; }
        public DataTable Result { get; set; }
        public Dictionary<string, string> Params { get; set; }
        public int UserId { get; set; }
        public int CokId { get; set; }
    }
}
