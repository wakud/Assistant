using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.MyClasses
{
    public class ExcelResult : ActionResult
    {
        public ExcelResult(string fileName, string report)
        {
            this.Filename = fileName;
            this.Report = report;
        }
        public string Report { get; private set; }
        public string Filename { get; private set; }

        
    }
}
