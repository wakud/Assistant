﻿using System;
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

namespace Assistant_TEP.Controllers
{
    public class HomeController : Controller
    {
        private MainContext _context;
        public HomeController(MainContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View(_context.Reports.Include(r => r.DbType).Include(z => z.ReportType).ToList());
        }

        public IActionResult Import()
        {
            return View();
        }

        public IActionResult SunFlower()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Dovidka()
        {
            return View(_context.Reports.Include(r => r.DbType).Include(z => z.ReportType).ToList());
        }

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
