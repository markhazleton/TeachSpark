using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;

namespace TeachSpark.Web.Areas.Admin.Controllers
{
    public class DashboardController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Admin Dashboard";

            var stats = new DashboardStats
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalWorksheets = await _context.Worksheets.CountAsync(),
                TotalTemplates = await _context.WorksheetTemplates.CountAsync(),
                TotalApiKeys = await _context.ApiKeys.CountAsync(),
                TotalBloomLevels = await _context.BloomLevels.CountAsync(),
                TotalCommonCoreStandards = await _context.CommonCoreStandards.CountAsync(),
                RecentWorksheets = await _context.Worksheets
                    .Include(w => w.User)
                    .Include(w => w.BloomLevel)
                    .Include(w => w.CommonCoreStandard)
                    .OrderByDescending(w => w.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                TopApiKeys = await _context.ApiKeys
                    .Include(a => a.User)
                    .OrderByDescending(a => a.RequestCount)
                    .Take(5)
                    .ToListAsync(),
                MonthlyWorksheetCount = await GetMonthlyWorksheetCounts()
            };

            return View(stats);
        }

        private async Task<Dictionary<string, int>> GetMonthlyWorksheetCounts()
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var worksheets = await _context.Worksheets
                .Where(w => w.CreatedAt >= sixMonthsAgo)
                .GroupBy(w => new { w.CreatedAt.Year, w.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            var result = new Dictionary<string, int>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var key = date.ToString("yyyy-MM");
                var count = worksheets.FirstOrDefault(w => w.Year == date.Year && w.Month == date.Month)?.Count ?? 0;
                result[key] = count;
            }

            return result;
        }
    }

    public class DashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalWorksheets { get; set; }
        public int TotalTemplates { get; set; }
        public int TotalApiKeys { get; set; }
        public int TotalBloomLevels { get; set; }
        public int TotalCommonCoreStandards { get; set; }
        public List<Data.Entities.Worksheet> RecentWorksheets { get; set; } = new();
        public List<Data.Entities.ApiKey> TopApiKeys { get; set; } = new();
        public Dictionary<string, int> MonthlyWorksheetCount { get; set; } = new();
    }
}
