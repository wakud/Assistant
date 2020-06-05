using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.Models
{
    public class IndexViewModel
    {
        public IEnumerable<Report> Reports { get; set; }
        public IEnumerable<ReportParam> ReportParams { get; set; }
        public IEnumerable<ReportParamType> ReportParamTypes { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel FilterViewModel { get; set; }
        public SortViewModel SortViewModel { get; set; }
    }
}
