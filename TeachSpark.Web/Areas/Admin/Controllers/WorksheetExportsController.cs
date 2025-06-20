using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;

namespace TeachSpark.Web.Areas.Admin.Controllers
{
    public class WorksheetExportsController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public WorksheetExportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/WorksheetExports
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Worksheet Exports Management";

            var stats = new ExportStats
            {
                TotalExports = await _context.WorksheetExports.CountAsync(),
                TotalSizeBytes = await _context.WorksheetExports.SumAsync(e => e.FileSizeBytes),
                TotalDownloads = await _context.WorksheetExports.SumAsync(e => e.DownloadCount),
                ExpiredFiles = await _context.WorksheetExports.CountAsync(e => e.ExpiresAt < DateTime.UtcNow),
                TemporaryFiles = await _context.WorksheetExports.CountAsync(e => e.IsTemporary),
                FormatBreakdown = await GetFormatBreakdown()
            };

            ViewData["Stats"] = stats;
            return View();
        }

        // GET: Admin/WorksheetExports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var export = await _context.WorksheetExports
                .Include(e => e.Worksheet)
                    .ThenInclude(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (export == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Export Details - {export.FileName}";
            return View(export);
        }

        // GET: Admin/WorksheetExports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var export = await _context.WorksheetExports
                .Include(e => e.Worksheet)
                    .ThenInclude(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (export == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Delete Export - {export.FileName}";
            return View(export);
        }

        // POST: Admin/WorksheetExports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var export = await _context.WorksheetExports.FindAsync(id);
            if (export != null)
            {
                // Delete physical file if it exists
                if (System.IO.File.Exists(export.FilePath))
                {
                    try
                    {
                        System.IO.File.Delete(export.FilePath);
                    }
                    catch (Exception ex)
                    {
                        SetErrorMessage($"Could not delete physical file: {ex.Message}");
                    }
                }

                _context.WorksheetExports.Remove(export);
                await _context.SaveChangesAsync();
                SetSuccessMessage($"Export '{export.FileName}' has been deleted successfully.");
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/WorksheetExports/CleanupExpired
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanupExpired()
        {
            var expiredExports = await _context.WorksheetExports
                .Where(e => e.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            var deletedCount = 0;
            var errorCount = 0;

            foreach (var export in expiredExports)
            {
                try
                {
                    // Delete physical file if it exists
                    if (System.IO.File.Exists(export.FilePath))
                    {
                        System.IO.File.Delete(export.FilePath);
                    }
                    deletedCount++;
                }
                catch
                {
                    errorCount++;
                }
            }

            _context.WorksheetExports.RemoveRange(expiredExports);
            await _context.SaveChangesAsync();

            if (errorCount > 0)
            {
                SetInfoMessage($"Cleaned up {deletedCount} expired exports. {errorCount} files could not be deleted from disk.");
            }
            else
            {
                SetSuccessMessage($"Successfully cleaned up {deletedCount} expired exports.");
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/WorksheetExports/CleanupTemporary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanupTemporary(int hoursOld = 24)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hoursOld);
            var oldTempExports = await _context.WorksheetExports
                .Where(e => e.IsTemporary && e.CreatedAt < cutoffTime)
                .ToListAsync();

            var deletedCount = 0;
            var errorCount = 0;

            foreach (var export in oldTempExports)
            {
                try
                {
                    // Delete physical file if it exists
                    if (System.IO.File.Exists(export.FilePath))
                    {
                        System.IO.File.Delete(export.FilePath);
                    }
                    deletedCount++;
                }
                catch
                {
                    errorCount++;
                }
            }

            _context.WorksheetExports.RemoveRange(oldTempExports);
            await _context.SaveChangesAsync();

            if (errorCount > 0)
            {
                SetInfoMessage($"Cleaned up {deletedCount} temporary exports older than {hoursOld} hours. {errorCount} files could not be deleted from disk.");
            }
            else
            {
                SetSuccessMessage($"Successfully cleaned up {deletedCount} temporary exports older than {hoursOld} hours.");
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<Dictionary<string, int>> GetFormatBreakdown()
        {
            return await _context.WorksheetExports
                .GroupBy(e => e.ExportFormat)
                .Select(g => new { Format = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Format, x => x.Count);
        }        // API endpoint for DataTables
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetExportsData()
        {
            var exports = await _context.WorksheetExports
                .Include(e => e.Worksheet)
                    .ThenInclude(w => w.User)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new
                {
                    id = e.Id,
                    fileName = e.FileName ?? string.Empty,
                    worksheetTitle = e.Worksheet != null ? e.Worksheet.Title : string.Empty,
                    user = e.Worksheet != null && e.Worksheet.User != null ? e.Worksheet.User.Email : string.Empty,
                    exportFormat = e.ExportFormat ?? string.Empty,
                    fileSizeMB = Math.Round(e.FileSizeBytes / 1024.0 / 1024.0, 2),
                    downloadCount = e.DownloadCount,
                    isTemporary = e.IsTemporary,
                    createdAt = e.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    expiresAt = e.ExpiresAt != null ? e.ExpiresAt.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty,
                    isExpired = e.ExpiresAt < DateTime.UtcNow
                })
                .ToListAsync();

            return Json(new { data = exports });
        }
    }

    public class ExportStats
    {
        public int TotalExports { get; set; }
        public long TotalSizeBytes { get; set; }
        public int TotalDownloads { get; set; }
        public int ExpiredFiles { get; set; }
        public int TemporaryFiles { get; set; }
        public Dictionary<string, int> FormatBreakdown { get; set; } = new();

        public string TotalSizeFormatted => TotalSizeBytes < 1024 * 1024
            ? $"{TotalSizeBytes / 1024.0:F1} KB"
            : $"{TotalSizeBytes / 1024.0 / 1024.0:F1} MB";
    }
}
