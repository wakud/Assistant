using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.MyClasses
{
    public class ParamSerializer
    {
        public static string serializeString(string inputField)
        {
            return inputField.ToString();
        }

        public static string serializePeriod(string inputField)
        {
            return inputField.Remove(4, 1);
        }

        public static int serializeInt(string inputField)
        {
            return Int32.Parse(inputField.ToString().Trim());
        }
    }
}
