using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.MyClasses
{
    public static class Utils
    {

        public static Task DeleteAsyncFile(string fileName)
        {
            return Task.Factory.StartNew(() => File.Delete(fileName));
        }

    }
}
