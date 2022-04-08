using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Assistant_TEP.Models;
using Assistant_TEP.MyClasses;
using Assistant_TEP.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using SharpDocx;
using System.Xml.Linq;

namespace Assistant_TEP.Controllers
{
    /// <summary>
    /// контролер CRUD звітів і довідок
    /// </summary>
    public class ReportsController : Controller
    {
        public static string UserName { get; }
        public static string cokCode;
        public static Dictionary<string, DataTable> zvit = new Dictionary<string, DataTable>();
        private readonly MainContext db;
        private readonly IWebHostEnvironment appEnv;

        public ReportsController(MainContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            appEnv = appEnvironment;
        }
        /// <summary>
        /// отримання звітів (вивід на екран)
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult GetReport(int Id)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            Report rep = db.Reports.Include(r => r.ReportParams).ThenInclude(rp => rp.ParamType).FirstOrDefault(p => p.Id == Id);
            List<Organization> organizations = db.Coks.ToList();
            GetReportViewModel ViewModel = new GetReportViewModel
            {
                rep = rep,
                user = user
            };
            ViewData["Organizations"] = new SelectList(organizations, "Code", "Name");
            string cokCode;
            if(user.CokId != null)
            {
                cokCode = user.Cok.Code;
            } 
            else
            {
                cokCode = (string)ViewData["Organizations"];
            }
            List<Utils.ParamSelectData> selectParamsList = new List<Utils.ParamSelectData>();
            foreach (ReportParam rp in rep.ReportParams)
            {
                if (rp.ParamType.TypeC == "select")
                {
                    if (rp.ParamSource.StartsWith("@sql:"))
                    {
                        string script = rp.ParamSource.Substring(5);
                        DataTable selects = new DataTable();
                        BillingUtils.ExecuteRawSql(script, cokCode, selects);
                        List<Utils.SelectParamReport> selectsList = new List<Utils.SelectParamReport>();
                        foreach (DataRow row in selects.Rows)
                        {
                            selectsList.Add(new Utils.SelectParamReport()
                            {
                                Id = row[0].ToString(),
                                Name = row[1].ToString()
                            });
                        }
                        SelectList htmlSelect = new SelectList(selectsList, "Id", "Name");
                        Utils.ParamSelectData selectData = new Utils.ParamSelectData { NameParam = rp.Name, selects = htmlSelect };
                        selectParamsList.Add(selectData);
                    }
                    else if(rp.ParamSource.StartsWith("@json:"))
                    {
                        string strList = rp.ParamSource.Substring(6);
                        List<Utils.SelectParamReport> selectList = JsonConvert.DeserializeObject<List<Utils.SelectParamReport>>(strList);
                        SelectList htmlSelect = new SelectList(selectList, "Id", "Name");
                        Utils.ParamSelectData selectData = new Utils.ParamSelectData { NameParam = rp.Name, selects = htmlSelect, NameDesc = rp.Description };
                        selectParamsList.Add(selectData);
                    }
                }
            }
            ViewData["SingleSelectParams"] = selectParamsList;
            return View(ViewModel);
        }
        /// <summary>
        /// Виконання звіту
        /// </summary>
        /// <param name="id"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult ExecuteReport(int id, IFormCollection form)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            Dictionary<string, string> parametersReplacements = new Dictionary<string, string>();
            if(user.AnyCok || user.IsAdmin == "1")
            {
                cokCode = form["organization"];
            }
            else
            {
                cokCode = user.Cok.Code;
            }
            Report rep = db.Reports
                .Include(r => r.ReportParams)
                    .ThenInclude(r => r.ParamType)
                .Include(r => r.DbType)
                .FirstOrDefault(r => r.Id == id);
            
            foreach (ReportParam param in rep.ReportParams)
            {
                if (param.ParamType.TypeC == "period")
                {
                    parameters[param.Name] = ParamSerializer.serializePeriod(form[param.Name]);
                }
                else if(param.ParamType.TypeC == "list")
                {
                    parametersReplacements[param.Name] = ParamSerializer.serializeList(form[param.Name], ",");
                }
                else
                {
                    parameters[param.Name] = form[param.Name];
                }
            }
            DataTable dt = BillingUtils.GetReportResults(
                appEnv.WebRootPath + "\\Files\\Scripts\\", rep,
                parameters, cokCode, parametersReplacements
            );
            ResultModelView res = new ResultModelView
            {
                ReportId = rep.Id,
                results = dt
            };

            zvit[user.Id.ToString() + "_" + rep.Id] = dt;
            return View(res);
        }
        /// <summary>
        /// Формування довідок, файлів в інші системи
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Export")]
        public IActionResult Export(int id)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            Report rep = db.Reports.Include(r => r.ReportParams).Include(r => r.DbType).FirstOrDefault(r => r.Id == id);
            DataTable dt = zvit[user.Id.ToString() + "_" + rep.Id];

            using (var wb = new XLWorkbook())
            {
                string Dtm = DateTime.Today.ToString("MMMM");
                string Dtr = DateTime.Today.ToString("yyyy");
                var ws = wb.Worksheets.Add("Звіт").SetTabColor(XLColor.Amber);
                //довідка про оплати
                if (rep.Name == "Видача довідки про оплату")
                {
                    string filePath = "\\Files\\Shablons\\";
                    string generatedPath = "dovidka.docx";
                    string fileName = "shablon.docx";
                    string fullPath = appEnv.WebRootPath + filePath + fileName;
                    string fullGenerated = appEnv.WebRootPath + filePath + generatedPath;
                    DateTime now = DateTime.Now;
                    Dovidka dovidka;
                    List<Oplata> oplatas = new List<Oplata>();
                    foreach(DataRow opl in dt.Rows)
                    {
                        oplatas.Add(new Oplata { DateOplaty = ((DateTime)opl[4]).ToString("D"), Suma = opl[3].ToString() });
                    }
                    if (dt.Rows.Count == 0)
                    {
                        dovidka = new Dovidka
                        {
                            Cok = user.Cok.Name,
                            Vykonavets = user.FullName,
                            Nach = user.Cok.Nach,
                            FullName = "",
                            FullAddress = "",
                            Oplats = oplatas
                        };
                    }
                    else
                    {
                        dovidka = new Dovidka
                        {
                            Cok = user.Cok.Name,
                            Vykonavets = user.FullName,
                            Nach = user.Cok.Nach,
                            AccountNumber = dt.Rows[0][0].ToString(),
                            FullName = dt.Rows[0][1].ToString(),
                            FullAddress = dt.Rows[0][2].ToString(),
                            DateFrom = DateTime.Parse(dt.Rows[0][5].ToString()),
                            DateTo = DateTime.Parse(dt.Rows[0][6].ToString()),
                            Oplats = oplatas
                        };
                    }
                    
                    var document = DocumentFactory.Create(fullPath, dovidka);
                    document.Generate(fullGenerated);

                    string NewFileName = "dovidka_" + DateTime.Now.ToString() + ".docx";
                    return File(
                        System.IO.File.ReadAllBytes(fullGenerated),
                        System.Net.Mime.MediaTypeNames.Application.Octet,
                        NewFileName
                    );
                }
                else if (rep.Name == "Видача довідки для призначення субсидій")     //довідка для субсидій
                {
                    string filePath = "\\Files\\Shablons\\";
                    string generatedPath = "PrnDovSubs.docx";
                    string fileName = "DovSubs.docx";
                    string fullPath = appEnv.WebRootPath + filePath + fileName;
                    string fullGenerated = appEnv.WebRootPath + filePath + generatedPath;
                    List<DovidkaSubs> ds = new List<DovidkaSubs>();
                    foreach (DataRow r in dt.Rows)
                    {
                        ds.Add(
                            new DovidkaSubs
                            {
                                Cok = user.Cok.Name,
                                Vykonavets = user.FullName,
                                Nach = user.Cok.Nach,
                                AccountId = int.Parse(r[0].ToString().Trim()),
                                AccountNumber = long.Parse(r[1].ToString().Trim()),
                                AccountNumberNew = long.Parse(r[2].ToString().Trim()),
                                PIP = r[3].ToString().Trim(),
                                FullAddress = r[4].ToString().Trim(),
                                Pip_Pilg = r[5] == null ? "" : r[5].ToString().Trim(),
                                Pilg_category = r[6] == null ? "" : r[6].ToString().Trim(),
                                BeneficiaryQuantity = r[7] == null ? 0 : int.Parse(r[7].ToString().Trim()),
                                TariffGroupId = short.Parse(r[8].ToString().Trim()),
                                DateFrom = DateTime.Parse(r[9].ToString().Trim()),
                                DateTo = DateTime.Parse(r[10].ToString().Trim()),
                                Price = decimal.Parse(r[11].ToString().Trim()),
                                ShortName = r[12].ToString().Trim(),
                                TariffGroupName = r[13].ToString().Trim(),
                                MaxTariffLimit = int.Parse(r[14].ToString().Trim()),
                                Id = byte.Parse(r[15].ToString().Trim()),
                                TimeZone = int.Parse(r[16].ToString().Trim()),
                                IsHeating = int.Parse(r[17].ToString().Trim()),
                                Discount = int.Parse(r[18].ToString().Trim()),
                                DiscountKoeff = decimal.Parse(r[19].ToString().Trim()),
                                PricePDV = decimal.Parse(r[20].ToString().Trim()),
                                GVP = r[21] == null ? "" : r[21].ToString().Trim(),
                                CPGV = r[22] == null ? "" : r[22].ToString().Trim(),
                                MinValue = int.Parse(r[23].ToString().Trim()),
                                MaxValue = int.Parse(r[24].ToString().Trim()),
                                IncrementValue = int.Parse(r[25].ToString().Trim()),
                                Borg = r[26] == null ? "0,00" : r[26].ToString().Trim(),  // тут помилка
                                RegisteredQuantity = int.Parse(r[27].ToString().Trim()),
                                QuantityTo = decimal.Parse(r[28].ToString().Trim()),
                                SanNormaSubsKwt = int.Parse(r[29].ToString().Trim()),
                                SanNormaSubsGrn = decimal.Parse(r[30].ToString().Trim()),
                                QuantityToGrn = decimal.Parse(r[31].ToString().Trim()),
                                nm_pay = decimal.Parse(r[32].ToString().Trim())
                            }
                        );
                    }

                    var document = DocumentFactory.Create(fullPath, ds);
                    document.Generate(fullGenerated);

                    string NewFileName = "dovidka_" + DateTime.Now.ToString() + ".docx";
                    return File(
                        System.IO.File.ReadAllBytes(fullGenerated),
                        System.Net.Mime.MediaTypeNames.Application.Octet,
                        NewFileName
                    );
                }
                else if (rep.Name == "Для смс \"Борг до оплати\"" || rep.Name == "Для смс \"Сума до оплати\"")      //формування файлу для сервісу смс
                {
                    int currentRow = 1;
                    int currentCell = 1;
                    foreach (DataColumn coll in dt.Columns)
                    {
                        ws.Cell(currentRow, currentCell).Value = coll.ColumnName;
                        currentCell++;
                    }
                    currentCell = 1; currentRow++;
                    ws.Cell(currentRow, currentCell).Value = dt.Rows;

                    using (var stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        var content = stream.ToArray();
                        User cok = db.Users.Include(c => c.Cok).FirstOrDefault(c => c.Cok.Code == cokCode);
                        return File(
                            content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            cok.Cok.Name + "_" + DateTime.Now.ToString("d") + ".xlsx");
                    }
                }
                else if (rep.Name == "Звіт по зонах")       //звіт по багатозонних облікових засобів
                {
                    XDocument xdoc = new XDocument();
                    XElement DataSet = new XElement("NewDataSet");
                    XElement subdiv = new XElement("SubdivisionId", cokCode.Substring(2, 2));
                    
                    var CurrentDate = DateTime.Now;
                    DateTime lastDayOfLastMonth = CurrentDate.Date.AddDays(-CurrentDate.Day);
                    XElement per = new XElement("Period", lastDayOfLastMonth.ToString("d"));

                    DataSet.Add(subdiv);
                    DataSet.Add(per);

                    foreach (DataRow r in dt.Rows)
                    {
                        XElement zona = new XElement("Zones");
                        XElement _id = new XElement("id", r[0].ToString().Trim());
                        XElement AccountNumber = new XElement("AccountNumber", r[1].ToString().Trim());
                        XElement PIP = new XElement("PIP", r[2].ToString().Trim());
                        XElement BlockLabel = new XElement("BlockLabel", r[3].ToString().Trim());
                        XElement BlockLabelName = new XElement("BlockLabelName", r[4].ToString().Trim());
                        XElement TariffGroupId = new XElement("TariffGroupId", r[5].ToString().Trim());
                        XElement TimeZonalId = new XElement("TimeZonalId", r[6].ToString().Trim());
                        XElement isHeating = new XElement("isHeating", r[7].ToString().Trim());
                        XElement BasePrice = new XElement("BasePrice", r[8].ToString().Trim().Replace(",", "."));
                        XElement Quantity_Nich = new XElement("Quantity_Nich", r[9].ToString().Trim());
                        XElement Quantity_PivPick = new XElement("Quantity_PivPick", r[10].ToString().Trim());
                        XElement Quantity_Pick = new XElement("Quantity_Pick", r[11].ToString().Trim());
                        XElement Tariff_Nich = new XElement("Tariff_Nich", r[12].ToString().Trim().Replace(",", "."));
                        XElement Tariff_PivPick = new XElement("Tariff_PivPick", r[13].ToString().Trim().Replace(",", "."));
                        XElement Tariff_Pick = new XElement("Tariff_Pick", r[14].ToString().Trim().Replace(",", "."));
                        XElement TarifficationBlockId = new XElement("TarifficationBlockId", r[15].ToString().Trim());

                        zona.Add(_id, AccountNumber, PIP, BlockLabel, BlockLabelName, TariffGroupId, TimeZonalId,
                            isHeating, BasePrice, Quantity_Nich, Quantity_PivPick, Quantity_Pick, Tariff_Nich,
                            Tariff_PivPick, Tariff_Pick, TarifficationBlockId
                            );
                        
                        DataSet.Add(zona);
                    }

                    xdoc.Add(DataSet);

                    using (var stream = new MemoryStream())
                    {
                        xdoc.Save(stream);
                        var content = stream.ToArray();
                        return File(
                            content,
                            "application/xml",
                            cokCode + "_" + DateTime.Now.ToString("d") + ".xml");
                    }
                }
                else if (rep.Name == "Надання претензій побутовим споживачам")      //формування претензій для суду
                {
                    string filePath = "\\Files\\Shablons\\";
                    string generatedPath = "pretenziaFO.docx";
                    string fileName = "pretenzia.docx";
                    string fullPath = appEnv.WebRootPath + filePath + fileName;
                    string fullGenerated = appEnv.WebRootPath + filePath + generatedPath;
                    Pretensia pr;
                    if (dt.Rows.Count == 0)
                    {
                        pr = new Pretensia
                        {
                            Cok = user.Cok.Name,
                            Vykonavets = user.FullName,
                            Nach = user.Cok.Nach,
                            AccountNumber = "",
                            AccountNumberNew = "",
                            PIP = "",
                            FullAddress = "",
                            SumaPay = 0
                        };
                    }
                    else
                    {
                        pr = new Pretensia
                        {
                            Cok = user.Cok.NmeDoc,
                            Vykonavets = user.FullName,
                            Nach = user.Cok.Nach,
                            Iban = user.Cok.Rah_Iban,
                            AccountNumber = dt.Rows[0][0].ToString().Trim(),
                            AccountNumberNew = dt.Rows[0][1].ToString().Trim(),
                            PIP = dt.Rows[0][2].ToString().Trim(),
                            FullAddress = dt.Rows[0][3].ToString().Trim(),
                            SumaPay = decimal.Parse(dt.Rows[0][4].ToString().Trim()),
                            DateFrom = DateTime.Parse(dt.Rows[0][5].ToString().Trim()),
                            DateTo = DateTime.Parse(dt.Rows[0][6].ToString().Trim())
                        };
                    }

                    var document = DocumentFactory.Create(fullPath, pr);
                    document.Generate(fullGenerated);

                    string NewFileName = "pretenzia_" + DateTime.Now.ToString() + ".docx";
                    return File(
                        System.IO.File.ReadAllBytes(fullGenerated),
                        System.Net.Mime.MediaTypeNames.Application.Octet,
                        NewFileName
                    );
                }
                else    //формування звітів в екселі
                {
                    ws.Cell(1, 2).Value = rep.Name;
                    ws.Cell(1, 2).Style.Font.Bold = true;
                    ws.Cell(2, 2).Value = "по " + cokCode + " за " + Dtm + " " + Dtr + " р.";
                    ws.Cell(2, 2).Style.Font.Bold = true;

                    int currentRow = 3;
                    int currentCell = 1;
                    foreach (DataColumn coll in dt.Columns)
                    {
                        ws.Cell(currentRow, currentCell).Value = coll.ColumnName;
                        currentCell++;
                    }
                    currentCell = 1; currentRow++;
                    ws.Cell(currentRow, currentCell).Value = dt.Rows;
                }

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "zvit.xlsx");
                }
            }
        }
    }
}