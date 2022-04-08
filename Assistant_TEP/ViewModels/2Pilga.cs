using Assistant_TEP.MyClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.ViewModels
{
    /// <summary>
    /// звіт пільга 2 на екран
    /// </summary>
    public class _2Pilga
    {
        public IEnumerable<Pilga2> pilga2s { get; set; }
        public Dictionary<int, string> nasPunkts { get; set; }
        public string Period { get; set; }

        public _2Pilga()
        {
            nasPunkts = new Dictionary<int, string>();
        }
    }
}
