using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Assistant_TEP.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.Data.SqlClient;
using Assistant_TEP.MyClasses;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using static Assistant_TEP.MyClasses.Utils;

namespace Assistant_TEP.Controllers
{
    /// <summary>
    /// головний контролер програми 
    /// </summary>
    public class HomeController : Controller
    {
        private MainContext _context;

        public HomeController(MainContext context)
        {
            _context = context;
        }
        /// <summary>
        /// тільки для користувача moskestr видно цей розділ
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Index()
        {
            if (User.Identity.Name == "moskestr")
            {
                return RedirectToAction ("SunFlower");
            }
            return View(_context.Reports.Include(r => r.DbType).Include(z => z.ReportType).ToList());
        }
        /// <summary>
        /// тільки для авторизованих виводимо список звітів
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Retail()
        {
            return View(_context.Reports.Include(r => r.DbType).Include(z => z.ReportType).ToList());
        }
        /// <summary>
        /// імпорт оплат
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Import()
        {
            User user = _context.Users
                .Include(u => u.Cok)
                .FirstOrDefault(u => u.Login == User.Identity.Name);
            string cok = user.Cok.Code;
            string script = "SELECT ReceiptSourceId, Name FROM FinanceDictionary.ReceiptSource ORDER BY Name";
            DataTable dt = new DataTable();
            BillingUtils.ExecuteRawSql(script, cok, dt);
            List<SelectParamReport> htmlSelect = new List<SelectParamReport>();
            foreach (DataRow row in dt.Rows)
            {
                htmlSelect.Add(new SelectParamReport()
                {
                    Id = row[0].ToString(), Name = row[1].ToString()
                });
            }
            
            ViewBag.Codes = new SelectList(htmlSelect, "Id", "Name");
            return View();
        }
        /// <summary>
        /// сонячні
        /// </summary>
        /// <returns></returns>
        public IActionResult SunFlower()
        {
            return View();
        }
        /// <summary>
        /// звіти на замовлення
        /// </summary>
        /// <returns></returns>
        public IActionResult Privacy()
        {
            return View();
        }
        /// <summary>
        /// видача довідок
        /// </summary>
        /// <returns></returns>
        public IActionResult Dovidka()
        {
            return View(_context.Reports.Include(r => r.DbType).Include(z => z.ReportType).ToList());
        }
        /// <summary>
        /// інші звіти
        /// </summary>
        /// <returns></returns>
        public IActionResult Other()
        {
            return View(_context.Reports.Include(r => r.DbType).Include(z => z.ReportType).ToList());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
