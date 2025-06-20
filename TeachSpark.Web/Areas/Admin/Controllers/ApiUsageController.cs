using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;

namespace TeachSpark.Web.Areas.Admin.Controllers
{
    public class ApiUsageController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public ApiUsageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ApiUsage
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "API Usage Analytics";

            var stats = new ApiUsageStats
            {
                TotalRequests = await _context.ApiUsages.CountAsync(),
                TotalToday = await _context.ApiUsages.CountAsync(u => u.RequestedAt.Date == DateTime.UtcNow.Date),
                TotalThisMonth = await _context.ApiUsages.CountAsync(u => u.RequestedAt.Month == DateTime.UtcNow.Month && u.RequestedAt.Year == DateTime.UtcNow.Year),
                AverageResponseTime = _context.ApiUsages
                    .AsEnumerable() // Switch to client-side evaluation
                    .Average(u => u.ResponseTime.TotalMilliseconds),
                TotalTokensUsed = await _context.ApiUsages.SumAsync(u => u.TokensUsed ?? 0),
                TotalCost = await _context.ApiUsages.SumAsync(u => u.CostIncurred ?? 0),
                ErrorRate = await CalculateErrorRate(),
                TopEndpoints = await GetTopEndpoints(),
                DailyUsage = await GetDailyUsage()
            };

            return View(stats);
        }

        // GET: Admin/ApiUsage/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiUsage = await _context.ApiUsages
                .Include(a => a.ApiKey)
                    .ThenInclude(k => k.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (apiUsage == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"API Usage Details - Request #{apiUsage.Id}";
            return View(apiUsage);
        }

        // GET: Admin/ApiUsage/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiUsage = await _context.ApiUsages
                .Include(a => a.ApiKey)
                    .ThenInclude(k => k.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (apiUsage == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Delete API Usage Record - Request #{apiUsage.Id}";
            return View(apiUsage);
        }

        // POST: Admin/ApiUsage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apiUsage = await _context.ApiUsages.FindAsync(id);
            if (apiUsage != null)
            {
                _context.ApiUsages.Remove(apiUsage);
                await _context.SaveChangesAsync();
                SetSuccessMessage($"API Usage record #{apiUsage.Id} has been deleted successfully.");
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/ApiUsage/DeleteOldRecords
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOldRecords(int daysOld = 90)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldRecords = _context.ApiUsages.Where(u => u.RequestedAt < cutoffDate);
            var count = await oldRecords.CountAsync();

            _context.ApiUsages.RemoveRange(oldRecords);
            await _context.SaveChangesAsync();

            SetSuccessMessage($"Deleted {count} API usage records older than {daysOld} days.");
            return RedirectToAction(nameof(Index));
        }

        private async Task<double> CalculateErrorRate()
        {
            var totalRequests = await _context.ApiUsages.CountAsync();
            if (totalRequests == 0) return 0;

            var errorRequests = await _context.ApiUsages.CountAsync(u => u.ResponseStatusCode >= 400);
            return (double)errorRequests / totalRequests * 100;
        }

        private async Task<List<EndpointUsage>> GetTopEndpoints()
        {
            return await _context.ApiUsages
                .GroupBy(u => u.Endpoint)
                .Select(g => new EndpointUsage
                {
                    Endpoint = g.Key,
                    RequestCount = g.Count(),
                    AverageResponseTime = g.Average(u => u.ResponseTime.TotalMilliseconds),
                    ErrorCount = g.Count(u => u.ResponseStatusCode >= 400)
                })
                .OrderByDescending(e => e.RequestCount)
                .Take(10)
                .ToListAsync();
        }

        private async Task<Dictionary<string, int>> GetDailyUsage()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var usage = await _context.ApiUsages
                .Where(u => u.RequestedAt >= thirtyDaysAgo)
                .GroupBy(u => u.RequestedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<string, int>();
            for (int i = 29; i >= 0; i--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                var key = date.ToString("yyyy-MM-dd");
                var count = usage.FirstOrDefault(u => u.Date == date)?.Count ?? 0;
                result[key] = count;
            }

            return result;
        }        // API endpoint for DataTables
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetApiUsageData()
        {
            var apiUsage = await _context.ApiUsages
                .Include(a => a.ApiKey)
                    .ThenInclude(k => k.User)
                .OrderByDescending(a => a.RequestedAt)
                .Select(a => new
                {
                    id = a.Id,
                    endpoint = a.Endpoint ?? string.Empty,
                    httpMethod = a.HttpMethod ?? string.Empty,
                    apiKeyName = a.ApiKey != null ? a.ApiKey.Name : string.Empty,
                    user = a.ApiKey != null && a.ApiKey.User != null ? a.ApiKey.User.Email : string.Empty,
                    ipAddress = a.IpAddress ?? string.Empty,
                    responseStatusCode = a.ResponseStatusCode,
                    responseTime = $"{a.ResponseTime.TotalMilliseconds:F0} ms",
                    tokensUsed = a.TokensUsed,
                    cost = a.CostIncurred != null ? $"${a.CostIncurred:F4}" : string.Empty,
                    requestedAt = a.RequestedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    hasError = !string.IsNullOrEmpty(a.ErrorMessage)
                })
                .ToListAsync();

            return Json(new { data = apiUsage });
        }
    }

    public class ApiUsageStats
    {
        public int TotalRequests { get; set; }
        public int TotalToday { get; set; }
        public int TotalThisMonth { get; set; }
        public double AverageResponseTime { get; set; }
        public int TotalTokensUsed { get; set; }
        public decimal TotalCost { get; set; }
        public double ErrorRate { get; set; }
        public List<EndpointUsage> TopEndpoints { get; set; } = new();
        public Dictionary<string, int> DailyUsage { get; set; } = new();
    }

    public class EndpointUsage
    {
        public string Endpoint { get; set; } = string.Empty;
        public int RequestCount { get; set; }
        public double AverageResponseTime { get; set; }
        public int ErrorCount { get; set; }
    }
}
