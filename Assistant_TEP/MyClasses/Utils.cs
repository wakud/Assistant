using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assistant_TEP.Models;
using System.Data;
using System.Reflection;

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
            public string NameDesc { get; set; }
        }

        //public static T CreateInstanceFromDataRow<T>(DataRow dr)
        //{
        //    T instance = Activator.CreateInstance<T>();
        //    FieldInfo[] fields = instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public |
        //          BindingFlags.Static | BindingFlags.FlattenHierarchy);
        //    foreach (FieldInfo field in fields)
        //    {
        //        Console.WriteLine(field.Name);
        //        object? val = dr[field.Name];
        //        Console.WriteLine(val);
        //        if (val != null)
        //        {
        //            Type attrType = field.FieldType;
        //            object? parsed = attrType.GetMethod("Parse").Invoke(null, new object[] { val });
        //            field.SetValue(instance, parsed);
        //        }
        //        else
        //        {
        //            field.SetValue(instance, null);
        //        }
        //        Console.WriteLine("VAL");
        //        Console.WriteLine(field.GetValue(instance));
        //    }
        //    return instance;
        //}

    }
}
