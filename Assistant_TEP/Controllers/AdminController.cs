using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assistant_TEP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Assistant_TEP.ViewModels;
using Assistant_TEP.MyClasses;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Assistant_TEP.Controllers
{
    public class AdminController : Controller
    {
        private readonly MainContext _context;
        private readonly IWebHostEnvironment _appEnvironment;

        public AdminController(MainContext context, IWebHostEnvironment appEnv)
        {
            _context = context;
            _appEnvironment = appEnv;
        }

        // GET: Admin
        [Authorize(Policy = "OnlyForAdministrator")]
        public async Task<IActionResult> Index(int Id)
        {
            return View(await _context.Reports.Include(r => r.DbType).ToListAsync());
        }

        // GET: Admin/Details/5
        [Authorize(Policy = "OnlyForAdministrator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                            .Include(r => r.DbType)
                            .Include(r => r.ReportType)
                            .Include(r => r.ReportParams)
                            .ThenInclude(p => p.ParamType)
                            .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // GET: Admin/Create
        [Authorize(Policy = "OnlyForAdministrator")]
        public IActionResult Create(int Id)
        {
            ViewData["DbTypeId"] = new SelectList(_context.DbTypes, "Id", "Type");
            ViewData["TypeReportId"] = new SelectList(_context.TypeReports, "Id", "Name");
            ViewData["RepId"] = Id;
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "OnlyForAdministrator")]
        public async Task<IActionResult> Create(int Id, [Bind("Name,Description,DbTypeId,TypeReportId")] Report report, ReportWithFile formReport)
        {
            if (formReport.FileScript != null)
            {
                string path = "/Files/Scripts/" + formReport.FileScript.FileName;
                report.FileScript = formReport.FileScript.FileName;
                if (TryValidateModel(report))
                {
                    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                        formReport.FileScript.CopyTo(fileStream);

                    _context.Add(report);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                ViewBag.error = "BadFile";
            }

            ViewData["DbTypeId"] = new SelectList(_context.DbTypes, "Id", "Type");
            ViewData["TypeReportId"] = new SelectList(_context.TypeReports, "Id", "Name");
            ViewData["RepId"] = Id;
            return View(formReport);
        }

        // GET: Admin/Edit/5
        [Authorize(Policy = "OnlyForAdministrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            //заміна Id на назви
            ViewData["DbTypeId"] = new SelectList(_context.DbTypes, "Id", "Type");
            ViewData["TypeReportId"] = new SelectList(_context.TypeReports, "Id", "Name");
            ViewData["RepId"] = id;
            ReportWithFile rpf = new ReportWithFile
            {
                DbTypeId = report.DbTypeId,
                Description = report.Description,
                Id = report.Id,
                Name = report.Name,
                TypeReportId = report.TypeReportId
            };
            return View(rpf);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "OnlyForAdministrator")]
        public async Task<IActionResult> Edit(int id, ReportWithFile formReport)
        {
            if (id != formReport.Id)
            {
                return NotFound();
            }

            Report OldReport = await _context.Reports.FirstOrDefaultAsync(r => r.Id == formReport.Id);
            if(OldReport == null)
            {
                ViewBag.error = "BadId";
            }
            else
            {
                if (formReport.FileScript == null)
                {
                    OldReport.DbTypeId = formReport.DbTypeId;
                    OldReport.Description = formReport.Description;
                    OldReport.Name = formReport.Name;
                    OldReport.TypeReportId = formReport.TypeReportId;
                    if (TryValidateModel(OldReport))
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    string path = "/Files/Scripts/" + formReport.FileScript.FileName;
                    string oldFilePath = OldReport.FileScript;
                    OldReport.DbTypeId = formReport.DbTypeId;
                    OldReport.Description = formReport.Description;
                    OldReport.Name = formReport.Name;
                    OldReport.TypeReportId = formReport.TypeReportId;
                    OldReport.FileScript = formReport.FileScript.FileName;
                    if (TryValidateModel(OldReport))
                    {
                        await Utils.DeleteAsyncFile(_appEnvironment.WebRootPath + "/Files/Scripts/" + oldFilePath);
                        using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                            formReport.FileScript.CopyTo(fileStream);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            ViewData["DbTypeId"] = new SelectList(_context.DbTypes, "Id", "Type");
            ViewData["TypeReportId"] = new SelectList(_context.TypeReports, "Id", "Name");
            ViewData["RepId"] = id;
            return View(formReport);
        }

        // GET: Admin/Delete/5
        [Authorize(Policy = "OnlyForAdministrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "OnlyForAdministrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            string filePath = _appEnvironment.WebRootPath + "/Files/Scripts/" + report.FileScript;
            await Utils.DeleteAsyncFile(filePath);
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Parameters
        [Authorize(Policy = "OnlyForAdministrator")]
        public async Task<IActionResult> ListParam( int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var report = await _context.Reports.FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }
            return View();
        }

        private bool ReportExists(int id)
        {
            return _context.Reports.Any(e => e.Id == id);
        }
    }
}
