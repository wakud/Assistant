using Assistant_TEP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.ViewModels
{
    public class ResultModelView
    {
        public int ReportId { get; set; }
        public User user;
        public Organization organization;
        public DataTable results { get; set; }
    }
}
