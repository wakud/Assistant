﻿using System;
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

namespace Assistant_TEP.Controllers
{
    public class ObminController : Controller
    {
        public static string UserName { get; }
        private readonly MainContext db;
        private readonly IWebHostEnvironment appEnv;
        public static IConfiguration Configuration;

        public ObminController(MainContext context, IWebHostEnvironment appEnviroment)
        {
            db = context;
            appEnv = appEnviroment;
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

        public ActionResult Pilg()
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
                        row.OWN_NUM = readerDbf.GetValue("OWN_NUM")?.ToString().Trim();
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

            Dictionary<string, SubsRezults> zapyt = new Dictionary<string, SubsRezults>();
            string FileScript = "obmin_subs.sql";
            string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
            DataTable dt = BillingUtils.GetSubsData(path, cokCode, obminSubs);

            foreach (DataRow dtRow in dt.Rows)
            {
                if(!zapyt.ContainsKey(dtRow.Field<string>("AccountNumber")))
                {
                    zapyt.Add(dtRow.Field<string>("AccountNumber"), new SubsRezults
                    {
                        DEBT = dtRow.Field<decimal>("debt"),
                        NM_PAY = dtRow.Field<decimal>("nm_pay"),
                        NORM_F6 = dtRow.Field<int>("norm_f6"),
                        TARYF_6 = dtRow.Field<decimal>("taryf_6")
                    });

                }
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

                    writer.Fields = new[] { APP_NUM, ZAP_R, ZAP_N, OWN_NUM, SUR_NAM, F_NAM, M_NAM, ENT_COD, DATA_S, RES2, ADR_NAM, VUL_COD,
                                    VUL_CAT, VUL_NAM, BLD_NUM, CORP_NUM, FLAT, NUMB, NUM_P, PLG_COD, PLG_N, CM_AREA, OP_AREA, NM_AREA, DEBT,
                                    OPP, OPL, ODV, NM_PAY, TARYF_1, TARYF_2, TARYF_3, TARYF_4, TARYF_5, TARYF_6, TARYF_7, TARYF_8, NORM_F1,
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
                        obmins.APP_NUM, obmins.ZAP_R, obmins.ZAP_N, obmins.OWN_NUM, obmins.SUR_NAM, obmins.F_NAM, obmins.M_NAM, obmins.ENT_COD,
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

        public ActionResult Subs(int id)
        {
            return View();
        }

        public ActionResult Subsydia(int id)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            string cokCode = user.Cok.Code;
            
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

                //робимо перевірку на код цоку
                if (user.Cok.Code == null)
                {
                    cokCode = "TR40";
                }

            //вказуємо шлях до файла
            string filePath = "\\files\\Obmin\\";
            string fileName = "TEP";
            string FullPath = appEnv.WebRootPath + filePath + fileName;

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
        
        public ActionResult Pilga2()
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            string cokCode = user.Cok.Code;

            //робимо перевірку на код цоку
            if (cokCode == null || user.AnyCok == true)
            {
                ViewBag.error = "BadCok";
                return View("/Views/Home/Privacy.cshtml");
            }

            //запускаємо скрипт і отримуємо результат 
            string FileScript = "Pilga2.sql";
            string path = appEnv.WebRootPath + "\\Files\\Scripts\\" + FileScript;
            DataTable dt = BillingUtils.GetPilga2(path, cokCode);

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

            //Створюємо список з категоріями пільг
            List<Pilga2> pilga2s = new List<Pilga2>();
            foreach (DataRow dr in dt.Rows)
            {
                Console.OutputEncoding = Encoding.GetEncoding(1251);
                Console.WriteLine(dr["FIO"].ToString());
            }

            //створюємо новий дбф файл згідно заданої нами структури
            using (Stream fos = System.IO.File.Open(FullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var writer = new DBFWriter())
                {
                    writer.CharEncoding = Encoding.GetEncoding(866);
                    writer.Signature = DBFSignature.DBase3;
                    writer.LanguageDriver = 0x26; // кодировка 866

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
                    var SUMM = new DBFField("SUMM", NativeDbType.Numeric, 8, 2);
                    var FACT = new DBFField("FACT", NativeDbType.Numeric, 19, 6);
                    var TARIF = new DBFField("TARIF", NativeDbType.Numeric, 14, 7);
                    var FLAG = new DBFField("FLAG", NativeDbType.Numeric, 1);

                    writer.Fields = new[] { CDPR, IDCODE, FIO, PPOS, RS, YEARIN, MONTHIN, LGCODE, DATA1, DATA2, LGKOL,
                        LGKAT, LGPRC, SUMM, FACT, TARIF, FLAG
                    };
                }
            }
            return View();
        }
    }
}
