using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assistant_TEP.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetDBF;
using Microsoft.Extensions.Configuration;
using System.Data;
using Assistant_TEP.MyClasses;
using Microsoft.Data.SqlClient;
using Assistant_TEP.ViewModels;

namespace Assistant_TEP.Controllers
{
    /// <summary>
    /// імпорт оплат, та імпорт сторонньої інформації в базу
    /// </summary>
    public class ImportController : Controller
    {
        public static string UserName { get; }
        private readonly MainContext db;
        private readonly IWebHostEnvironment appEnv;
        public static IConfiguration Configuration;
        public static Dictionary<int, float> BankImportProgress = new Dictionary<int, float>();


        public ImportController(MainContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            appEnv = appEnvironment;
        }
        /// <summary>
        /// імпортоплат з банків
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Privat(IFormFile formFile)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);

            string cokCode = user.Cok.Code;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string filePath = "\\Files\\Import\\" + Period.per_now().per_str + "\\" + cokCode + "\\";
            string fullPath = appEnv.WebRootPath + filePath + user.Id + formFile.FileName;
            //перевіряємо чи файл дбф
            if (!formFile.FileName.ToLower().EndsWith(".dbf"))
            {
                ViewBag.error = "BadFile";
                return View("/Views/Home/Import.cshtml");
            }
            //видаляємо директорію, якщо є
            if (Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.Delete(appEnv.WebRootPath + filePath, true);
            //створюємо директорію
            if (!Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.CreateDirectory(appEnv.WebRootPath + filePath);
            //зчитуємо з файлу
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
                formFile.CopyTo(fileStream);

            //перевіряємо чи файл з приватбанку
            if (formFile.FileName.ToLower().StartsWith("yrp"))
            {
                List<Privat> pryvat = new List<Privat>();

                using (var dbfDataReader = NDbfReader.Table.Open(fullPath))
                {
                    var readerDbf = dbfDataReader.OpenReader(Encoding.GetEncoding(866));
                    while (readerDbf.Read())
                    {
                        try
                        {
                            var row = new Privat();
                            row.OS_RAH_B = long.Parse(readerDbf.GetValue("OS_RAH_B").ToString().Trim());
                            row.OS_RAH_N = long.Parse(readerDbf.GetValue("OS_RAH_N").ToString().Trim());
                            row.PAYDATE = DateTime.Parse(readerDbf.GetValue("PAYDATE").ToString().Trim());
                            row.SUMMA = decimal.Parse(readerDbf.GetValue("SUMMA").ToString().Trim());
                            row.PARAMETER = readerDbf.GetValue("PARAMETER") != null ? int.Parse(readerDbf.GetValue("PARAMETER").ToString().Trim()) : 0;
                            row.CREATDATE = DateTime.Parse(readerDbf.GetValue("CREATDATE").ToString().Trim());
                            /* відключаємо не потрібні нам поля
                            row.FAMILY = readerDbf.GetValue("FAMILY").ToString().Trim();
                            row.NAME = readerDbf.GetValue("NAME").ToString().Trim();
                            row.NAME_1 = readerDbf.GetValue("NAME_1").ToString().Trim();
                            row.TOWN = readerDbf.GetValue("TOWN").ToString().Trim();
                            row.STREET = readerDbf.GetValue("STREET").ToString().Trim();
                            row.HOUSE = readerDbf.GetValue("HOUSE").ToString().Trim();
                            row.HOUSE_S = readerDbf.GetValue("HOUSE_S").ToString().Trim();
                            row.APARTMENT = readerDbf.GetValue("APARTMENT").ToString().Trim();
                            row.APARTMENTS = readerDbf.GetValue("APARTMENTS").ToString().Trim();
                            row.PAYTYPE = readerDbf.GetValue("PAYTYPE").ToString().Trim();
                            row.OPERATOR = int.Parse(readerDbf.GetValue("OPERATOR").ToString().Trim());
                            row.CREATHH = int.Parse(readerDbf.GetValue("CREATHH").ToString().Trim());
                            row.CREATMM = int.Parse(readerDbf.GetValue("CREATMM").ToString().Trim());
                            */
                            pryvat.Add(row);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            ViewBag.error = "BadFile";
                            return View("/Views/Home/Import.cshtml");
                        }
                    }
                }
                //замінюємо особові від привату на наші особові
                Dictionary<long, long> search = new Dictionary<long, long>();
                string FileScript = "AccNumber.sql";
                string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
                DataTable dt = BillingUtils.GetAccNumb(path, cokCode);

                foreach (DataRow dtRow in dt.Rows)
                {
                    try
                    {
                        search.Add(long.Parse(dtRow["AccountNumberNew"].ToString()), long.Parse(dtRow["AccountNumber"].ToString()));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                //назва файлу
                string fullPathNew = fullPath + "_" + user.Id.ToString() + "_" + "pryvat.dbf";
                //записуємо дані у файл
                using (Stream fos = System.IO.File.Open(fullPathNew, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var writer = new DBFWriter())
                    {
                        writer.CharEncoding = Encoding.GetEncoding(866);
                        writer.Signature = DBFSignature.DBase3;
                        writer.LanguageDriver = 0x26; // кодировка 866
                        var os_rah_b = new DBFField("os_rah_b", NativeDbType.Numeric, 4);
                        var os_rah_n = new DBFField("os_rah_n", NativeDbType.Numeric, 12);
                        var AccountNumber = new DBFField("accountnum", NativeDbType.Numeric, 12);
                        var paydate = new DBFField("paydate", NativeDbType.Date);
                        var summa = new DBFField("summa", NativeDbType.Numeric, 14, 2);
                        var parameter = new DBFField("parameter", NativeDbType.Numeric, 9);
                        var family = new DBFField("family", NativeDbType.Char, 50);
                        var name = new DBFField("name", NativeDbType.Char, 50);
                        var name_1 = new DBFField("name_1", NativeDbType.Char, 50);
                        var town = new DBFField("town", NativeDbType.Char, 20);
                        var street = new DBFField("street", NativeDbType.Char, 30);
                        var house = new DBFField("house", NativeDbType.Char, 10);
                        var house_s = new DBFField("house_s", NativeDbType.Char, 5);
                        var apartment = new DBFField("apartment", NativeDbType.Char, 10);
                        var apartments = new DBFField("apartments", NativeDbType.Char, 5);
                        var paytype = new DBFField("paytype", NativeDbType.Char, 15);
                        var Operator = new DBFField("operator", NativeDbType.Numeric, 15);
                        var creatdate = new DBFField("creatdate", NativeDbType.Date);
                        var creathh = new DBFField("creathh", NativeDbType.Numeric, 2);
                        var creatmm = new DBFField("creatmm", NativeDbType.Numeric, 2);

                        writer.Fields = new[] {os_rah_b, os_rah_n, AccountNumber, paydate, summa, parameter, family, name, name_1,
                            town, street, house, house_s, apartment, apartments, paytype, Operator, creatdate, creathh, creatmm
                        };

                        foreach (Privat privat in pryvat)
                        {
                            long? AccNumber = privat.AccountNumber;
                            if (search.ContainsKey(privat.OS_RAH_N))
                            {
                                AccNumber = search[privat.OS_RAH_N];
                            }
                            else
                            {
                                AccNumber = privat.OS_RAH_N;
                            }

                            writer.AddRecord(privat.OS_RAH_B, privat.OS_RAH_N, AccNumber, privat.PAYDATE, privat.SUMMA, privat.PARAMETER,
                                privat.FAMILY, privat.NAME, privat.NAME_1, privat.TOWN, privat.STREET, privat.HOUSE, privat.HOUSE_S,
                                privat.APARTMENT, privat.APARTMENTS, privat.PAYTYPE, privat.OPERATOR, privat.CREATDATE, privat.CREATHH,
                                privat.CREATMM);
                        }
                        writer.Write(fos);
                    }
                }
                //видаємо файл користувачу
                byte[] fileBytesNew = System.IO.File.ReadAllBytes(fullPathNew);
                return File(fileBytesNew, System.Net.Mime.MediaTypeNames.Application.Octet, formFile.FileName);
            }
            else if (formFile.FileName.ToLower().StartsWith("stand"))      //файл з укрпошти
            {
                List<UkrPostal> postal = new List<UkrPostal>();
                Dictionary<string, NDbfReader.IColumn> ColumnInstances = new Dictionary<string, NDbfReader.IColumn>();

                using (var dbfDataReader = NDbfReader.Table.Open(fullPath))
                {
                    var readerDbf = dbfDataReader.OpenReader(Encoding.GetEncoding(1251));
                    
                    foreach(NDbfReader.IColumn c in readerDbf.Table.Columns.ToList()){
                        ColumnInstances[c.Name.ToUpper()] = c;
                    }
                    while (readerDbf.Read())
                    {
                        try
                        {
                            var row = new UkrPostal();
                            row.PAY_DATE = DateTime.Parse(readerDbf.GetValue(ColumnInstances["PAY_DATE"]).ToString().Trim());
                            row.KOD_OPZ = readerDbf.GetValue(ColumnInstances["KOD_OPZ"]).ToString().Trim();
                            row.REESTR_NUM = readerDbf.GetValue(ColumnInstances["REESTR_NUM"]).ToString().Trim();
                            object? pip = readerDbf.GetValue(ColumnInstances["FIO"]);
                            if (pip == null)
                            {
                                row.FIO = "";
                            }
                            else
                            {
                                row.FIO = readerDbf.GetValue(ColumnInstances["FIO"]).ToString().Trim();
                            }
                            object? adresa = readerDbf.GetValue(ColumnInstances["ADRESS"]);
                            if (adresa == null)
                            {
                                row.ADRESS = "";
                            }
                            else
                            {
                                row.ADRESS = readerDbf.GetValue(ColumnInstances["ADRESS"]).ToString().Trim();
                            }
                            object? tel = readerDbf.GetValue(ColumnInstances["TELEFON"]);
                            if (tel == null)
                            {
                                row.TELEFON = "0";
                            }
                            else
                            {
                                row.TELEFON = readerDbf.GetValue(ColumnInstances["TELEFON"]).ToString().Trim();
                            }
                            row.SENDER_ACC = long.Parse(readerDbf.GetValue(ColumnInstances["SENDER_ACC"]).ToString().Trim());
                            row.PAY_SUM = decimal.Parse(readerDbf.GetValue(ColumnInstances["PAY_SUM"]).ToString().Trim());
                            row.SEND_SUM = decimal.Parse(readerDbf.GetValue(ColumnInstances["SEND_SUM"]).ToString().Trim());
                            object? pre = readerDbf.GetValue(ColumnInstances["PREV"]);
                            if (pre == null)
                            {
                                row.PREV = "";
                            }
                            else
                            {
                                row.PREV = readerDbf.GetValue(ColumnInstances["PREV"]).ToString().Trim();
                            }
                            object? cur = readerDbf.GetValue(ColumnInstances["CURR"]);
                            if (cur == null)
                            {
                                row.CURR = "";
                            }
                            else
                            {
                                row.CURR = readerDbf.GetValue(ColumnInstances["CURR"]).ToString().Trim();
                            }
                        row.REESTR_SUM = decimal.Parse(readerDbf.GetValue(ColumnInstances["REESTR_SUM"]).ToString().Trim());
                            postal.Add(row);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            ViewBag.error = "BadFile";
                            return View("/Views/Home/Import.cshtml");
                        }
                    }
                }

                Dictionary<long, long> search = new Dictionary<long, long>();
                string FileScript = "AccNumber.sql";
                string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
                DataTable dt = BillingUtils.GetAccNumb(path, cokCode);

                foreach (DataRow dtRow in dt.Rows)
                {
                    try
                    {
                        search.Add(long.Parse(dtRow["AccountNumberNew"].ToString()), long.Parse(dtRow["AccountNumber"].ToString()));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }

                string fullPathNew = fullPath + "_" + user.Id.ToString() + "_" + "ukrpost.dbf";

                using (Stream fos = System.IO.File.Open(fullPathNew, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var writer = new DBFWriter())
                    {
                        writer.CharEncoding = Encoding.GetEncoding(1251);
                        writer.Signature = DBFSignature.DBase3;
                        var PAY_DATE = new DBFField("PAY_DATE", NativeDbType.Date);
                        var KOD_OPZ = new DBFField("KOD_OPZ", NativeDbType.Char, 8);
                        var REESTR_NUM = new DBFField("REESTR_NUM", NativeDbType.Char, 20);
                        var FIO = new DBFField("FIO", NativeDbType.Char, 50);
                        var ADRESS = new DBFField("ADRESS", NativeDbType.Char, 50);
                        var TELEFON = new DBFField("TELEFON", NativeDbType.Char, 11);
                        var SENDER_ACC = new DBFField("SENDER_ACC", NativeDbType.Numeric, 20);
                        var PAY_SUM = new DBFField("PAY_SUM", NativeDbType.Numeric, 9, 2);
                        var SEND_SUM = new DBFField("SEND_SUM", NativeDbType.Numeric, 9, 2);
                        var PREV = new DBFField("PREV", NativeDbType.Char, 9);
                        var CURR = new DBFField("CURR", NativeDbType.Char, 9);
                        var REESTR_SUM = new DBFField("REESTR_SUM", NativeDbType.Numeric, 9, 2);
                        var Account = new DBFField("account", NativeDbType.Numeric, 20);

                        writer.Fields = new[] { PAY_DATE, KOD_OPZ, REESTR_NUM, FIO, ADRESS, TELEFON, SENDER_ACC, PAY_SUM,
                                                 SEND_SUM, PREV, CURR, REESTR_SUM, Account
                        };

                        foreach (UkrPostal ukrPostal in postal)
                        {
                            long? AccNumber = ukrPostal.SENDER_ACC;
                            if (search.ContainsKey(ukrPostal.SENDER_ACC))
                            {
                                AccNumber = search[ukrPostal.SENDER_ACC];
                            }
                            else
                            {
                                AccNumber = ukrPostal.SENDER_ACC;
                            }

                            writer.AddRecord(ukrPostal.PAY_DATE, ukrPostal.KOD_OPZ, ukrPostal.REESTR_NUM, ukrPostal.FIO,
                                ukrPostal.ADRESS, ukrPostal.TELEFON, ukrPostal.SENDER_ACC, ukrPostal.PAY_SUM, ukrPostal.SEND_SUM,
                                ukrPostal.PREV, ukrPostal.CURR, ukrPostal.REESTR_SUM, AccNumber
                            );
                        }

                        writer.Write(fos);
                    }
                }

                byte[] fileBytesNew = System.IO.File.ReadAllBytes(fullPathNew);
                return File(fileBytesNew, System.Net.Mime.MediaTypeNames.Application.Octet, formFile.FileName);
            }
            else    //файл з ощадбанку
            {
                List<Oschad> oschads = new List<Oschad>();
                using (var dbfDataReader = NDbfReader.Table.Open(fullPath))
                {
                    var readerDbf = dbfDataReader.OpenReader(Encoding.GetEncoding(866));
                    while (readerDbf.Read())
                    {
                        try
                        {
                            var row = new Oschad();
                            row.ppp = readerDbf.GetValue("PPP");
                            row.oper = readerDbf.GetValue("OPER");
                            row.kwyt = readerDbf.GetValue("KWYT");
                            row.dat = DateTime.Parse(readerDbf.GetValue("DAT").ToString().Trim());
                            row.tim = readerDbf.GetValue("TIM");
                            row.ora = readerDbf.GetValue("ORA");
                            row.wyd = readerDbf.GetValue("WYD");
                            row.suma = decimal.Parse(readerDbf.GetValue("SUMA").ToString().Trim());
                            object? count = readerDbf.GetValue("COUNT");
                            int OutValue;
                            if (count == null || !int.TryParse(count.ToString().Trim().Replace("/", "."), out OutValue))
                            {
                                row.count = 0;
                            }
                            else
                            {
                                row.count = OutValue;
                            }
                            row.navul = readerDbf.GetValue("NAVUL");
                            row.vl = readerDbf.GetValue("VL");
                            row.dm = readerDbf.GetValue("DM");
                            row.b = readerDbf.GetValue("B");
                            row.kwr = readerDbf.GetValue("KWR");
                            row.r = readerDbf.GetValue("R");
                            object? pip = readerDbf.GetValue("PIB");
                            if (pip == null)
                            {
                                row.pib = "";
                            }
                            else
                            {
                                row.pib = readerDbf.GetValue("PIB").ToString().Trim();
                            }
                            row.gek = readerDbf.GetValue("GEK");
                            row.numbpers = long.Parse(readerDbf.GetValue("NUMBPERS").ToString().Trim());
                            row.new_number = readerDbf.GetValue("NEW_NUMBER");
                            row.street = readerDbf.GetValue("STREET");
                            row.street_ptr = readerDbf.GetValue("STREET_PTR");
                            row.house = readerDbf.GetValue("HOUSE");
                            row.apartment = readerDbf.GetValue("APARTMENT");
                            row.tank = readerDbf.GetValue("TANK");
                            row.kb = int.Parse(readerDbf.GetValue("KB").ToString().Trim());
                            row.bank = readerDbf.GetValue("BANK");
                            row.nazv_ppp = readerDbf.GetValue("NAZV_PPP");
                            row.poppok = readerDbf.GetValue("POPPOK");
                            row.or = long.Parse(readerDbf.GetValue("OR").ToString().Trim());
                            row.or1 = long.Parse(readerDbf.GetValue("OR1").ToString().Trim());
                            row.or_old = readerDbf.GetValue("OR_OLD");
                            row.oz = readerDbf.GetValue("OZ");
                            row.bknp = readerDbf.GetValue("BKNP").ToString().Trim();
                            row.mfo = int.Parse(readerDbf.GetValue("MFO").ToString().Trim());
                            row.dat_v = readerDbf.GetValue("DAT_V");
                            row.dat_rozp = DateTime.Parse(readerDbf.GetValue("DAT_ROZP").ToString().Trim());
                            oschads.Add(row);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            ViewBag.error = "BadFile";
                            return View("/Views/Home/Import.cshtml");
                        }
                    }
                }

                Dictionary<long, long> search = new Dictionary<long, long>();
                string FileScript = "AccNumber.sql";
                string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
                DataTable dt = BillingUtils.GetAccNumb(path, cokCode);

                foreach (DataRow dtRow in dt.Rows)
                {
                    try
                    {
                        search.Add(long.Parse(dtRow["AccountNumberNew"].ToString()), long.Parse(dtRow["AccountNumber"].ToString()));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }

                string fullPathNew = fullPath + "_" + user.Id.ToString() + "_" + "oschad.dbf";

                using (Stream fos = System.IO.File.Open(fullPathNew, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var writer = new DBFWriter())
                    {
                        writer.CharEncoding = Encoding.GetEncoding(866);
                        writer.Signature = DBFSignature.DBase3;
                        writer.LanguageDriver = 0x26; // кодировка 866
                        var ppp = new DBFField("ppp", NativeDbType.Numeric, 3);
                        var oper = new DBFField("oper", NativeDbType.Char, 6);
                        var kwyt = new DBFField("kwyt", NativeDbType.Numeric, 10);
                        var dat = new DBFField("dat", NativeDbType.Date);
                        var tim = new DBFField("tim", NativeDbType.Char, 8);
                        var ora = new DBFField("ora", NativeDbType.Numeric, 7);
                        var wyd = new DBFField("wyd", NativeDbType.Numeric, 4);
                        var suma = new DBFField("suma", NativeDbType.Numeric, 8, 2);
                        var count = new DBFField("count", NativeDbType.Numeric, 7);
                        var navul = new DBFField("navul", NativeDbType.Char, 20);
                        var vl = new DBFField("vl", NativeDbType.Char, 3);
                        var dm = new DBFField("dm", NativeDbType.Char, 3);
                        var b = new DBFField("b", NativeDbType.Char, 1);
                        var kwr = new DBFField("kwr", NativeDbType.Char, 4);
                        var r = new DBFField("r", NativeDbType.Char, 1);
                        var pib = new DBFField("pib", NativeDbType.Char, 20);
                        var gek = new DBFField("gek", NativeDbType.Char, 3);
                        var numbpers = new DBFField("numbpers", NativeDbType.Numeric, 10);
                        var new_number = new DBFField("new_number", NativeDbType.Numeric, 10);
                        var street = new DBFField("street", NativeDbType.Char, 20);
                        var street_ptr = new DBFField("street_ptr", NativeDbType.Numeric, 5);
                        var house = new DBFField("house", NativeDbType.Char, 4);
                        var apartment = new DBFField("apartment", NativeDbType.Char, 4);
                        var tank = new DBFField("tank", NativeDbType.Char, 10);
                        var kb = new DBFField("kb", NativeDbType.Numeric, 3);
                        var bank = new DBFField("bank", NativeDbType.Char, 20);
                        var nazv_ppp = new DBFField("nazv_ppp", NativeDbType.Char, 20);
                        var poppok = new DBFField("poppok", NativeDbType.Numeric, 6);
                        var or = new DBFField("or", NativeDbType.Numeric, 10);
                        var or1 = new DBFField("or1", NativeDbType.Numeric, 10);
                        var or_old = new DBFField("or_old", NativeDbType.Numeric, 10);
                        var oz = new DBFField("oz", NativeDbType.Char, 1);
                        var bknp = new DBFField("bknp", NativeDbType.Char, 7);
                        var mfo = new DBFField("mfo", NativeDbType.Numeric, 6);
                        var dat_v = new DBFField("dat_v", NativeDbType.Date);
                        var dat_rozp = new DBFField("dat_rozp", NativeDbType.Date);

                        writer.Fields = new[] {ppp, oper, kwyt, dat, tim, ora, wyd, suma, count, navul, vl, dm, b, kwr, r, pib, gek, numbpers,
                            new_number, street, street_ptr, house, apartment, tank, kb, bank, nazv_ppp, poppok, or, or1, or_old, oz, bknp,
                            mfo, dat_v, dat_rozp
                        };

                        foreach (Oschad oschad in oschads)
                        {
                            long? AccNumber = oschad.or;
                            if (search.ContainsKey(oschad.or))
                            {
                                AccNumber = search[oschad.or];
                            }
                            else
                            {
                                AccNumber = oschad.or;
                            }
                            int poppok1 = 0;

                            writer.AddRecord(
                                oschad.ppp, oschad.oper, oschad.kwyt, oschad.dat, oschad.tim, oschad.ora, oschad.wyd, oschad.suma, oschad.count, oschad.navul, oschad.vl, oschad.dm,
                                oschad.b, oschad.kwr, oschad.r, oschad.pib, oschad.gek, AccNumber, oschad.new_number, oschad.street, oschad.street_ptr, oschad.house,
                                oschad.apartment, oschad.tank, oschad.kb, oschad.bank, oschad.nazv_ppp, poppok1, oschad.or, oschad.or1, oschad.or_old, oschad.oz, oschad.bknp,
                                oschad.mfo, oschad.dat_v, oschad.dat_rozp
                            );
                        }
                        writer.Write(fos);
                    }
                }
                byte[] fileBytesNew = System.IO.File.ReadAllBytes(fullPathNew);
                return File(fileBytesNew, System.Net.Mime.MediaTypeNames.Application.Octet, formFile.FileName);
            }
        }
        /// <summary>
        /// Імпорт оплат з банків
        /// </summary>
        /// <param name="file"></param>
        /// <param name="name"></param>
        /// <param name="dateIn"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]    
        public IActionResult Bank(IFormFile file, string name, DateTime dateIn, int source)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            Console.OutputEncoding = Encoding.GetEncoding(1251);
            string cokCode = user.Cok.Code;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string filePath = "\\Files\\Import\\" + Period.per_now().per_str + "\\" + cokCode + "\\";
            string fullPath = appEnv.WebRootPath + filePath + user.Id + file.FileName;

            if (Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.Delete(appEnv.WebRootPath + filePath, true);

            if (!Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.CreateDirectory(appEnv.WebRootPath + filePath);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
                file.CopyTo(fileStream);

            if (file.FileName.ToLower().StartsWith("yrp"))
            {
                List<Privat> pryvat = new List<Privat>();   
                List<Privat> badPay = new List<Privat>();   

                using (var dbfDataReader = NDbfReader.Table.Open(fullPath))
                {
                    var readerDbf = dbfDataReader.OpenReader(Encoding.GetEncoding(866));
                    while (readerDbf.Read())
                    {
                        try
                        {
                            var row = new Privat();
                            row.OS_RAH_B = long.Parse(readerDbf.GetValue("OS_RAH_B").ToString().Trim());
                            row.OS_RAH_N = long.Parse(readerDbf.GetValue("OS_RAH_N").ToString().Trim());
                            row.PAYDATE = DateTime.Parse(readerDbf.GetValue("PAYDATE").ToString().Trim());
                            row.SUMMA = decimal.Parse(readerDbf.GetValue("SUMMA").ToString().Trim());
                            row.PARAMETER = readerDbf.GetValue("PARAMETER") != null ? int.Parse(readerDbf.GetValue("PARAMETER").ToString().Trim()) : 0;
                            row.CREATDATE = DateTime.Parse(readerDbf.GetValue("CREATDATE").ToString().Trim());
                            pryvat.Add(row);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            ViewBag.error = "BadFile";
                            return View("/Views/Home/Import.cshtml");
                        }
                    }
                }

                Dictionary<long, long> search = new Dictionary<long, long>();
                Dictionary<long, long> search1 = new Dictionary<long, long>();
                string FileScript = "AccNumber.sql";
                string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
                DataTable dt = BillingUtils.GetAccNumb(path, cokCode);
                decimal sum = 0;
                int cnt = pryvat.Count;

                foreach (DataRow dtRow in dt.Rows)
                {
                    try
                    {
                        search.Add(long.Parse(dtRow["AccountNumberNew"].ToString()),
                            long.Parse(dtRow["AccountNumber"].ToString()));

                        search1.Add(long.Parse(dtRow["AccountNumberNew"].ToString()),
                            long.Parse(dtRow["AccountId"].ToString()));

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }

                foreach (Privat privat in pryvat)
                {
                    if (search.ContainsKey(privat.OS_RAH_N))
                    {
                        privat.AccountNumber = search[privat.OS_RAH_N];
                        privat.AccountId = search1[privat.OS_RAH_N];
                    }
                    else
                    {
                        privat.AccountNumber = privat.OS_RAH_N;
                        privat.AccountId = 0;
                    }
                    sum += privat.SUMMA;
                }

                ReceiptPackage rp = new ReceiptPackage()
                {
                    Name = name,
                    PayDate = dateIn.ToString("dd.MM.yyyy"),
                    Summa = sum,
                    Cnt = cnt,
                    SourceId = source
                };

                string scrypt = "INSERT_INTO ReceiptPackage.sql";
                string scryptPath = appEnv.WebRootPath + "\\Files\\Scripts\\" + scrypt;
                string fullscrypt = "USE " + cokCode + "_Utility" + "\n";
                fullscrypt += System.IO.File.ReadAllText(scryptPath, Encoding.GetEncoding(1251));
                string sqlExpression = string.Format(fullscrypt, rp.Name/*0*/, rp.PayDate/*1*/, rp.Cnt/*2*/,
                    rp.Summa.ToString().Replace(",", ".")/*3*/, rp.SourceId/*4*/);

                //BillingUtils.ExecuteRawSql(sqlExpression, cokCode);  //це буде працювати коли дадуть доступ на інсерт

                string connectionString = @"Server=172.19.1.177;Database=Assistant_TEP;User Id=sa; Password=Tepo2019;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    int lastId = 0;
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows) // якщо є дані
                    {
                        while (reader.Read())
                        {
                            lastId = int.Parse(reader.GetValue(0).ToString().Trim());
                        }
                        reader.Close();

                        int Period = 0;
                        string Script = "Інсерт оплат з банку.sql";
                        string scriptPath = appEnv.WebRootPath + "\\Files\\Scripts\\" + Script;
                        string script = "USE " + cokCode + "_Utility" + "\n";
                        script += System.IO.File.ReadAllText(scriptPath, Encoding.GetEncoding(1251));
                        float ProgressPerOneRecord = pryvat.Count != 0 ? (float)(100f / pryvat.Count) : 100f;
                        if (pryvat.Count == 0)
                        {
                            BankImportProgress[user.Id] = 100f;
                        }
                        else
                        {
                            BankImportProgress[user.Id] = 0f;
                        }
                        foreach (Privat pr in pryvat)
                        {
                            Period = Convert.ToDateTime(pr.PAYDATE).Year * 100 + Convert.ToDateTime(pr.PAYDATE).Month;

                            string sqlExpression1 = string.Format(script
                            , lastId/*0*/, pr.AccountId/*1*/, pr.PAYDATE.ToString("dd.MM.yyyy")/*2*/
                            , pr.SUMMA.ToString().Replace(",", ".")/*3*/, Period.ToString()/*4*/, Period.ToString()/*5*/
                            , pr.AccountNumber/*6*/, pr.FAMILY/*7*/, pr.PARAMETER/*8*/
                            );
                            try
                            {
                                command = new SqlCommand(sqlExpression1, connection);
                                command.ExecuteNonQuery();
                                //int number = command.ExecuteNonQuery();
                                //Console.WriteLine("Добавлено: {0}", number);
                                BankImportProgress[user.Id] += ProgressPerOneRecord;
                            }
                            catch (Exception ex)
                            {
                                badPay.Add(new Privat()
                                {
                                    FAMILY = "Особовий не знайдено",
                                    AccountNumber = pr.AccountNumber,
                                    SUMMA = pr.SUMMA,
                                    PAYDATE = pr.PAYDATE,
                                    PARAMETER = pr.PARAMETER
                                });
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                    connection.Close();
                }

                ViewBag.good = "Success";

                if (badPay.Count != 0)
                {
                    return View("/Views/Home/_BadPay.cshtml", badPay);
                }

                return View("/Views/Home/Import.cshtml");
            }
            
            return View("/Views/Home/Import.cshtml");
        }
        /// <summary>
        /// прогрес бар
        /// </summary>
        /// <returns></returns>
        public JsonResult CheckImportProgress()
        {
            User user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            if(!BankImportProgress.ContainsKey(user.Id))
            {
                BankImportProgress[user.Id] = -1f;
            }
            Console.WriteLine("Progress");
            Console.WriteLine(BankImportProgress[user.Id]);
            return Json(
                new {
                    progress = BankImportProgress[user.Id]
                }
           );
        }

    }
}
