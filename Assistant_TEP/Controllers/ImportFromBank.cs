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

namespace Assistant_TEP.Controllers
{
    /// <summary>
    /// імпорт оплат з банків
    /// </summary>
    public class ImportFromBank : Controller
    {
        public static string UserName { get; }
        private readonly MainContext db;
        private readonly IWebHostEnvironment appEnv;
        public static IConfiguration Configuration;

        public ImportFromBank(MainContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            appEnv = appEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
