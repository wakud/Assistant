using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using Assistant_TEP.Models;
using Assistant_TEP.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetDBF;
using Microsoft.Extensions.Configuration;
using System.Data;
using Assistant_TEP.MyClasses;
using ClosedXML.Excel;

namespace Assistant_TEP.Controllers
{
    public class ObminController : Controller
    {
        public static string UserName { get; }
        private readonly MainContext db;
        private readonly IWebHostEnvironment appEnv;
        public static IConfiguration Configuration;
        private readonly XLWorkbook wb;
        public static Dictionary<int, float> BankImportProgress = new Dictionary<int, float>();

        public ObminController(MainContext context, IWebHostEnvironment appEnviroment)
        {
            db = context;
            appEnv = appEnviroment;
            wb = new XLWorkbook();
        }

        //****Запит про наявність боргу по пільговикам****
        public ActionResult Pilg()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Pilg(IFormFile formFile)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            string cokCode = user.Cok.Code;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string filePath = "\\Files\\Obmin\\" + Period.per_now().per_str + "\\" + cokCode + "\\";
            string fullPath = appEnv.WebRootPath + filePath + formFile.FileName + user.Id;

            //видаляємо директорію
            if (Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.Delete(appEnv.WebRootPath + filePath, true);

            //створюємо директорію
            if (!Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.CreateDirectory(appEnv.WebRootPath + filePath);

            //зберігаємо файл
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
                formFile.CopyTo(fileStream);

            List<ObminPerson> obmins = new List<ObminPerson>();

            //Зчитуємо з .dbf і закидаємо в ліст
            using (var dbfDataReader = NDbfReader.Table.Open(fullPath))
            {
                var readerDbf = dbfDataReader.OpenReader(Encoding.GetEncoding(866));
                while (readerDbf.Read())
                {
                    try
                    {
                        var row = new ObminPerson();
                        row.COD = int.Parse(readerDbf.GetValue("COD").ToString().Trim());
                        row.CDPR = int.Parse(readerDbf.GetValue("CDPR").ToString().Trim());
                        row.NCARD = int.Parse(readerDbf.GetValue("NCARD").ToString().Trim());
                        row.IDPIL = readerDbf.GetValue("IDPIL")?.ToString().Trim();
                        row.PASPPIL = readerDbf.GetValue("PASPPIL")?.ToString().Trim();
                        row.FIOPIL = readerDbf.GetValue("FIOPIL").ToString().Trim();
                        row.INDEX = int.Parse(readerDbf.GetValue("INDEX").ToString().Trim());
                        row.CDUL = int.Parse(readerDbf.GetValue("CDUL").ToString().Trim());
                        row.HOUSE = readerDbf.GetValue("HOUSE").ToString().Trim();
                        row.BUILD = readerDbf.GetValue("BUILD")?.ToString().Trim();
                        row.APT = readerDbf.GetValue("APT")?.ToString().Trim();
                        row.KAT = int.Parse(readerDbf.GetValue("KAT").ToString().Trim());
                        row.LGCODE = int.Parse(readerDbf.GetValue("LGCODE").ToString().Trim());
                        row.DATEIN = readerDbf.GetValue("DATEIN").ToString().Trim();
                        row.DATEOUT = readerDbf.GetValue("DATEOUT").ToString().Trim();
                        row.MONTHZV = int.Parse(readerDbf.GetValue("MonthZv").ToString().Trim());
                        row.YEARZV = int.Parse(readerDbf.GetValue("YearZv").ToString().Trim());
                        row.RAH = readerDbf.GetValue("RAH").ToString().Trim();
                        row.MONEY = int.Parse(readerDbf.GetValue("MONEY").ToString().Trim());
                        row.EBK = readerDbf.GetValue("EBK")?.ToString().Trim();
                        row.SUM_BORG = decimal.Parse(readerDbf.GetValue("Sum_Borg").ToString().Trim());
                        obmins.Add(row);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        ViewBag.error = "BadFile";
                        return View();
                    }
                }
            }

            Dictionary<string, decimal> vybir = new Dictionary<string, decimal>();
            string FileScript = "obmin_pilg.sql";
            string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
            DataTable dt = BillingUtils.GetResults(path, cokCode);

            foreach (DataRow dtRow in dt.Rows)
            {
                vybir.Add(dtRow.Field<string>("AccountNumber"), dtRow.Field<decimal>("RestSumm"));
            }

            using (Stream fos = System.IO.File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var writer = new DBFWriter())
                {
                    writer.CharEncoding = Encoding.GetEncoding(866);
                    writer.Signature = DBFSignature.DBase3;
                    writer.LanguageDriver = 0x26; // кодировка 866
                    var field1 = new DBFField("COD", NativeDbType.Numeric, 4);
                    var field2 = new DBFField("CDPR", NativeDbType.Numeric, 12);
                    var field3 = new DBFField("NCARD", NativeDbType.Numeric, 10);
                    var field4 = new DBFField("IDPIL", NativeDbType.Char, 10);
                    var field5 = new DBFField("PASPPIL", NativeDbType.Char, 14);
                    var field6 = new DBFField("FIOPIL", NativeDbType.Char, 50);
                    var field7 = new DBFField("INDEX", NativeDbType.Numeric, 6);
                    var field8 = new DBFField("CDUL", NativeDbType.Numeric, 5);
                    var field9 = new DBFField("HOUSE", NativeDbType.Char, 7);
                    var field10 = new DBFField("BUILD", NativeDbType.Char, 2);
                    var field11 = new DBFField("APT", NativeDbType.Char, 4);
                    var field12 = new DBFField("KAT", NativeDbType.Numeric, 4);
                    var field13 = new DBFField("LGCODE", NativeDbType.Numeric, 4);
                    var field14 = new DBFField("DATEIN", NativeDbType.Char, 10);
                    var field15 = new DBFField("DATEOUT", NativeDbType.Char, 10);
                    var field16 = new DBFField("MONTHZV", NativeDbType.Numeric, 2);
                    var field17 = new DBFField("YEARZV", NativeDbType.Numeric, 4);
                    var field18 = new DBFField("RAH", NativeDbType.Char, 25);
                    var field19 = new DBFField("MONEY", NativeDbType.Numeric, 1);
                    var field20 = new DBFField("EBK", NativeDbType.Char, 10);
                    var field21 = new DBFField("SUM_BORG", NativeDbType.Numeric, 9, 2);

                    writer.Fields = new[] { field1, field2, field3, field4, field5, field6, field7, field8, field9, field10,
                                    field11, field12, field13, field14, field15, field16, field17, field18, field19, field20, field21 };

                    foreach (ObminPerson obmin in obmins)
                    {
                        decimal borg = obmin.SUM_BORG;
                        if (vybir.ContainsKey(obmin.RAH))
                        {
                            borg = vybir[obmin.RAH];
                        }

                        writer.AddRecord(
                        obmin.COD, obmin.CDPR, obmin.NCARD, obmin.IDPIL, obmin.PASPPIL, obmin.FIOPIL, obmin.INDEX, obmin.CDUL, obmin.HOUSE,
                        obmin.BUILD, obmin.APT, obmin.KAT, obmin.LGCODE, obmin.DATEIN, obmin.DATEOUT, obmin.MONTHZV, obmin.YEARZV, obmin.RAH,
                        obmin.MONEY, obmin.EBK, borg
                        );
                    }
                    writer.Write(fos);
                }
            }
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            string[] nameParts = formFile.FileName.Split(".");
            string newName = nameParts[0] + "." + nameParts[1].Replace("M", "N");
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, newName);
        }
        //----------------------------------------------------
        //****Запит про наявність боргу по субсидіям****
        public ActionResult Subs(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Subs(IFormFile formFile)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            string cokCode = user.Cok.Code;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string filePath = "\\Files\\Obmin\\" + Period.per_now().per_str + "\\" + cokCode + "\\";
            string fullPath = appEnv.WebRootPath + filePath + formFile.FileName + user.Id;

            //видаляємо директорію
            if (Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.Delete(appEnv.WebRootPath + filePath, true);

            //створюємо директорію
            if (!Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.CreateDirectory(appEnv.WebRootPath + filePath);

            //зберігаємо файл
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
                formFile.CopyTo(fileStream);
            
            List<ObminSubs> obminSubs = new List<ObminSubs>();
            //Зчитуємо з .dbf і закидаємо в ліст
            using (var dbfDataReader = NDbfReader.Table.Open(fullPath))
            {
                var readerDbf = dbfDataReader.OpenReader(Encoding.GetEncoding(866));
                while (readerDbf.Read())
                {
                    try
                    {
                        var row = new ObminSubs();
                        row.APP_NUM = readerDbf.GetValue("APP_NUM")?.ToString().Trim();
                        row.ZAP_R = int.Parse(readerDbf.GetValue("ZAP_R")?.ToString().Trim());
                        row.ZAP_N = int.Parse(readerDbf.GetValue("ZAP_N")?.ToString().Trim());
                        var rawValue = readerDbf.GetValue("OWN_NUM")?.ToString().Trim();
                        if (!long.TryParse(rawValue, out long TempNum))
                        {
                            //Console.WriteLine("NOT CORRECT");
                            //Console.WriteLine(rawValue);
                            ViewBag.error = "badOs";
                            ViewData["Message"] = rawValue;
                        }
                        else
                        {
                            row.OWN_NUM = TempNum;
                        }
                        row.SUR_NAM = readerDbf.GetValue("SUR_NAM")?.ToString().Trim();
                        row.F_NAM = readerDbf.GetValue("F_NAM")?.ToString().Trim();
                        row.M_NAM = readerDbf.GetValue("M_NAM")?.ToString().Trim();
                        row.ENT_COD = readerDbf.GetValue("ENT_COD")?.ToString().Trim();
                        row.DATA_S = DateTime.Parse(readerDbf.GetValue("DATA_S")?.ToString().Trim());
                        row.RES2 = int.Parse(readerDbf.GetValue("RES2")?.ToString().Trim());
                        row.ADR_NAM = readerDbf.GetValue("ADR_NAM")?.ToString().Trim();
                        row.VUL_COD = int.Parse(readerDbf.GetValue("VUL_COD")?.ToString().Trim());
                        row.VUL_CAT = readerDbf.GetValue("VUL_CAT")?.ToString().Trim();
                        row.VUL_NAM = readerDbf.GetValue("VUL_NAM")?.ToString().Trim();
                        row.BLD_NUM = readerDbf.GetValue("BLD_NUM")?.ToString().Trim();
                        row.CORP_NUM = readerDbf.GetValue("CORP_NUM")?.ToString().Trim();
                        row.FLAT = readerDbf.GetValue("FLAT")?.ToString().Trim();
                        row.NUMB = int.Parse(readerDbf.GetValue("NUMB")?.ToString().Trim());
                        row.NUM_P = int.Parse(readerDbf.GetValue("NUM_P")?.ToString().Trim());
                        row.PLG_COD = int.Parse(readerDbf.GetValue("PLG_COD")?.ToString().Trim());
                        row.PLG_N = int.Parse(readerDbf.GetValue("PLG_N")?.ToString().Trim());
                        row.CM_AREA = decimal.Parse(readerDbf.GetValue("CM_AREA")?.ToString().Trim());
                        row.OP_AREA = decimal.Parse(readerDbf.GetValue("OP_AREA")?.ToString().Trim());
                        row.NM_AREA = decimal.Parse(readerDbf.GetValue("NM_AREA")?.ToString().Trim());
                        row.DEBT = decimal.Parse(readerDbf.GetValue("DEBT")?.ToString().Trim());
                        row.OPP = readerDbf.GetValue("OPP")?.ToString().Trim();
                        row.OPL = readerDbf.GetValue("OPL")?.ToString().Trim();
                        row.ODV = readerDbf.GetValue("ODV")?.ToString().Trim();
                        row.NM_PAY = decimal.Parse(readerDbf.GetValue("NM_PAY")?.ToString().Trim());
                        row.TARYF_1 = float.Parse(readerDbf.GetValue("TARYF_1")?.ToString().Trim());
                        row.TARYF_2 = float.Parse(readerDbf.GetValue("TARYF_2")?.ToString().Trim());
                        row.TARYF_3 = float.Parse(readerDbf.GetValue("TARYF_3")?.ToString().Trim());
                        row.TARYF_4 = float.Parse(readerDbf.GetValue("TARYF_4")?.ToString().Trim());
                        row.TARYF_5 = float.Parse(readerDbf.GetValue("TARYF_5")?.ToString().Trim());
                        row.TARYF_6 = float.Parse(readerDbf.GetValue("TARYF_6")?.ToString().Trim());
                        row.TARYF_7 = float.Parse(readerDbf.GetValue("TARYF_7")?.ToString().Trim());
                        row.TARYF_8 = float.Parse(readerDbf.GetValue("TARYF_8")?.ToString().Trim());
                        row.NORM_F1 = float.Parse(readerDbf.GetValue("NORM_F1")?.ToString().Trim());
                        row.NORM_F2 = float.Parse(readerDbf.GetValue("NORM_F2")?.ToString().Trim());
                        row.NORM_F3 = float.Parse(readerDbf.GetValue("NORM_F3")?.ToString().Trim());
                        row.NORM_F4 = float.Parse(readerDbf.GetValue("NORM_F4")?.ToString().Trim());
                        row.NORM_F5 = float.Parse(readerDbf.GetValue("NORM_F5")?.ToString().Trim());
                        row.NORM_F6 = float.Parse(readerDbf.GetValue("NORM_F6")?.ToString().Trim());
                        row.NORM_F7 = float.Parse(readerDbf.GetValue("NORM_F7")?.ToString().Trim());
                        row.NORM_F8 = float.Parse(readerDbf.GetValue("NORM_F8")?.ToString().Trim());
                        row.OZN_1 = readerDbf.GetValue("OZN_1")?.ToString().Trim();
                        obminSubs.Add(row);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        ViewBag.error = "BadFile";
                        return View();
                    }
                }
            }

            Dictionary<long, SubsRezults> zapyt = new Dictionary<long, SubsRezults>();
            string FileScript = "obmin_subs.sql";
            string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
            DataTable dt = BillingUtils.GetSubsData(path, cokCode, obminSubs);

            foreach (DataRow dtRow in dt.Rows)
            {
                if (!zapyt.ContainsKey(dtRow.Field<long>("AccountNumberNew")))
                {
                    zapyt.Add(dtRow.Field<long>("AccountNumberNew"), new SubsRezults
                    {
                        DEBT = dtRow.Field<decimal>("debt"),
                        NM_PAY = dtRow.Field<decimal>("nm_pay"),
                        NORM_F6 = dtRow.Field<int>("norm_f6"),
                        TARYF_6 = dtRow.Field<decimal>("taryf_6")
                    });
                }
                //Console.WriteLine(dtRow.Field<decimal>("debt"));
                //Console.WriteLine(dtRow.Field<long>("AccountNumberNew").ToString().Trim());
            }

            using (Stream fos = System.IO.File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var writer = new DBFWriter())
                {
                    writer.CharEncoding = Encoding.GetEncoding(866);
                    writer.Signature = DBFSignature.DBase3;
                    writer.LanguageDriver = 0x26; // кодировка 866
                    var APP_NUM = new DBFField("APP_NUM", NativeDbType.Char, 6);
                    var ZAP_R = new DBFField("ZAP_R", NativeDbType.Numeric, 4);
                    var ZAP_N = new DBFField("ZAP_N", NativeDbType.Numeric, 4);
                    var OWN_NUM = new DBFField("OWN_NUM", NativeDbType.Char, 15);
                    var SUR_NAM = new DBFField("SUR_NAM", NativeDbType.Char, 30);
                    var F_NAM = new DBFField("F_NAM", NativeDbType.Char, 20);
                    var M_NAM = new DBFField("M_NAM", NativeDbType.Char, 20);
                    var ENT_COD = new DBFField("ENT_COD", NativeDbType.Char, 10);
                    var DATA_S = new DBFField("DATA_S", NativeDbType.Date);
                    var RES2 = new DBFField("RES2", NativeDbType.Numeric, 4);
                    var ADR_NAM = new DBFField("ADR_NAM", NativeDbType.Char, 30);
                    var VUL_COD = new DBFField("VUL_COD", NativeDbType.Numeric, 4);
                    var VUL_CAT = new DBFField("VUL_CAT", NativeDbType.Char, 8);
                    var VUL_NAM = new DBFField("VUL_NAM", NativeDbType.Char, 30);
                    var BLD_NUM = new DBFField("BLD_NUM", NativeDbType.Char, 7);
                    var CORP_NUM = new DBFField("CORP_NUM", NativeDbType.Char, 2);
                    var FLAT = new DBFField("FLAT", NativeDbType.Char, 9);
                    var NUMB = new DBFField("NUMB", NativeDbType.Numeric, 2);
                    var NUM_P = new DBFField("NUM_P", NativeDbType.Numeric, 2);
                    var PLG_COD = new DBFField("PLG_COD", NativeDbType.Numeric, 3);
                    var PLG_N = new DBFField("PLG_N", NativeDbType.Numeric, 2);
                    var CM_AREA = new DBFField("CM_AREA", NativeDbType.Numeric, 6, 2);
                    var OP_AREA = new DBFField("OP_AREA", NativeDbType.Numeric, 6, 2);
                    var NM_AREA = new DBFField("NM_AREA", NativeDbType.Numeric, 6, 2);
                    var DEBT = new DBFField("DEBT", NativeDbType.Numeric, 9, 2);
                    var OPP = new DBFField("OPP", NativeDbType.Char, 8);
                    var OPL = new DBFField("OPL", NativeDbType.Char, 8);
                    var ODV = new DBFField("ODV", NativeDbType.Char, 8);
                    var NM_PAY = new DBFField("NM_PAY", NativeDbType.Numeric, 9, 2);
                    var TARYF_1 = new DBFField("TARYF_1", NativeDbType.Numeric, 9, 4);
                    var TARYF_2 = new DBFField("TARYF_2", NativeDbType.Numeric, 9, 4);
                    var TARYF_3 = new DBFField("TARYF_3", NativeDbType.Numeric, 9, 4);
                    var TARYF_4 = new DBFField("TARYF_4", NativeDbType.Numeric, 9, 4);
                    var TARYF_5 = new DBFField("TARYF_5", NativeDbType.Numeric, 9, 4);
                    var TARYF_6 = new DBFField("TARYF_6", NativeDbType.Numeric, 9, 4);
                    var TARYF_7 = new DBFField("TARYF_7", NativeDbType.Numeric, 9, 4);
                    var TARYF_8 = new DBFField("TARYF_8", NativeDbType.Numeric, 9, 4);
                    var NORM_F1 = new DBFField("NORM_F1", NativeDbType.Numeric, 9, 4);
                    var NORM_F2 = new DBFField("NORM_F2", NativeDbType.Numeric, 9, 4);
                    var NORM_F3 = new DBFField("NORM_F3", NativeDbType.Numeric, 9, 4);
                    var NORM_F4 = new DBFField("NORM_F4", NativeDbType.Numeric, 9, 4);
                    var NORM_F5 = new DBFField("NORM_F5", NativeDbType.Numeric, 9, 4);
                    var NORM_F6 = new DBFField("NORM_F6", NativeDbType.Numeric, 9, 4);
                    var NORM_F7 = new DBFField("NORM_F7", NativeDbType.Numeric, 9, 4);
                    var NORM_F8 = new DBFField("NORM_F8", NativeDbType.Numeric, 9, 4);
                    var OZN_1 = new DBFField("OZN_1", NativeDbType.Char, 16);

                    writer.Fields = new[] { APP_NUM, ZAP_R, ZAP_N, OWN_NUM, SUR_NAM, F_NAM, M_NAM, ENT_COD, DATA_S, RES2,
                                    ADR_NAM, VUL_COD, VUL_CAT, VUL_NAM, BLD_NUM, CORP_NUM, FLAT, NUMB, NUM_P, PLG_COD,
                                    PLG_N, CM_AREA, OP_AREA, NM_AREA, DEBT, OPP, OPL, ODV, NM_PAY, TARYF_1, TARYF_2, 
                                    TARYF_3, TARYF_4, TARYF_5, TARYF_6, TARYF_7, TARYF_8, NORM_F1,
                                    NORM_F2, NORM_F3, NORM_F4, NORM_F5, NORM_F6, NORM_F7, NORM_F8, OZN_1
                    };

                    foreach (ObminSubs obmins in obminSubs)
                    {
                        //заповнюємо дані по замовчуванні з дбф-ки
                        decimal borg = obmins.DEBT;
                        decimal nm_pay = obmins.NM_PAY;
                        decimal taryf_6 = (decimal)obmins.TARYF_6;
                        int norm_f6 = (int)obmins.NORM_F6;
                        string opp = "00000000";
                        string opl = "00000000";
                        string odv = "00000000";
                        string ozn_1 = "0000000000010000";

                        if (zapyt.ContainsKey(obmins.OWN_NUM))
                        {
                            //заповнюємо дані з скрипта
                            borg = zapyt[obmins.OWN_NUM].DEBT;
                            nm_pay = zapyt[obmins.OWN_NUM].NM_PAY;
                            norm_f6 = zapyt[obmins.OWN_NUM].NORM_F6;
                            taryf_6 = zapyt[obmins.OWN_NUM].TARYF_6;
                            ozn_1 = "0000000000000000";
                            opp = "00100000";
                            opl = "00100000";
                            odv = "00400000";
                        }

                        writer.AddRecord(
                        obmins.APP_NUM, obmins.ZAP_R, obmins.ZAP_N, obmins.OWN_NUM.ToString(), obmins.SUR_NAM, obmins.F_NAM, obmins.M_NAM, obmins.ENT_COD,
                        obmins.DATA_S, obmins.RES2, obmins.ADR_NAM, obmins.VUL_COD, obmins.VUL_CAT, obmins.VUL_NAM, obmins.BLD_NUM,
                        obmins.CORP_NUM, obmins.FLAT, obmins.NUMB, obmins.NUM_P, obmins.PLG_COD, obmins.PLG_N, obmins.CM_AREA, obmins.OP_AREA,
                        obmins.NM_AREA, borg, opp, opl, odv, nm_pay, obmins.TARYF_1, obmins.TARYF_2, obmins.TARYF_3,
                        obmins.TARYF_4, obmins.TARYF_5, taryf_6, obmins.TARYF_7, obmins.TARYF_8, obmins.NORM_F1, obmins.NORM_F2,
                        obmins.NORM_F3, obmins.NORM_F4, obmins.NORM_F5, norm_f6, obmins.NORM_F7, obmins.NORM_F8, ozn_1
                        );
                    }
                    writer.Write(fos);
                }
            }
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, formFile.FileName);
        }
        //----------------------------------------------------
        //****Формування звіту "Пільга -2"****
        public ActionResult Pilga2(string period)
        {
            _2Pilga viewModel = new _2Pilga();
            if (period == null)
            {
                return View(viewModel);
            }

            viewModel.Period = period;

            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            string cokCode = user.Cok.Code;

            //робимо перевірку на код цоку
            if (cokCode == null || user.AnyCok == true)
            {
                ViewBag.error = "BadCok";
                return View("/Views/Home/Privacy.cshtml");
            }

            //запускаємо скрипт і отримуємо результат 
            string FileScript = "Pilga_Naseleni.sql";
            string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
            DataTable dt = BillingUtils.GetPilgaCity(path, cokCode);

            //вказуємо шлях до DBF файла
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string filePath = "\\Files\\Obmin\\" + Period.per_now().per_str + "\\" + cokCode + "\\";
            string fileName = cokCode + "ilg.dbf";
            string FullPath = appEnv.WebRootPath + filePath + fileName;

            //видаляємо директорію
            if (Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.Delete(appEnv.WebRootPath + filePath, true);

            //створюємо директорію
            if (!Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.CreateDirectory(appEnv.WebRootPath + filePath);

            foreach (DataRow dr in dt.Rows)
            {
                int curNas;
                if (!dr.IsNull("KodNasPunktu") && !dr.IsNull("NasPunkt") && int.TryParse(dr["KodNasPunktu"].ToString(), out curNas))
                {
                    viewModel.nasPunkts[curNas] = dr["NasPunkt"].ToString();
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Pilga2(int[] ids, string exportType, string period)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            string cokCode = user.Cok.Code;
            Console.WriteLine(period);
            List<int> listNumber = new List<int>();     //записуємо отримані коди вибраних населених пунктів

            foreach (int i in ids)
            {
                listNumber.Add(i);
            }

            //запускаємо скрипт і отримуємо результат 
            string FileScript = "Pilga2.sql";
            string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
            DataTable dt = BillingUtils.GetPilga2(path, cokCode, int.Parse(period.Replace("-", "")), listNumber);

            //Створюємо список з категоріями пільг
            List<Pilga2> pilga2s = new List<Pilga2>();

            //виводимо на екран
            if (exportType == "screen")
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Pilga2 pilga = new Pilga2();
                    pilga.CDPR = long.Parse(dr["CDPR"].ToString());
                    pilga.IDCODE = dr["IDCODE"].ToString();
                    pilga.FIO = dr["FIO"].ToString();
                    pilga.PPOS = dr["PPOS"].ToString();
                    pilga.RS = dr["RS"].ToString();
                    pilga.YEARIN = int.Parse(dr["YEARIN"].ToString());
                    pilga.MONTHIN = int.Parse(dr["MONTHIN"].ToString());
                    pilga.LGCODE = int.Parse(dr["LGCODE"].ToString());
                    pilga.DATA1 = DateTime.Parse(dr["DATA1"].ToString());
                    pilga.DATA2 = DateTime.Parse(dr["DATA2"].ToString());
                    pilga.LGKOL = int.Parse(dr["LGKOL"].ToString());
                    pilga.LGKAT = int.Parse(dr["LGKAT"].ToString());
                    pilga.LGPRC = int.Parse(dr["LGPRC"].ToString());
                    pilga.SUMM = decimal.Parse(dr["SUMM"].ToString());
                    pilga.FACT = decimal.Parse(dr["FACT"].ToString());
                    pilga.TARIF = decimal.Parse(dr["TARIF"].ToString());
                    pilga.FLAG = int.Parse(dr["FLAG"].ToString());
                    pilga.isBlock = int.Parse(dr["isBlock"].ToString());
                    pilga.idNasPunkt = int.Parse(dr["KodNasPunktu"].ToString());
                    pilga.NasPunkt = dr["NasPunkt"].ToString();
                    pilga2s.Add(pilga);
                }

                return Json(pilga2s);
            }

            //виводимо у дбф
            if (exportType == "dbf")
            {
                string filePath = "\\Files\\Obmin\\" + Period.per_now().per_str + "\\" + cokCode + "\\";
                string FileName = "ilg_" + user.Id.ToString() + "__";
                string fullPath = appEnv.WebRootPath + filePath + FileName;

                //якщо є файл то видаляємо його
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                //створюємо новий дбф файл згідно заданої нами структури
                using (Stream fos = System.IO.File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using DBFWriter writer = new DBFWriter
                    {
                        CharEncoding = Encoding.GetEncoding(866),
                        Signature = DBFSignature.DBase3
                    };

                    //структура файлу дбф
                    var CDPR = new DBFField("CDPR", NativeDbType.Numeric, 12);
                    var IDCODE = new DBFField("IDCODE", NativeDbType.Char, 10);
                    var FIO = new DBFField("FIO", NativeDbType.Char, 50);
                    var PPOS = new DBFField("PPOS", NativeDbType.Char, 15);
                    var RS = new DBFField("RS", NativeDbType.Char, 25);
                    var YEARIN = new DBFField("YEARIN", NativeDbType.Numeric, 4);
                    var MONTHIN = new DBFField("MONTHIN", NativeDbType.Numeric, 2);
                    var LGCODE = new DBFField("LGCODE", NativeDbType.Numeric, 4);
                    var DATA1 = new DBFField("DATA1", NativeDbType.Date);
                    var DATA2 = new DBFField("DATA2", NativeDbType.Date);
                    var LGKOL = new DBFField("LGKOL", NativeDbType.Numeric, 2);
                    var LGKAT = new DBFField("LGKAT", NativeDbType.Numeric, 3);
                    var LGPRC = new DBFField("LGPRC", NativeDbType.Numeric, 3);
                    var SUMM = new DBFField("SUMM", NativeDbType.Numeric, 10, 2);
                    var FACT = new DBFField("FACT", NativeDbType.Numeric, 20, 6);
                    var TARIF = new DBFField("TARIF", NativeDbType.Numeric, 16, 7);
                    var FLAG = new DBFField("FLAG", NativeDbType.Numeric, 1);

                    writer.Fields = new[] { CDPR, IDCODE, FIO, PPOS, RS, YEARIN, MONTHIN, LGCODE, DATA1, DATA2, LGKOL,
                                            LGKAT, LGPRC, SUMM, FACT, TARIF, FLAG
                                          };

                    //зберігаємо вибрані дані з скрипта в файл
                    foreach (DataRow dr in dt.Rows)
                    {
                        writer.AddRecord( int.Parse(dr["CDPR"].ToString()), dr["IDCODE"], dr["FIO"], dr["PPOS"], dr["RS"],
                            int.Parse(dr["YEARIN"].ToString()), int.Parse(dr["MONTHIN"].ToString()), int.Parse(dr["LGCODE"].ToString()),
                            dr["DATA1"], dr["DATA2"], int.Parse(dr["LGKOL"].ToString()), int.Parse(dr["LGKAT"].ToString()),
                            int.Parse(dr["LGPRC"].ToString()), dr["SUMM"], dr["FACT"], dr["TARIF"], int.Parse(dr["FLAG"].ToString())
                        );
                    }

                    //записуємо у файл
                    writer.Write(fos);
                }

                //видаємо користувачу файл
                string fileNameNew = FileName + DateTime.Now.ToString() + ".dbf";
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileNameNew);
            }

            //виводимо на друк
            if (exportType == "excel")
            {
                //запускаємо скрипт і отримуємо результат 
                string FileScript1 = "Pilga2Excel.sql";
                string path1 = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript1;
                DataTable dt1 = BillingUtils.GetPilga2(path1, cokCode, int.Parse(period.Replace("-", "")), listNumber);

                foreach (DataRow dr in dt1.Rows)
                {
                    Pilga2 pilga = new Pilga2();
                    //pilga.CDPR = long.Parse(dr["CDPR"].ToString());
                    pilga.IDCODE = dr["IDCODE"].ToString();
                    pilga.FIO = dr["FIO"].ToString();
                    //pilga.PPOS = dr["PPOS"].ToString();
                    pilga.RS = dr["RS"].ToString();
                    pilga.YEARIN = int.Parse(dr["YEARIN"].ToString());
                    pilga.MONTHIN = int.Parse(dr["MONTHIN"].ToString());
                    //pilga.LGCODE = int.Parse(dr["LGCODE"].ToString());
                    pilga.DATA1 = DateTime.Parse(dr["DATA1"].ToString());
                    pilga.DATA2 = DateTime.Parse(dr["DATA2"].ToString());
                    pilga.LGKOL = int.Parse(dr["LGKOL"].ToString());
                    pilga.LGKAT = int.Parse(dr["LGKAT"].ToString());
                    pilga.LGNAME = dr["LGNAME"].ToString();
                    pilga.LGPRC = int.Parse(dr["LGPRC"].ToString());
                    //pilga.SUMM = decimal.Parse(dr["SUMM"].ToString());
                    //pilga.FACT = decimal.Parse(dr["FACT"].ToString());
                    //pilga.TARIF = decimal.Parse(dr["TARIF"].ToString());
                    //pilga.FLAG = int.Parse(dr["FLAG"].ToString());
                    //pilga.isBlock = int.Parse(dr["isBlock"].ToString());
                    //pilga.idNasPunkt = int.Parse(dr["KodNasPunktu"].ToString());
                    pilga.NasPunkt = dr["NasPunkt"].ToString();
                    pilga.VulName = dr["VulName"].ToString();
                    pilga.Bild = dr["Bild"].ToString();
                    pilga.Korp = dr["Korp"].ToString();
                    pilga.Apartment = dr["Apartment"].ToString();
                    pilga.woz = decimal.Parse(dr["woz"].ToString());
                    pilga.z1 = decimal.Parse(dr["z1"].ToString());
                    pilga.z2 = decimal.Parse(dr["z2"].ToString());
                    pilga.z3 = decimal.Parse(dr["z3"].ToString());
                    pilga.z4 = decimal.Parse(dr["z4"].ToString());
                    pilga.wozKwt = decimal.Parse(dr["wozKwt"].ToString());
                    pilga.z1Kwt = decimal.Parse(dr["z1Kwt"].ToString());
                    pilga.z2Kwt = decimal.Parse(dr["z2Kwt"].ToString());
                    pilga.z3Kwt = decimal.Parse(dr["z3Kwt"].ToString());
                    pilga.z4Kwt = decimal.Parse(dr["z4Kwt"].ToString());
                    pilga2s.Add(pilga);
                }
                Dictionary<string, List<Pilga2>> categories = new Dictionary<string, List<Pilga2>>();
                Dictionary<string, CategoryTotals> categoryTotals = new Dictionary<string, CategoryTotals>();
                foreach(Pilga2 p2 in pilga2s)
                {
                    if (!categories.ContainsKey(p2.LGNAME))
                    {
                        categories[p2.LGNAME] = new List<Pilga2>();
                    }
                    if (!categoryTotals.ContainsKey(p2.LGNAME))
                    {
                        categoryTotals[p2.LGNAME] = new CategoryTotals();
                    }
                    categories[p2.LGNAME].Add(p2);
                    categoryTotals[p2.LGNAME].Count += 1;
                    categoryTotals[p2.LGNAME].Code = p2.LGKAT.ToString();
                    
                    categoryTotals[p2.LGNAME].WoZoneCount += p2.woz;
                    categoryTotals[p2.LGNAME].FirstZoneCount += p2.z1;
                    categoryTotals[p2.LGNAME].SecondZoneCount += p2.z2;
                    categoryTotals[p2.LGNAME].ThirdZoneCount += p2.z3;
                    categoryTotals[p2.LGNAME].Lights += p2.z4;
                    categoryTotals[p2.LGNAME].TotalCharged += p2.woz + p2.z1 + p2.z2 + p2.z3 + p2.z4;
                }

                Excel excel = new Excel(categories, categoryTotals);
                excel.CreateZvit(user, period);
                byte[] content = excel.CreateFile();

                return File(
                                content, 
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "pilga2.xlsx"
                            );
            }

            return Redirect("Pilga2");
        }
        //----------------------------------------------------
        //Формування файлу для УСЗН субсидій з новими особовими
        public ActionResult Zapyt(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Zapyt(IFormFile formFile)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            string cokCode = user.Cok.Code;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string filePath = "\\Files\\Obmin\\" + Period.per_now().per_str + "\\" + cokCode + "\\";
            string fullPath = appEnv.WebRootPath + filePath + formFile.FileName + user.Id;

            //видаляємо директорію
            if (Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.Delete(appEnv.WebRootPath + filePath, true);

            //створюємо директорію
            if (!Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.CreateDirectory(appEnv.WebRootPath + filePath);

            //зберігаємо файл
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
                formFile.CopyTo(fileStream);

            List<Zvirka> zv = new List<Zvirka>();

            //Зчитуємо з .dbf і закидаємо в ліст
            using (var dbfDataReader = NDbfReader.Table.Open(fullPath))
            {
                var readerDbf = dbfDataReader.OpenReader(Encoding.GetEncoding(866));
                while (readerDbf.Read())
                {
                    try
                    {
                        var row = new Zvirka();
                        List<NDbfReader.IColumn> columns = readerDbf.Table.Columns.ToList();
                        row.SUR_NAM = readerDbf.GetValue(columns[0])?.ToString().Trim();
                        row.F_NAM = readerDbf.GetValue(columns[1])?.ToString().Trim();
                        row.M_NAM = readerDbf.GetValue(columns[2])?.ToString().Trim();
                        row.INDX = readerDbf.GetValue(columns[3])?.ToString().Trim();
                        row.N_NAME = readerDbf.GetValue(columns[4])?.ToString().Trim();
                        row.N_CODE = readerDbf.GetValue(columns[5])?.ToString().Trim();
                        row.VUL_CAT = readerDbf.GetValue(columns[6])?.ToString().Trim();
                        row.VUL_NAME = readerDbf.GetValue(columns[7])?.ToString().Trim();
                        row.VUL_CODE = readerDbf.GetValue(columns[8])?.ToString().Trim();
                        row.BLD_NUM = readerDbf.GetValue(columns[9])?.ToString().Trim();
                        row.CORP_NUM = readerDbf.GetValue(columns[10])?.ToString().Trim();
                        row.FLAT = readerDbf.GetValue(columns[11])?.ToString().Trim();
                        row.OWN_NUM = readerDbf.GetValue(columns[12])?.ToString().Trim();
                        row.APP_NUM = readerDbf.GetValue(columns[13])?.ToString().Trim();
                        if (readerDbf.GetValue(columns[14]) != null)
                        {
                            row.DAT_BEG = DateTime.Parse(readerDbf.GetValue(columns[14])?.ToString().Trim());
                        }
                        if (readerDbf.GetValue(columns[14]) != null) 
                        { 
                            row.DAT_END = DateTime.Parse(readerDbf.GetValue(columns[15])?.ToString().Trim());
                        }
                        row.CM_AREA = decimal.Parse(readerDbf.GetValue(columns[16])?.ToString().Trim());
                        row.NM_AREA = decimal.Parse(readerDbf.GetValue(columns[17])?.ToString().Trim());
                        row.BLC_AREA = decimal.Parse(readerDbf.GetValue(columns[18])?.ToString().Trim());
                        row.FROG = decimal.Parse(readerDbf.GetValue(columns[19])?.ToString().Trim());
                        row.DEBT = decimal.Parse(readerDbf.GetValue(columns[20])?.ToString().Trim());
                        row.NUMB = int.Parse(readerDbf.GetValue(columns[21])?.ToString().Trim());
                        row.P1 = decimal.Parse(readerDbf.GetValue(columns[22])?.ToString().Trim());
                        row.N1 = decimal.Parse(readerDbf.GetValue(columns[23])?.ToString().Trim());
                        row.P2 = decimal.Parse(readerDbf.GetValue(columns[24])?.ToString().Trim());
                        row.N2 = decimal.Parse(readerDbf.GetValue(columns[25])?.ToString().Trim());
                        row.P3 = decimal.Parse(readerDbf.GetValue(columns[26])?.ToString().Trim());
                        row.N3 = decimal.Parse(readerDbf.GetValue(columns[27])?.ToString().Trim());
                        row.P4 = decimal.Parse(readerDbf.GetValue(columns[28])?.ToString().Trim());
                        row.N4 = decimal.Parse(readerDbf.GetValue(columns[29])?.ToString().Trim());
                        row.P5 = decimal.Parse(readerDbf.GetValue(columns[30])?.ToString().Trim());
                        row.N5 = decimal.Parse(readerDbf.GetValue(columns[31])?.ToString().Trim());
                        row.P6 = decimal.Parse(readerDbf.GetValue(columns[32])?.ToString().Trim());
                        row.N6 = decimal.Parse(readerDbf.GetValue(columns[33])?.ToString().Trim());
                        row.P7 = decimal.Parse(readerDbf.GetValue(columns[34])?.ToString().Trim());
                        row.N7 = decimal.Parse(readerDbf.GetValue(columns[35])?.ToString().Trim());
                        row.P8 = decimal.Parse(readerDbf.GetValue(columns[36])?.ToString().Trim());
                        row.N8 = decimal.Parse(readerDbf.GetValue(columns[37])?.ToString().Trim());
                        zv.Add(row);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        ViewBag.error = "BadFile";
                        return View();
                    }
                }
            }

            Dictionary<string, string> zap = new Dictionary<string, string>();
            string FileScript = "napovnenia.sql";
            string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
            DataTable dt = BillingUtils.Napovnenia(path, cokCode, zv);

            foreach (DataRow dtRow in dt.Rows)
            {
                string acc = dtRow.Field<string>("acc");
                if (acc != null && acc.Trim() != "" && !zap.ContainsKey(acc))
                {
                    zap.Add(dtRow.Field<string>("acc"), dtRow.Field<string>("NewAcc"));
                }
            }

            using (Stream fos = System.IO.File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var writer = new DBFWriter())
                {
                    writer.CharEncoding = Encoding.GetEncoding(866);
                    writer.Signature = DBFSignature.DBase3;
                    writer.LanguageDriver = 0x26; // кодировка 866
                    var sur_nam = new DBFField("sur_nam", NativeDbType.Char, 30);
                    var f_nam = new DBFField("f_nam", NativeDbType.Char, 15);
                    var m_nam = new DBFField("m_nam", NativeDbType.Char, 20);
                    var indx = new DBFField("indx", NativeDbType.Char, 6);
                    var n_name = new DBFField("n_name", NativeDbType.Char, 30);
                    var n_code = new DBFField("n_code", NativeDbType.Char, 5);
                    var vul_cat = new DBFField("vul_cat", NativeDbType.Char, 7);
                    var vul_name = new DBFField("vul_name", NativeDbType.Char, 30);
                    var vul_code = new DBFField("vul_code", NativeDbType.Char, 5);
                    var bld_num = new DBFField("bld_num", NativeDbType.Char, 7);
                    var corp_num = new DBFField("corp_num", NativeDbType.Char, 2);
                    var flat = new DBFField("flat", NativeDbType.Char, 7);
                    var own_num = new DBFField("own_num", NativeDbType.Char, 15);
                    var app_num = new DBFField("app_num", NativeDbType.Char, 8);
                    var dat_beg = new DBFField("dat_beg", NativeDbType.Date);
                    var dat_end = new DBFField("dat_end", NativeDbType.Date);
                    var cm_area = new DBFField("cm_area", NativeDbType.Numeric, 7, 2);
                    var nm_area = new DBFField("nm_area", NativeDbType.Numeric, 7, 2);
                    var blc_area = new DBFField("blc_area", NativeDbType.Numeric, 5, 2);
                    var frog = new DBFField("frog", NativeDbType.Numeric, 5, 1);
                    var debt = new DBFField("debt", NativeDbType.Numeric, 10, 2);
                    var numb = new DBFField("numb", NativeDbType.Numeric, 2);
                    var p1 = new DBFField("p1", NativeDbType.Numeric, 10, 4);
                    var n1 = new DBFField("n1", NativeDbType.Numeric, 10, 4);
                    var p2 = new DBFField("p2", NativeDbType.Numeric, 10, 4);
                    var n2 = new DBFField("n2", NativeDbType.Numeric, 10, 4);
                    var p3 = new DBFField("p3", NativeDbType.Numeric, 10, 4);
                    var n3 = new DBFField("n3", NativeDbType.Numeric, 10, 4);
                    var p4 = new DBFField("p4", NativeDbType.Numeric, 10, 4);
                    var n4 = new DBFField("n4", NativeDbType.Numeric, 10, 4);
                    var p5 = new DBFField("p5", NativeDbType.Numeric, 10, 4);
                    var n5 = new DBFField("n5", NativeDbType.Numeric, 10, 4);
                    var p6 = new DBFField("p6", NativeDbType.Numeric, 10, 4);
                    var n6 = new DBFField("n6", NativeDbType.Numeric, 10, 4);
                    var p7 = new DBFField("p7", NativeDbType.Numeric, 10, 4);
                    var n7 = new DBFField("n7", NativeDbType.Numeric, 10, 4);
                    var p8 = new DBFField("p8", NativeDbType.Numeric, 10, 4);
                    var n8 = new DBFField("n8", NativeDbType.Numeric, 10, 4);

                    writer.Fields = new[] { sur_nam, f_nam, m_nam, indx, n_name, n_code, vul_cat, vul_name, vul_code,
                        bld_num, corp_num, flat, own_num, app_num, dat_beg, dat_end, cm_area, nm_area, blc_area, frog,
                        debt, numb, p1, n1, p2, n2, p3, n3, p4, n4, p5, n5, p6, n6, p7, n7, p8, n8
                    };

                    foreach (Zvirka zvirka in zv)
                    {
                        //заповнюємо дані по замовчуванні з дбф-ки
                        string accNum = zvirka.OWN_NUM;
                        if (zvirka.OWN_NUM != null && zap.ContainsKey(zvirka.OWN_NUM))
                        {
                            //заповнюємо дані з скрипта
                            accNum = zap[zvirka.OWN_NUM];
                        }
                        else
                        {
                            accNum = "";
                        }

                        writer.AddRecord(zvirka.SUR_NAM, zvirka.F_NAM, zvirka.M_NAM, zvirka.INDX, zvirka.N_NAME,
                            zvirka.N_CODE, zvirka.VUL_CAT, zvirka.VUL_NAME, zvirka.VUL_CODE, zvirka.BLD_NUM,
                            zvirka.CORP_NUM, zvirka.FLAT, accNum, zvirka.APP_NUM, zvirka.DAT_BEG, zvirka.DAT_END,
                            zvirka.CM_AREA, zvirka.NM_AREA, zvirka.BLC_AREA, zvirka.FROG, zvirka.DEBT, zvirka.NUMB,
                            zvirka.P1, zvirka.N1, zvirka.P2, zvirka.N2, zvirka.P3, zvirka.N3, zvirka.P4, zvirka.N4,
                            zvirka.P5, zvirka.N5, zvirka.P6, zvirka.N6, zvirka.P7, zvirka.N7, zvirka.P8, zvirka.N8
                            );
                    }

                    writer.Write(fos);
                }
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, formFile.FileName);
            //return View();
        }
        //---------------------------------------------------
        //Формування файлу для УСЗН пільг з новими особовими
        public ActionResult ZapytP(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult ZapytP(IFormFile formFile)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            string cokCode = user.Cok.Code;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string filePath = "\\Files\\Obmin\\" + Period.per_now().per_str + "\\" + cokCode + "\\";
            string fullPath = appEnv.WebRootPath + filePath + formFile.FileName + user.Id;

            //видаляємо директорію
            if (Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.Delete(appEnv.WebRootPath + filePath, true);

            //створюємо директорію
            if (!Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.CreateDirectory(appEnv.WebRootPath + filePath);

            //зберігаємо файл
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
                formFile.CopyTo(fileStream);

            List<ZvirkaOsPilg> zv = new List<ZvirkaOsPilg>();

            //Зчитуємо з .dbf і закидаємо в ліст
            using (var dbfDataReader = NDbfReader.Table.Open(fullPath))
            {
                var readerDbf = dbfDataReader.OpenReader(Encoding.GetEncoding(866));
                while (readerDbf.Read())
                {
                    try
                    {
                        var row = new ZvirkaOsPilg();
                        List<NDbfReader.IColumn> columns = readerDbf.Table.Columns.ToList();
                        row.COD = int.Parse(readerDbf.GetValue(columns[0])?.ToString().Trim());
                        row.CDPR = long.Parse(readerDbf.GetValue(columns[1])?.ToString().Trim());
                        row.NCARD = long.Parse(readerDbf.GetValue(columns[2])?.ToString().Trim());
                        row.IDCODE = readerDbf.GetValue(columns[3])?.ToString().Trim();
                        row.PASP = readerDbf.GetValue(columns[4])?.ToString().Trim();
                        row.FIO = readerDbf.GetValue(columns[5])?.ToString().Trim();
                        row.IDPIL = readerDbf.GetValue(columns[6])?.ToString().Trim();
                        row.PASPPIL = readerDbf.GetValue(columns[7])?.ToString().Trim();
                        row.FIOPIL = readerDbf.GetValue(columns[8])?.ToString().Trim();
                        row.INDEX = int.Parse(readerDbf.GetValue(columns[9])?.ToString().Trim());
                        row.CDUL = int.Parse(readerDbf.GetValue(columns[10])?.ToString().Trim());
                        row.HOUSE = readerDbf.GetValue(columns[11])?.ToString().Trim();
                        row.BUILD = readerDbf.GetValue(columns[12])?.ToString().Trim();
                        row.APT = readerDbf.GetValue(columns[13])?.ToString().Trim();
                        row.LGCODE = int.Parse(readerDbf.GetValue(columns[14])?.ToString().Trim());
                        row.KAT = int.Parse(readerDbf.GetValue(columns[15])?.ToString().Trim());
                        row.YEARIN = int.Parse(readerDbf.GetValue(columns[16])?.ToString().Trim());
                        row.MONTHIN = int.Parse(readerDbf.GetValue(columns[17])?.ToString().Trim());
                        row.YEAROUT = int.Parse(readerDbf.GetValue(columns[18])?.ToString().Trim());
                        row.MONTHOUT = int.Parse(readerDbf.GetValue(columns[19])?.ToString().Trim());
                        row.RAH = readerDbf.GetValue(columns[20])?.ToString().Trim();
                        row.RIZN = int.Parse(readerDbf.GetValue(columns[21])?.ToString().Trim());
                        row.TARIF = long.Parse(readerDbf.GetValue(columns[22])?.ToString().Trim());
                        zv.Add(row);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        ViewBag.error = "BadFile";
                        return View();
                    }
                }
            }

            Dictionary<string, string> zap = new Dictionary<string, string>();
            string FileScript = "napovnenia.sql";
            string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
            DataTable dt = BillingUtils.PilgNewAcc(path, cokCode, zv);

            foreach (DataRow dtRow in dt.Rows)
            {
                string acc = dtRow.Field<string>("acc");
                if (acc != null && acc.Trim() != "" && !zap.ContainsKey(acc))
                {
                    zap.Add(dtRow.Field<string>("acc"), dtRow.Field<string>("NewAcc"));
                }
            }

            using (Stream fos = System.IO.File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var writer = new DBFWriter())
                {
                    writer.CharEncoding = Encoding.GetEncoding(866);
                    writer.Signature = DBFSignature.DBase3;
                    writer.LanguageDriver = 0x26; // кодировка 866
                    var cod = new DBFField("cod", NativeDbType.Numeric, 4);
                    var cdpr = new DBFField("cdpr", NativeDbType.Numeric, 12);
                    var ncard = new DBFField("ncard", NativeDbType.Numeric, 10);
                    var idcode = new DBFField("idcode", NativeDbType.Char, 10);
                    var pasp = new DBFField("pasp", NativeDbType.Char, 14);
                    var fio = new DBFField("fio", NativeDbType.Char, 50);
                    var idpil = new DBFField("idpil", NativeDbType.Char, 10);
                    var pasppil = new DBFField("pasppil", NativeDbType.Char, 14);
                    var fiopil = new DBFField("fiopil", NativeDbType.Char, 50);
                    var index = new DBFField("index", NativeDbType.Numeric, 6);
                    var cdul = new DBFField("cdul", NativeDbType.Numeric, 5);
                    var house = new DBFField("house", NativeDbType.Char, 7);
                    var build = new DBFField("build", NativeDbType.Char, 2);
                    var apt = new DBFField("apt", NativeDbType.Char, 4);
                    var lgcode = new DBFField("lgcode", NativeDbType.Numeric, 4);
                    var kat = new DBFField("kat", NativeDbType.Numeric, 4);
                    var yearin = new DBFField("yearin", NativeDbType.Numeric, 4);
                    var monthin = new DBFField("monthin", NativeDbType.Numeric, 2);
                    var yearout = new DBFField("yearout", NativeDbType.Numeric, 4);
                    var monthout = new DBFField("monthout", NativeDbType.Numeric, 2);
                    var rah = new DBFField("rah", NativeDbType.Char, 25);
                    var rizn = new DBFField("rizn", NativeDbType.Numeric, 6);
                    var tarif = new DBFField("tarif", NativeDbType.Numeric, 10);

                    writer.Fields = new[] { cod, cdpr, ncard, idcode, pasp, fio, idpil, pasppil, fiopil,
                        index, cdul, house, build, apt, lgcode, kat, yearin, monthin, yearout, monthout,
                        rah, rizn, tarif
                    };

                    foreach (ZvirkaOsPilg zvirka in zv)
                    {
                        //заповнюємо дані по замовчуванні з дбф-ки
                        string accNum = zvirka.RAH;
                        if (zvirka.RAH != null && zap.ContainsKey(zvirka.RAH))
                        {
                            //заповнюємо дані з скрипта
                            accNum = zap[zvirka.RAH];
                        }
                        else
                        {
                            accNum = "";
                        }

                        writer.AddRecord(zvirka.COD, zvirka.CDPR, zvirka.NCARD, zvirka.IDCODE, zvirka.PASP,
                            zvirka.FIO, zvirka.IDPIL, zvirka.PASPPIL, zvirka.FIOPIL, zvirka.INDEX,
                            zvirka.CDUL, zvirka.HOUSE, zvirka.BUILD, zvirka.APT, zvirka.LGCODE,
                            zvirka.KAT, zvirka.YEARIN, zvirka.MONTHIN, zvirka.YEAROUT, zvirka.MONTHOUT, accNum,
                            zvirka.RIZN, zvirka.TARIF
                            );
                    }

                    writer.Write(fos);
                }
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, formFile.FileName);
            //return View();
        }
        //---------------------------------------------------

        //Надання інформації по абонентам в УСЗН ТРДА субсидій.
        public ActionResult Subsydia(int id)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            string cokCode = user.Cok.Code;

            //робимо перевірку на код цоку
            if (user.Cok.Code == null)
            {
                cokCode = "TR40";
            }

            if (cokCode == "TR40")
            {
                //запускаємо скрипт і отримуємо результат 
                string FileScript = "Собезу на субсидії.sql";
                string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
                DataTable dt = BillingUtils.GetResults(path, cokCode);

                //зчитуємо субсидійні номера
                Dictionary<int, string> Sub_e = new Dictionary<int, string>();
                string PathRead = appEnv.WebRootPath + "\\files\\Obmin\\SUB_E.DBF";

                using (var dbfDataReader = NDbfReader.Table.Open(PathRead))
                {
                    var reader = dbfDataReader.OpenReader(Encoding.GetEncoding(866));
                    while (reader.Read())
                    {
                        Sub_e.Add(int.Parse(reader.GetValue("NUMBPERS").ToString().Trim()),
                            int.Parse(reader.GetValue("PCODE").ToString().Substring(4)).ToString().Trim());    //обрізаємо спереді 2099
                    }
                }

                //вказуємо шлях до файла
                string filePath = "\\files\\Obmin\\";
                string fileName = "TEP";
                string FullPath = appEnv.WebRootPath + filePath + fileName;

                //видаляємо файл
                if (System.IO.File.Exists(FullPath))
                {
                    System.IO.File.Delete(FullPath);
                }

                //створюємо новий дбф файл згідно заданої нами структури
                using (Stream fos = System.IO.File.Open(FullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var writer = new DBFWriter())
                    {
                        writer.CharEncoding = Encoding.GetEncoding(866);
                        writer.Signature = DBFSignature.DBase3;

                        //структура файлу дбф
                        var Rash = new DBFField("Rash", NativeDbType.Numeric, 10);
                        var Ora = new DBFField("Ora", NativeDbType.Char, 40);
                        var Fio = new DBFField("Fio", NativeDbType.Char, 100);
                        var Name_v = new DBFField("Name_v", NativeDbType.Char, 50);
                        var Bld = new DBFField("Bld", NativeDbType.Char, 9);
                        var Corp = new DBFField("Corp", NativeDbType.Char, 10);
                        var Flat = new DBFField("Flat", NativeDbType.Char, 5);
                        var Nazva = new DBFField("Nazva", NativeDbType.Char, 40);
                        var Tariff = new DBFField("Tariff", NativeDbType.Char, 40);
                        var Discount = new DBFField("Discount", NativeDbType.Numeric, 20, 5);
                        var Pilgovuk = new DBFField("Pilgovuk", NativeDbType.Char, 100);
                        var Gar_voda = new DBFField("Gar_voda", NativeDbType.Numeric, 20, 5);
                        var Gaz_vn = new DBFField("Gaz_vn", NativeDbType.Numeric, 20, 5);
                        var El_opal = new DBFField("El_opal", NativeDbType.Numeric, 20, 5);
                        var Kilk_pilg = new DBFField("Kilk_pilg", NativeDbType.Char, 2);
                        var T11_cod_na = new DBFField("T11_cod_na", NativeDbType.Char, 40);
                        var Orendar = new DBFField("Orendar", NativeDbType.Char, 40);
                        var Borg = new DBFField("Borg", NativeDbType.Numeric, 20, 5);

                        writer.Fields = new[] { Rash, Ora, Fio, Name_v, Bld, Corp, Flat, Nazva, Tariff, Discount,
                                                Pilgovuk, Gar_voda, Gaz_vn, El_opal, Kilk_pilg, T11_cod_na, Orendar, Borg };

                        //наповнюємо файл даними
                        foreach (DataRow dr in dt.Rows)
                        {
                            string pcode;
                            if (!Sub_e.TryGetValue(dr.Field<int>("Rash"), out pcode))
                            {
                                pcode = "0";
                            }

                            writer.AddRecord(
                                dr.Field<int>(("Rash")),
                                pcode,
                                dr.Field<string>("Fio"),
                                dr.Field<string>("Name_v"),
                                dr.Field<string>("Bld"),
                                dr.Field<string>("Corp"),
                                dr.Field<string>("Flat"),
                                dr.Field<string>("Nazva"),
                                dr.Field<string>("Tariff"),
                                dr.Field<decimal>("Discount"),
                                dr.Field<string>("Pilgovuk"),
                                dr.Field<decimal>("Gar_voda"),
                                dr.Field<decimal>("Gaz_vn"),
                                dr.Field<decimal>("El_opal"),
                                dr.Field<string>("Kilk_pilg"),
                                dr.Field<string>("T11_cod_na"),
                                dr.Field<string>("Orendar"),
                                dr.Field<decimal>("Borg")
                            );
                        }
                        //записуємо у файл
                        writer.Write(fos);
                    }
                }

                //видаємо користувачу файл
                string fileNameNew = fileName + "_" + DateTime.Now.ToString() + ".dbf";
                byte[] fileBytes = System.IO.File.ReadAllBytes(FullPath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileNameNew);
            }
            else if (cokCode == "TR33")
            {
                //запускаємо скрипт і отримуємо результат 
                string FileScript = "Собезу на субсидії Зборів.sql";
                string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
                DataTable dt = BillingUtils.GetResults(path, cokCode);

                //зчитуємо субсидійні номера
                Dictionary<long, string> Sub_e = new Dictionary<long, string>();
                string PathRead = appEnv.WebRootPath + "\\files\\Obmin\\SUB.DBF";

                using (var dbfDataReader = NDbfReader.Table.Open(PathRead))
                {
                    var reader = dbfDataReader.OpenReader(Encoding.GetEncoding(866));
                    while (reader.Read())
                    {
                        Sub_e.Add(int.Parse(reader.GetValue("NUMBPERS").ToString().Trim()),
                            long.Parse(reader.GetValue("PCODE").ToString().Substring(4)).ToString().Trim());    //обрізаємо спереді 2099
                    }
                }

                //вказуємо шлях до файла
                string filePath = "\\files\\Obmin\\";
                string fileName = "TEP";
                string FullPath = appEnv.WebRootPath + filePath + fileName;
                
                //видаляємо файл
                if (System.IO.File.Exists(FullPath))
                {
                    System.IO.File.Delete(FullPath);
                }

                //створюємо новий дбф файл згідно заданої нами структури
                using (Stream fos = System.IO.File.Open(FullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var writer = new DBFWriter())
                    {
                        writer.CharEncoding = Encoding.GetEncoding(866);
                        writer.Signature = DBFSignature.DBase3;

                        //структура файлу дбф
                        var Rash = new DBFField("Rash", NativeDbType.Numeric, 10);
                        var Ora = new DBFField("Ora", NativeDbType.Char, 40);
                        var Fio = new DBFField("Fio", NativeDbType.Char, 100);
                        var Name_v = new DBFField("Name_v", NativeDbType.Char, 50);
                        var Bld = new DBFField("Bld", NativeDbType.Char, 9);
                        var Corp = new DBFField("Corp", NativeDbType.Char, 10);
                        var Flat = new DBFField("Flat", NativeDbType.Char, 5);
                        var Nazva = new DBFField("Nazva", NativeDbType.Char, 40);
                        var Tariff = new DBFField("Tariff", NativeDbType.Char, 40);
                        var Discount = new DBFField("Discount", NativeDbType.Numeric, 20, 5);
                        var Pilgovuk = new DBFField("Pilgovuk", NativeDbType.Char, 100);
                        var Gar_voda = new DBFField("Gar_voda", NativeDbType.Numeric, 20, 5);
                        var Gaz_vn = new DBFField("Gaz_vn", NativeDbType.Numeric, 20, 5);
                        var El_opal = new DBFField("El_opal", NativeDbType.Numeric, 20, 5);
                        var Kilk_pilg = new DBFField("Kilk_pilg", NativeDbType.Char, 2);
                        var T11_cod_na = new DBFField("T11_cod_na", NativeDbType.Char, 40);
                        var Orendar = new DBFField("Orendar", NativeDbType.Char, 40);
                        var Borg = new DBFField("Borg", NativeDbType.Numeric, 20, 5);

                        writer.Fields = new[] { Rash, Ora, Fio, Name_v, Bld, Corp, Flat, Nazva, Tariff, Discount,
                                            Pilgovuk, Gar_voda, Gaz_vn, El_opal, Kilk_pilg, T11_cod_na, Orendar, Borg };

                        //наповнюємо файл даними
                        foreach (DataRow dr in dt.Rows)
                        {
                            string pcode;
                            if (!Sub_e.TryGetValue(dr.Field<int>("Rash"), out pcode))
                            {
                                pcode = "0";
                            }

                            writer.AddRecord(
                                dr.Field<int>(("Rash")),
                                pcode,
                                dr.Field<string>("Fio"),
                                dr.Field<string>("Name_v"),
                                dr.Field<string>("Bld"),
                                dr.Field<string>("Corp"),
                                dr.Field<string>("Flat"),
                                dr.Field<string>("Nazva"),
                                dr.Field<string>("Tariff"),
                                dr.Field<decimal>("Discount"),
                                dr.Field<string>("Pilgovuk"),
                                dr.Field<decimal>("Gar_voda"),
                                dr.Field<decimal>("Gaz_vn"),
                                dr.Field<decimal>("El_opal"),
                                dr.Field<string>("Kilk_pilg"),
                                dr.Field<string>("T11_cod_na"),
                                dr.Field<string>("Orendar"),
                                dr.Field<decimal>("Borg")
                            );
                        }
                        //записуємо у файл
                        writer.Write(fos);
                    }
                }

                //видаємо користувачу файл
                string fileNameNew = fileName + "_" + DateTime.Now.ToString() + ".dbf";
                byte[] fileBytes = System.IO.File.ReadAllBytes(FullPath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileNameNew);
            }

            return View();
        }
        //---------------------------------------------------
        //Формування файлів на Укрспецінформ
        public ActionResult Ukrspecinform()
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            string cokCode = user.Cok.Code;

            //запускаємо скрипт і отримуємо результат 
            string FileScript = "UkrSpecInform.sql";
            string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
            DataTable dt = BillingUtils.Ukrspecinform(path, cokCode);

            //вказуємо шлях до DBF файла
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string filePath = "\\Files\\Obmin\\" + Period.per_now().per_str + "\\" + cokCode + "\\";
            string fileName = cokCode + "EE.dbf";
            string FullPath = appEnv.WebRootPath + filePath + fileName;

            //видаляємо директорію
            if (Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.Delete(appEnv.WebRootPath + filePath, true);

            //створюємо директорію
            if (!Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.CreateDirectory(appEnv.WebRootPath + filePath);

            //створюємо новий дбф файл згідно заданої нами структури
            using (Stream fos = System.IO.File.Open(FullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var writer = new DBFWriter())
                {
                    writer.CharEncoding = Encoding.GetEncoding(866);
                    writer.Signature = DBFSignature.DBase3;
                    writer.LanguageDriver = 0x26; // кодировка 866

                    //структура файлу дбф
                    var Ptr = new DBFField("PTR", NativeDbType.Numeric, 20, 5);
                    var Numbpers = new DBFField("NUMBPERS", NativeDbType.Numeric, 20, 5);
                    var NewNumber = new DBFField("NEW_NUMBER", NativeDbType.Numeric, 20, 5);
                    var StreetPtr = new DBFField("STREETPTR", NativeDbType.Numeric, 20, 5);
                    var Street = new DBFField("STREET", NativeDbType.Char, 150);
                    var House = new DBFField("HOUSE", NativeDbType.Char, 10);
                    var Apartment = new DBFField("APARTMENT", NativeDbType.Char, 5);
                    var Tank = new DBFField("TANK", NativeDbType.Char, 10);
                    var Family = new DBFField("FAMILY", NativeDbType.Char, 140);
                    var Ldate = new DBFField("LDATE", NativeDbType.Date);
                    var Lcount = new DBFField("LCOUNT", NativeDbType.Numeric, 20, 5);
                    var Billdate = new DBFField("BILLDATE", NativeDbType.Date);
                    var Datestart = new DBFField("DATESTART", NativeDbType.Date);
                    var CStart = new DBFField("C_START", NativeDbType.Char, 20);
                    var Dateons = new DBFField("DATEONS", NativeDbType.Date);
                    var COns = new DBFField("C_ONS", NativeDbType.Char, 20);
                    var Ecount = new DBFField("ECOUNT", NativeDbType.Numeric, 20, 5);
                    var Billsumma = new DBFField("BILLSUMMA", NativeDbType.Numeric, 20, 5);
                    var Subsyd = new DBFField("SUBSYD", NativeDbType.Numeric, 20, 5);
                    var Borgsumma = new DBFField("BORGSUMMA", NativeDbType.Numeric, 20, 5);
                    var Tariff = new DBFField("TARIFF", NativeDbType.Numeric, 20, 5);
                    var Limit = new DBFField("LIMIT", NativeDbType.Numeric, 20, 5);
                    var Discount = new DBFField("DISCOUNT", NativeDbType.Numeric, 20, 5);
                    var Kredyt = new DBFField("KREDYT", NativeDbType.Numeric, 20, 5);
                    var Realsumm = new DBFField("REALSUMM", NativeDbType.Numeric, 20, 5);
                    var UsSubsyd = new DBFField("US_SUBSYD", NativeDbType.Numeric, 20, 5);
                    var DataClose = new DBFField("DATA_CLOSE", NativeDbType.Date);
                    var Lastpaydat = new DBFField("LASTPAYDAT", NativeDbType.Date);
                    var OplataPop = new DBFField("OPLATA_POP", NativeDbType.Numeric, 20, 5);
                    var OplataCur = new DBFField("OPLATA_CUR", NativeDbType.Numeric, 20, 5);
                    var SaldoP = new DBFField("SALDO_P", NativeDbType.Numeric, 20, 5);
                    var DoOplaty = new DBFField("DO_OPLATY", NativeDbType.Numeric, 20, 5);

                    writer.Fields = new[] { Ptr, Numbpers, NewNumber, StreetPtr, Street, House, Apartment, Tank, Family, Ldate,
                        Lcount, Billdate, Datestart, CStart, Dateons, COns, Ecount, Billsumma, Subsyd, Borgsumma, Tariff, Limit,
                        Discount, Kredyt, Realsumm, UsSubsyd, DataClose, Lastpaydat, OplataPop, OplataCur, SaldoP, DoOplaty };

                    //наповнюємо файл даними
                    foreach (DataRow dr in dt.Rows)
                    {
                        int lcount = string.IsNullOrEmpty(dr["lcount"].ToString()) ? 0 : int.Parse(dr["lcount"].ToString());
                        int ecount = string.IsNullOrEmpty(dr["ecount"].ToString()) ? 0 : int.Parse(dr["ecount"].ToString());
                        decimal subsyd = string.IsNullOrEmpty(dr["subsyd"].ToString()) ? 0 : decimal.Parse(dr["subsyd"].ToString());
                        writer.AddRecord(
                            dr.Field<int>("ptr"),
                            dr.Field<long>("numbpers"),
                            dr.Field<long>("NEWnumbpers"),
                            dr.Field<int>("street_ptr"),
                            dr.Field<string>("street"),
                            dr.Field<string?>("house"),
                            dr.Field<string?>("apartment"),
                            dr.Field<string?>("tank"),
                            dr.Field<string?>("family"),
                            dr.Field<DateTime?>("ldate"),
                            lcount,
                            dr.Field<DateTime?>("billdate"),
                            dr.Field<DateTime?>("datestart"),
                            dr.Field<string?>("c_start"),
                            dr.Field<DateTime?>("dateons"),
                            dr.Field<string?>("c_ons"),
                            //dr.Field<int?>("ecount"),
                            ecount,
                            dr.Field<decimal>("billsumma"),
                            //dr.Field<decimal?>("subsyd"),
                            subsyd,
                            dr.Field<decimal>("borgsumma"),
                            dr.Field<decimal>("tariff"),
                            dr.Field<int?>("limit"),
                            dr.Field<decimal?>("discount"),
                            dr.Field<decimal>("kredyt"),
                            dr.Field<decimal?>("realsumm"),
                            dr.Field<decimal?>("us_subsyd"),
                            dr.Field<DateTime?>("data_close"),
                            dr.Field<DateTime?>("lastpaydat"),
                            dr.Field<decimal?>("oplata_pop"),
                            dr.Field<decimal?>("oplata_cur"),
                            dr.Field<decimal?>("saldo_p"),
                            dr.Field<decimal?>("do_oplaty")
                        );
                    }
                    //записуємо у файл
                    writer.Write(fos);
                }
            }
            //видаємо користувачу файл
            byte[] fileBytes = System.IO.File.ReadAllBytes(FullPath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        //---------------------------------------------------
        //Монетизація субсидій та пільг
        public ActionResult MoneySubs(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult MoneySubs(IFormFile formFile)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string filePath = "\\Files\\Obmin\\Money\\" + Period.per_now().per_str + "\\";
            string fullPath = appEnv.WebRootPath + filePath + formFile.FileName;

            //видаляємо директорію
            if (Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.Delete(appEnv.WebRootPath + filePath, true);

            //створюємо директорію
            if (!Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.CreateDirectory(appEnv.WebRootPath + filePath);

            //зберігаємо файл
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
                formFile.CopyTo(fileStream);

            Dictionary<string, MoneySubsydii> zap = new Dictionary<string, MoneySubsydii>();
            string FileScript = "субсидії монетизація по області.sql";
            string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
            DataTable dt = BillingUtils.GetMoneySubsData(path);
            
            foreach (DataRow dtRow in dt.Rows)
            {
                string newRah = dtRow.Field<long>("newRah").ToString();
                string accountCode = dtRow.Field<int>("raj").ToString() + ":" + dtRow.Field<long>("osRah").ToString();
                MoneySubsydii data = new MoneySubsydii
                {
                    Raj = dtRow.Field<int>("raj"),
                    Pip = dtRow.Field<string>("FullName"),
                    OsRah = dtRow.Field<long>("osRah").ToString(),
                    NewRah = dtRow.Field<long>("newRah"),
                    Spogyto = (double)dtRow.Field<decimal>("sumSpogyto"),
                    Borg = (double)dtRow.Field<decimal>("sumBorg"),
                };
                if (!zap.ContainsKey(accountCode))
                {
                    zap.Add(accountCode, data);
                }
                if (!zap.ContainsKey(newRah))
                {
                    zap.Add(newRah, data);
                }
                
            }

            MoneyExcel excel = new MoneyExcel(zap, fullPath);
            excel.ToExcel();
            byte[] content = excel.CreateFile();

            return File(
                            content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            formFile.FileName
                        );

        }
        //---------------------------------------------------
        //Монетизація субсидій та пільг
        public ActionResult Pilg_Subs_server()
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            ViewData["Cok"] = user.Cok.NmeDoc.ToString().Trim();
            return View();
        }

        [HttpPost]
        public ActionResult Pilg_Subs_server(IFormFile formFile)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string filePath = "\\Files\\Obmin\\MoneyToUtility\\" + Period.per_now().per_str + "\\";
            string fullPath = appEnv.WebRootPath + filePath + formFile.FileName;
            //створюємо директорію, якщо не має
            if (!Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.CreateDirectory(appEnv.WebRootPath + filePath);
            //зберігаємо файл
            if (formFile.FileName.ToUpper().StartsWith("LK") || formFile.FileName.ToUpper().StartsWith("RK"))
            {
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    formFile.CopyTo(fileStream);
                }
            }
            else
            {
                ViewBag.error = "BadFile";
            }
            
            return View();
        }

        [HttpPost]
        public ActionResult Pilg_Subs_raj(string file)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            string cok = user.Cok.Code;
            //шукаємо ексель файли
            string dir = "\\Files\\Obmin\\MoneyToUtility\\" + Period.per_now().per_str + "\\";  //директорія зфайлами
            string[] FileName;  //тут буде назва файла
            string fullPath = "";   //тут буде повний шлях до файла
            string fl = "";
            //Робимо перевірку на директорію, якщо є то є і файли
            if (Directory.Exists(appEnv.WebRootPath + dir))
            {
                FileName = Directory.GetFiles(appEnv.WebRootPath + dir);

                for (int i = 0; i < FileName.Length; i++)
                {
                    fl = Path.GetFileName(FileName[i]); // только имя файла с расширением
                    if (file == "subsydija" && fl.ToUpper().StartsWith("RK"))
                    {
                        fullPath = appEnv.WebRootPath + dir + fl;
                    }
                    else if (file == "pilga" && fl.ToUpper().StartsWith("LK"))
                    {
                        fullPath = appEnv.WebRootPath + dir + fl;
                    }
                }
            }
            //робимо перевірку чи є файл
            if (System.IO.File.Exists(fullPath))
            {
                string FileScript = "AccNumber.sql";
                string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
                Dictionary<string, MoneySubsydii> zap = new Dictionary<string, MoneySubsydii>();
                MoneyExcel excel = new MoneyExcel(zap, fullPath);
                decimal zagalSuma = 0;
                int kt = 0;

                //вказуємо шлях до DBF i TXT файла
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                string FilePath = "\\Files\\Obmin\\MoneyToUtility\\" + Period.per_now().per_str + "\\" + cok + "\\";
                string DBFfileName = cok +"_"+ file + ".dbf";
                string TXTfileName = cok +"_"+ file + ".txt";
                string FullPath = appEnv.WebRootPath + FilePath;
                string DBFfullPath = FullPath + DBFfileName;
                string TXTfullPath = FullPath + TXTfileName;
                // ZIP результат
                string FileResultPath = "\\Files\\Obmin\\MoneyToUtility\\" + Period.per_now().per_str + "\\";
                string FullResultPath = appEnv.WebRootPath + FileResultPath;
                string zipfile_name = cok + "_" + file + "_" + Period.per_now().per_str + ".zip";
                string FullZipResult = FullResultPath + zipfile_name;

                //видаляємо директорію
                if (Directory.Exists(FullPath))
                {
                    Directory.Delete(FullPath, true);
                }
                //створюємо директорію
                Directory.CreateDirectory(FullPath);

                //видаляємо ZIP
                if (System.IO.File.Exists(FullZipResult))
                {
                    System.IO.File.Delete(FullZipResult);
                }

                //створюємо новий дбф файл згідно заданої нами структури
                using (Stream fos = System.IO.File.Open(DBFfullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                { 
                    using (var writer = new DBFWriter())
                    {
                        writer.CharEncoding = Encoding.GetEncoding(866);
                        writer.Signature = DBFSignature.DBase3;
                        writer.LanguageDriver = 0x26; // кодировка 866

                        //структура файлу дбф
                        DBFField Os_Rah = new DBFField("Os_Rah", NativeDbType.Char, 20);
                        DBFField PayDate = new DBFField("PayDate", NativeDbType.Date);
                        DBFField TotalSumm = new DBFField("TotalSumm", NativeDbType.Numeric, 10, 2);
                        
                        writer.Fields = new[] { Os_Rah, PayDate, TotalSumm };
                        kt = excel.FromExcel(cok).Count;

                        Dictionary<string, string> search = new Dictionary<string, string>();
                        DataTable dt = BillingUtils.GetAccNumb(path, cok);
                        //Отримуємо по скрипту всі особові номери 
                        foreach (DataRow dtRow in dt.Rows)
                        {
                            try
                            {
                                search.Add(dtRow["AccountNumberNew"].ToString().Trim(),
                                    dtRow["AccountNumber"].ToString().Trim());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }

                        foreach (var s in excel.FromExcel(cok))
                        {
                            //Потрібно всі нові особові перевести на старі
                            if (search.ContainsKey(s.AccNumber.ToString()))
                            {
                                s.OsRah = (search[s.AccNumber.ToString()]);
                            }
                            else
                            {
                                s.OsRah = s.AccNumber.ToString();
                            }

                            //наповнюємо файл даними
                            writer.AddRecord(s.OsRah, DateTime.Now.Date, s.SumaOplaty);
                            zagalSuma += s.SumaOplaty;
                        }
                        //записуємо у файл
                        writer.Write(fos);

                        //створюємо текстовий файл
                        using (StreamWriter sw = new StreamWriter(TXTfullPath, false, System.Text.Encoding.Default))
                        {
                            sw.WriteLine("Кількість абонентів: " + kt);
                            sw.WriteLine("Загальна сума: " + zagalSuma);
                        }
                    }
                }
                //видаємо користувачу файл
                ZipFile.CreateFromDirectory(FullPath, FullZipResult);    //створюємо архів з папки
                byte[] mas = System.IO.File.ReadAllBytes(FullResultPath + zipfile_name);
                return File(mas, System.Net.Mime.MediaTypeNames.Application.Zip, zipfile_name);
            }
            else
            {
                ViewBag.error = "NetuFile";
                return View("Pilg_Subs_server");
            }
        }
       
    }
}


            
