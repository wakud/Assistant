using Assistant_TEP.Models;
using Assistant_TEP.MyClasses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using DotNetDBF;

namespace Assistant_TEP.Controllers
{
    public class SunFlower : Controller
    {
        public static string UserName { get; }
        private readonly MainContext db;
        private readonly IWebHostEnvironment appEnv;
        public static IConfiguration Configuration;
        public static Dictionary<string, string> people;

        public SunFlower(MainContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            appEnv = appEnvironment;
        }

        [HttpPost]
        public IActionResult Sun1(IFormFile formFile, int Id)
        {
            Console.OutputEncoding = Encoding.GetEncoding(1251);
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            string filePath = "\\Files\\SunFlower\\" + Period.per_now().per_str + "\\";
            string fullPath = appEnv.WebRootPath + filePath + user.Id + formFile.FileName;
            if (!formFile.FileName.ToLower().EndsWith(".csv"))
            {
                ViewBag.error = "BadFile";
                return View("/Views/Home/Import.cshtml");
            }
            else
            {
                DataTable dt = new DataTable();
                string Fulltext;

                if (Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.Delete(appEnv.WebRootPath + filePath, true);

                if (!Directory.Exists(appEnv.WebRootPath + filePath))
                    Directory.CreateDirectory(appEnv.WebRootPath + filePath);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    formFile.CopyTo(fileStream);

                var srcEncoding = Encoding.GetEncoding(1251);
                using (StreamReader sr = new StreamReader(fullPath, encoding: srcEncoding))
                {
                    while (!sr.EndOfStream)
                    {
                        Fulltext = sr.ReadToEnd().ToString(); 
                        string[] rows = Fulltext.Split('\n'); 
                        for (int i = 1; i < rows.Count(); i++)  
                        {
                            string[] rowValues = rows[i].Split(";"); 
                            {
                                if (i == 1)
                                {
                                    dt.Columns.Add("Kb_a");
                                    dt.Columns.Add("Kb_a1");
                                    dt.Columns.Add("Kk_a");
                                    dt.Columns.Add("Kb_b");
                                    dt.Columns.Add("Kk_b");
                                    dt.Columns.Add("D_k");
                                    dt.Columns.Add("Summa");
                                    dt.Columns.Add("Vid");
                                    dt.Columns.Add("Ndoc");
                                    dt.Columns.Add("Ndoc1");
                                    dt.Columns.Add("I_va");
                                    dt.Columns.Add("Da");
                                    dt.Columns.Add("Da_doc");
                                    dt.Columns.Add("Nk_a");
                                    dt.Columns.Add("Nk_b");
                                    dt.Columns.Add("Nazn");
                                    dt.Columns.Add("Nazn1");
                                    dt.Columns.Add("Kod_a");
                                    dt.Columns.Add("Kod_b");
                                }
                                else
                                {
                                    DataRow dr = dt.NewRow();
                                    for (int k = 0; k < rowValues.Count(); k++)
                                    {
                                        dr[k] = rowValues[k].ToString();
                                    }
                                    dt.Rows.Add(dr); 
                                }
                            }
                        }
                    }

                }
                if (Id == 1)
                {
                    people = System.IO.File.ReadAllLines(appEnv.WebRootPath + "\\Files\\SunFlower\\spisok1.txt", Encoding.Default)
                        .Select(x => x.Split(new[] { ',' }))
                        .Where(x => x.Length == 2)
                        .ToDictionary(x => x[0], x => x[1]);
                }
                else if (Id == 2)
                {
                    people = System.IO.File.ReadAllLines(appEnv.WebRootPath + "\\Files\\SunFlower\\spisok2.txt", Encoding.Default)
                        .Select(x => x.Split(new[] { ',' }))
                        .Where(x => x.Length == 2)
                        .ToDictionary(x => x[0], x => x[1]);
                }
                else if (Id == 3)
                {
                    people = System.IO.File.ReadAllLines(appEnv.WebRootPath + "\\Files\\SunFlower\\spisok3.txt", Encoding.Default)
                        .Select(x => x.Split(new[] { ',' }))
                        .Where(x => x.Length == 2)
                        .ToDictionary(x => x[0], x => x[1]);
                }
                else if (Id == 4)
                {
                    Dictionary<string, string> people1 = System.IO.File.ReadAllLines(appEnv.WebRootPath + "\\Files\\SunFlower\\spisok1.txt", Encoding.Default)
                        .Select(x => x.Split(new[] { ',' }))
                        .Where(x => x.Length == 2)
                        .ToDictionary(x => x[0], x => x[1]);
                    Dictionary<string, string> people2 = System.IO.File.ReadAllLines(appEnv.WebRootPath + "\\Files\\SunFlower\\spisok2.txt", Encoding.Default)
                        .Select(x => x.Split(new[] { ',' }))
                        .Where(x => x.Length == 2)
                        .ToDictionary(x => x[0], x => x[1]);
                    Dictionary<string, string> people3 = System.IO.File.ReadAllLines(appEnv.WebRootPath + "\\Files\\SunFlower\\spisok3.txt", Encoding.Default)
                        .Select(x => x.Split(new[] { ',' }))
                        .Where(x => x.Length == 2)
                        .ToDictionary(x => x[0], x => x[1]);
                    people = people1.Concat(people2).ToDictionary(x => x.Key, x => x.Value);
                    people = people.Concat(people3).ToDictionary(x => x.Key, x => x.Value);
                }

                List<SunFL> pay = new List<SunFL>();
                foreach (DataRow dr in dt.Rows)
                {
                    string b = dr[15].ToString().Replace("\"", "").Trim();
                    if (people.ContainsKey(b) && Id == 1 || people.ContainsKey(b) && Id == 2
                        || people.ContainsKey(b) && Id == 3)
                    {
                        pay.Add(
                            new SunFL
                            {
                                kb_a = dr.Field<string>("Kb_a"),
                                kk_a = dr.Field<string>("Kk_a"),
                                kb_b = dr.Field<string>("Kb_b"),
                                kk_b = dr.Field<string>("Kk_b"),
                                d_k = dr.Field<string>("D_k"),
                                summa = dr.Field<string>("Summa"),
                                vid = dr.Field<string>("Vid"),
                                ndoc = dr.Field<string>("Ndoc"),
                                i_va = dr.Field<string>("I_va"),
                                da = dr.Field<string>("Da"),
                                da_doc = dr.Field<string>("Da_doc"),
                                nk_a = dr.Field<string>("Nk_a"),
                                nk_b = dr.Field<string>("Nk_b"),
                                nazn = dr.Field<string>("Nazn"),
                                nazn1 = dr.Field<string>("Nazn1"),
                                kod_a = dr.Field<string>("Kod_a"),
                                kod_b = dr.Field<string>("Kod_b")
                            }
                        );
                    }
                    else if (!people.ContainsKey(b) && Id == 4)
                    {
                        pay.Add(
                            new SunFL
                            {
                                kb_a = dr.Field<string>("Kb_a"),
                                kk_a = dr.Field<string>("Kk_a"),
                                kb_b = dr.Field<string>("Kb_b"),
                                kk_b = dr.Field<string>("Kk_b"),
                                d_k = dr.Field<string>("D_k"),
                                summa = dr.Field<string>("Summa"),
                                vid = dr.Field<string>("Vid"),
                                ndoc = dr.Field<string>("Ndoc"),
                                i_va = dr.Field<string>("I_va"),
                                da = dr.Field<string>("Da"),
                                da_doc = dr.Field<string>("Da_doc"),
                                nk_a = dr.Field<string>("Nk_a"),
                                nk_b = dr.Field<string>("Nk_b"),
                                nazn = dr.Field<string>("Nazn"),
                                nazn1 = dr.Field<string>("Nazn1"),
                                kod_a = dr.Field<string>("Kod_a"),
                                kod_b = dr.Field<string>("Kod_b")
                            }
                        );
                    }

                }

                if (Id == 4)
                {
                    pay = pay.GetRange(0, pay.Count - 3);
                }
                fullPath = appEnv.WebRootPath + "\\Files\\SunFlower\\ClientBank.dbf";
                using (Stream fos = System.IO.File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var writer = new DBFWriter())
                    {
                        writer.CharEncoding = Encoding.GetEncoding(1251);
                        writer.Signature = DBFSignature.DBase3;
                        writer.LanguageDriver = 0x57; 
                        var Kb_a = new DBFField("Kb_a", NativeDbType.Char, 6);
                        var Kk_a = new DBFField("Kk_a", NativeDbType.Char, 29);
                        var Kb_b = new DBFField("Kb_b", NativeDbType.Char, 12);
                        var Kk_b = new DBFField("Kk_b", NativeDbType.Char, 29);
                        var D_k = new DBFField("D_k", NativeDbType.Numeric, 11);
                        var Summa = new DBFField("Summa", NativeDbType.Numeric, 15, 4);
                        var Vid = new DBFField("Vid", NativeDbType.Numeric, 11);
                        var Ndoc = new DBFField("Ndoc", NativeDbType.Char, 10);
                        var I_va = new DBFField("I_va", NativeDbType.Numeric, 11);
                        var Da = new DBFField("Da", NativeDbType.Date);
                        var Da_doc = new DBFField("Da_doc", NativeDbType.Date);
                        var Nk_a = new DBFField("Nk_a", NativeDbType.Char, 38);
                        var Nk_b = new DBFField("Nk_b", NativeDbType.Char, 38);
                        var Nazn = new DBFField("Nazn", NativeDbType.Char, 160);
                        var Kod_a = new DBFField("Kod_a", NativeDbType.Char, 14);
                        var Kod_b = new DBFField("Kod_b", NativeDbType.Char, 14);
                        writer.Fields = new[]
                        {
                            Kb_a, Kk_a, Kb_b, Kk_b, D_k, Summa, Vid, Ndoc, I_va, Da, Da_doc, Nk_a, Nk_b, Nazn, Kod_a, Kod_b
                        };

                        foreach (var p in pay)
                        {
                            string n = p.nazn + "; " + p.nazn1;
                            writer.AddRecord(p.kb_a, p.kk_a, p.kb_b, p.kk_b, p.d_k, p.summa, p.vid, p.ndoc, p.i_va,
                               DateTime.Parse(p.da), DateTime.Parse(p.da_doc), p.nk_a, p.nk_b, n, p.kod_a, p.kod_b
                               );
                        }
                        writer.Write(fos);
                    }
                }
                string fileNameNew = "ClientBank_" + DateTime.Now.ToString("d") + ".dbf";
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                if(System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileNameNew);
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
