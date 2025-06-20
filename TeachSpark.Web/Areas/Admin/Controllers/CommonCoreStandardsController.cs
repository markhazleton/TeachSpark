using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;
using TeachSpark.Web.Data.Entities;

namespace TeachSpark.Web.Areas.Admin.Controllers
{
    public class CommonCoreStandardsController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public CommonCoreStandardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/CommonCoreStandards
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Common Core Standards Management";
            return View();
        }

        // GET: Admin/CommonCoreStandards/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var standard = await _context.CommonCoreStandards
                .Include(s => s.Worksheets)
                    .ThenInclude(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (standard == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Standard Details - {standard.Code}";
            return View(standard);
        }

        // GET: Admin/CommonCoreStandards/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Create New Common Core Standard";
            return View();
        }

        // POST: Admin/CommonCoreStandards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Grade,Subject,Domain,Description,Category,ExampleActivities,IsActive,SortOrder")] CommonCoreStandard standard)
        {
            if (ModelState.IsValid)
            {
                // Check if code already exists
                var existingCode = await _context.CommonCoreStandards.AnyAsync(s => s.Code == standard.Code);
                if (existingCode)
                {
                    ModelState.AddModelError("Code", "A standard with this code already exists.");
                    ViewData["Title"] = "Create New Common Core Standard";
                    return View(standard);
                }

                _context.Add(standard);
                await _context.SaveChangesAsync();
                SetSuccessMessage($"Common Core Standard '{standard.Code}' has been created successfully.");
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Create New Common Core Standard";
            return View(standard);
        }

        // GET: Admin/CommonCoreStandards/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var standard = await _context.CommonCoreStandards.FindAsync(id);
            if (standard == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit Standard - {standard.Code}";
            return View(standard);
        }

        // POST: Admin/CommonCoreStandards/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Grade,Subject,Domain,Description,Category,ExampleActivities,IsActive,SortOrder")] CommonCoreStandard standard)
        {
            if (id != standard.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if code already exists (excluding current record)
                    var existingCode = await _context.CommonCoreStandards
                        .AnyAsync(s => s.Code == standard.Code && s.Id != standard.Id);
                    if (existingCode)
                    {
                        ModelState.AddModelError("Code", "A standard with this code already exists.");
                        ViewData["Title"] = $"Edit Standard - {standard.Code}";
                        return View(standard);
                    }

                    _context.Update(standard);
                    await _context.SaveChangesAsync();
                    SetSuccessMessage($"Common Core Standard '{standard.Code}' has been updated successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommonCoreStandardExists(standard.Id))
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
            ViewData["Title"] = $"Edit Standard - {standard.Code}";
            return View(standard);
        }

        // GET: Admin/CommonCoreStandards/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var standard = await _context.CommonCoreStandards
                .Include(s => s.Worksheets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (standard == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Delete Standard - {standard.Code}";
            return View(standard);
        }

        // POST: Admin/CommonCoreStandards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var standard = await _context.CommonCoreStandards
                .Include(s => s.Worksheets)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (standard != null)
            {
                if (standard.Worksheets.Any())
                {
                    SetErrorMessage($"Cannot delete Standard '{standard.Code}' because it is used by {standard.Worksheets.Count} worksheet(s).");
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.CommonCoreStandards.Remove(standard);
                await _context.SaveChangesAsync();
                SetSuccessMessage($"Common Core Standard '{standard.Code}' has been deleted successfully.");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CommonCoreStandardExists(int id)
        {
            return _context.CommonCoreStandards.Any(e => e.Id == id);
        }        // API endpoint for DataTables
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetStandardsData()
        {
            var standards = await _context.CommonCoreStandards
                .Include(s => s.Worksheets)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Code)
                .Select(s => new
                {
                    id = s.Id,
                    code = s.Code,
                    grade = s.Grade,
                    subject = s.Subject,
                    domain = s.Domain,
                    category = s.Category ?? "",
                    description = s.Description != null && s.Description.Length > 100 ? s.Description.Substring(0, 100) + "..." : s.Description ?? "",
                    isActive = s.IsActive,
                    worksheetCount = s.Worksheets.Count,
                    sortOrder = s.SortOrder
                })
                .ToListAsync();

            return Json(new { data = standards });
        }
    }
}
