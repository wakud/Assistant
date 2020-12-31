using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assistant_TEP.Models;
namespace Assistant_TEP.MyClasses
{
    public static class Utils
    {
        public static Task DeleteAsyncFile(string fileName)
        {
            return Task.Factory.StartNew(() => File.Delete(fileName));
        }

        public class SelectParamReport
        {
            public string Name { get; set; }
            public string Id { get; set; }
        }

        public class ParamSelectData
        {
            public SelectList selects { get; set; }
            public string NameParam { get; set; }
        }

    }
}
