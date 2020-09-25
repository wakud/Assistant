using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Assistant_TEP.Models;
using Assistant_TEP.MyClasses;
using Assistant_TEP.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using SharpDocx;

namespace Assistant_TEP.Controllers
{
    public class ReportsController : Controller
    {
        public static string UserName { get; }
        public static string cokCode;
        public static Dictionary<string, DataTable> zvit = new Dictionary<string, DataTable>();
        public static Dictionary<string, string> parameters = new Dictionary<string, string>();
        private readonly MainContext db;
        private readonly IWebHostEnvironment appEnv;

        public ReportsController(MainContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            appEnv = appEnvironment;
        }

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
                        DataTable selects = BillingUtils.ExecuteRawSql(script, cokCode);
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
                        Utils.ParamSelectData selectData = new Utils.ParamSelectData { NameParam = rp.Name, selects = htmlSelect };
                        selectParamsList.Add(selectData);
                    }
                }
            }
            ViewData["SingleSelectParams"] = selectParamsList;
            return View(ViewModel);
        }

        public ActionResult ExecuteReport(int id, IFormCollection form)
        {
            User user = db.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == User.Identity.Name);
            if(user.IsAdmin == "1")
            {
                cokCode = form["organization"];
            }
            else
            {
                cokCode = user.Cok.Code;
            }
            Report rep = db.Reports.Include(r => r.ReportParams).ThenInclude(r => r.ParamType).Include(r => r.DbType).FirstOrDefault(r => r.Id == id);
            
            foreach (ReportParam param in rep.ReportParams)
            {
                if (param.ParamType.TypeC == "period")
                {
                    parameters[param.Name] = ParamSerializer.serializePeriod(form[param.Name]);
                }
                else
                {
                    parameters[param.Name] = form[param.Name];
                }
            }
            DataTable dt = BillingUtils.GetReportResults(appEnv.WebRootPath + "\\Files\\Scripts\\", rep, parameters, cokCode);
            ResultModelView res = new ResultModelView
            {
                ReportId = rep.Id,
                results = dt
            };
            zvit[user.Id.ToString() + "_" + rep.Id] = dt;
            return View(res);
        }

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
                
                if (rep.Name == "Видача довідки про оплату")
                {
                    string filePath = "\\Files\\Shablons\\";
                    string generatedPath = "dovidka.docx";
                    string fileName = "shablon.docx";
                    string fullPath = appEnv.WebRootPath + filePath + fileName;
                    string fullGenerated = appEnv.WebRootPath + filePath + generatedPath;
                    DateTime now = DateTime.Now;

                    // Назва звіту
                    ws.Cell(1, 2).Value = "Довідка про оплату за електроенергію";
                    ws.Cell(1, 2).Style.Font.Bold = true;
                }
                else
                {
                    // Назва звіту
                    ws.Cell(1, 2).Value = rep.Name;
                    ws.Cell(1, 2).Style.Font.Bold = true;
                    ws.Cell(2, 2).Value = "по " + cokCode + " за " + Dtm + " " + Dtr + " р.";
                    ws.Cell(2, 2).Style.Font.Bold = true;
                }

                int currentRow = 3;
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

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "zvit.xlsx");
                }
            }
        }
    }
}