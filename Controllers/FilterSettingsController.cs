using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JobFilter.Data;
using JobFilter.Models;

namespace JobFilter.Controllers
{
    public class FilterSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FilterSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FilterSettings
        public async Task<IActionResult> Index()
        {
            return View(await _context.FilterSetting.ToListAsync());
        }

        // GET: FilterSettings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var filterSetting = await _context.FilterSetting
                .FirstOrDefaultAsync(m => m.Id == id);
            if (filterSetting == null)
            {
                return NotFound();
            }

            return View(filterSetting);
        }

        // GET: FilterSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FilterSettings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CrawlUrl,ExcludeWord,IgnoreCompany,MinimumWage")] FilterSetting filterSetting)
        {
            if (ModelState.IsValid)
            {
                _context.Add(filterSetting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(filterSetting);
        }

        // GET: FilterSettings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var filterSetting = await _context.FilterSetting.FindAsync(id);
            if (filterSetting == null)
            {
                return NotFound();
            }
            return View(filterSetting);
        }

        // POST: FilterSettings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CrawlUrl,ExcludeWord,IgnoreCompany,MinimumWage")] FilterSetting filterSetting)
        {
            if (id != filterSetting.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(filterSetting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilterSettingExists(filterSetting.Id))
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
            return View(filterSetting);
        }

        // GET: FilterSettings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var filterSetting = await _context.FilterSetting
                .FirstOrDefaultAsync(m => m.Id == id);
            if (filterSetting == null)
            {
                return NotFound();
            }

            return View(filterSetting);
        }

        // POST: FilterSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var filterSetting = await _context.FilterSetting.FindAsync(id);
            _context.FilterSetting.Remove(filterSetting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FilterSettingExists(int id)
        {
            return _context.FilterSetting.Any(e => e.Id == id);
        }
    }
}
