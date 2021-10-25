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
using System.Text;
using System.Security.Cryptography;

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

        //Утиліта для кодування паролів
        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "RJdpFwmvPB";        //https://www.random.org/strings/
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                clearText = Convert.ToBase64String(ms.ToArray());
            }
            return clearText;
        }

        //Утиліта для розкодування паролів
        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "RJdpFwmvPB";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
            return cipherText;
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
