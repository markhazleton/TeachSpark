using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;
using TeachSpark.Web.Data.Entities;

namespace TeachSpark.Web.Areas.Admin.Controllers
{
    public class BloomLevelsController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public BloomLevelsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/BloomLevels
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Bloom Levels Management";
            var bloomLevels = await _context.BloomLevels
                .Include(b => b.Worksheets)
                .OrderBy(b => b.Order)
                .ToListAsync();
            return View(bloomLevels);
        }

        // GET: Admin/BloomLevels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloomLevel = await _context.BloomLevels
                .Include(b => b.Worksheets)
                    .ThenInclude(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bloomLevel == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Bloom Level Details - {bloomLevel.Name}";
            return View(bloomLevel);
        }

        // GET: Admin/BloomLevels/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Create New Bloom Level";
            return View();
        }

        // POST: Admin/BloomLevels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Order,ActionVerbs,Examples,ColorCode")] BloomLevel bloomLevel)
        {
            if (ModelState.IsValid)
            {
                // Check if order already exists
                var existingOrder = await _context.BloomLevels.AnyAsync(b => b.Order == bloomLevel.Order);
                if (existingOrder)
                {
                    ModelState.AddModelError("Order", "A Bloom Level with this order already exists.");
                    ViewData["Title"] = "Create New Bloom Level";
                    return View(bloomLevel);
                }

                _context.Add(bloomLevel);
                await _context.SaveChangesAsync();
                SetSuccessMessage($"Bloom Level '{bloomLevel.Name}' has been created successfully.");
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Create New Bloom Level";
            return View(bloomLevel);
        }

        // GET: Admin/BloomLevels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloomLevel = await _context.BloomLevels.FindAsync(id);
            if (bloomLevel == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit Bloom Level - {bloomLevel.Name}";
            return View(bloomLevel);
        }

        // POST: Admin/BloomLevels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Order,ActionVerbs,Examples,ColorCode")] BloomLevel bloomLevel)
        {
            if (id != bloomLevel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if order already exists (excluding current record)
                    var existingOrder = await _context.BloomLevels
                        .AnyAsync(b => b.Order == bloomLevel.Order && b.Id != bloomLevel.Id);
                    if (existingOrder)
                    {
                        ModelState.AddModelError("Order", "A Bloom Level with this order already exists.");
                        ViewData["Title"] = $"Edit Bloom Level - {bloomLevel.Name}";
                        return View(bloomLevel);
                    }

                    _context.Update(bloomLevel);
                    await _context.SaveChangesAsync();
                    SetSuccessMessage($"Bloom Level '{bloomLevel.Name}' has been updated successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BloomLevelExists(bloomLevel.Id))
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
            ViewData["Title"] = $"Edit Bloom Level - {bloomLevel.Name}";
            return View(bloomLevel);
        }

        // GET: Admin/BloomLevels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloomLevel = await _context.BloomLevels
                .Include(b => b.Worksheets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bloomLevel == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Delete Bloom Level - {bloomLevel.Name}";
            return View(bloomLevel);
        }

        // POST: Admin/BloomLevels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bloomLevel = await _context.BloomLevels
                .Include(b => b.Worksheets)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bloomLevel != null)
            {
                if (bloomLevel.Worksheets.Any())
                {
                    SetErrorMessage($"Cannot delete Bloom Level '{bloomLevel.Name}' because it is used by {bloomLevel.Worksheets.Count} worksheet(s).");
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.BloomLevels.Remove(bloomLevel);
                await _context.SaveChangesAsync();
                SetSuccessMessage($"Bloom Level '{bloomLevel.Name}' has been deleted successfully.");
            }

            return RedirectToAction(nameof(Index));
        }
        private bool BloomLevelExists(int id)
        {
            return _context.BloomLevels.Any(e => e.Id == id);
        }

        // API endpoint for DataTables
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetBloomLevelsData()
        {
            var bloomLevels = await _context.BloomLevels
                .Include(b => b.Worksheets)
                .OrderBy(b => b.Order)
                .Select(b => new
                {
                    id = b.Id,
                    name = b.Name,
                    description = b.Description,
                    order = b.Order,
                    colorCode = b.ColorCode,
                    worksheetCount = b.Worksheets.Count,
                    actionVerbs = b.ActionVerbs != null && b.ActionVerbs.Length > 50 ? b.ActionVerbs.Substring(0, 50) + "..." : b.ActionVerbs ?? ""
                })
                .ToListAsync();

            return Json(new { data = bloomLevels });
        }
    }
}
