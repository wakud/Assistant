using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assistant_TEP.Models;

namespace Assistant_TEP.Controllers
{
    public class ReportParamsController : Controller
    {
        private readonly MainContext _context;

        public ReportParamsController(MainContext context)
        {
            _context = context;
        }

        // GET: ReportParams
        public async Task<IActionResult> Index()
        {
            var mainContext = _context.ReportParams.Include(r => r.ParamType).Include(r => r.Report);
            return View(await mainContext.ToListAsync());
        }

        // GET: ReportParams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reportParam = await _context.ReportParams
                .Include(r => r.ParamType)
                .Include(r => r.Report)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reportParam == null)
            {
                return NotFound();
            }

            return View(reportParam);
        }

        // GET: ReportParams/Create
        public IActionResult Create(int Id)
        {
            ViewData["ParamTypeId"] = new SelectList(_context.ReportParamTypes, "Id", "Name");
            ViewData["RepId"] = Id;
            return View();
        }

        // POST: ReportParams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int Id, [Bind("Name,Description,ReportId,ParamTypeId")] ReportParam reportParam)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reportParam);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParamTypeId"] = new SelectList(_context.ReportParamTypes, "Id", "Name", reportParam.ParamTypeId);
            ViewData["RepId"] = Id;
            return View(reportParam);
        }

        // GET: ReportParams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reportParam = await _context.ReportParams.FindAsync(id);
            if (reportParam == null)
            {
                return NotFound();
            }
            ViewData["ParamTypeId"] = new SelectList(_context.ReportParamTypes, "Id", "Name", reportParam.ParamTypeId);
            ViewData["ReportId"] = new SelectList(_context.Reports, "Id", "Name", reportParam.ReportId);
            return View(reportParam);
        }

        // POST: ReportParams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ReportId,ParamTypeId")] ReportParam reportParam)
        {
            if (id != reportParam.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reportParam);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportParamExists(reportParam.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParamTypeId"] = new SelectList(_context.ReportParamTypes, "Id", "Name", reportParam.ParamTypeId);
            ViewData["ReportId"] = new SelectList(_context.Reports, "Id", "Name", reportParam.ReportId);
            return View(reportParam);
        }

        // GET: ReportParams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reportParam = await _context.ReportParams
                .Include(r => r.ParamType)
                .Include(r => r.Report)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reportParam == null)
            {
                return NotFound();
            }

            return View(reportParam);
        }

        // POST: ReportParams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reportParam = await _context.ReportParams.FindAsync(id);
            _context.ReportParams.Remove(reportParam);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReportParamExists(int id)
        {
            return _context.ReportParams.Any(e => e.Id == id);
        }
    }
}
