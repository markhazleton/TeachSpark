using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;
using TeachSpark.Web.Data.Entities;
using System.Security.Cryptography;
using System.Text;

namespace TeachSpark.Web.Areas.Admin.Controllers
{
    public class ApiKeysController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public ApiKeysController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ApiKeys
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "API Keys Management";
            return View();
        }

        // GET: Admin/ApiKeys/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiKey = await _context.ApiKeys
                .Include(a => a.User)
                .Include(a => a.ApiUsages.OrderByDescending(u => u.RequestedAt).Take(10))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (apiKey == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"API Key Details - {apiKey.Name}";
            return View(apiKey);
        }

        // GET: Admin/ApiKeys/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Create New API Key";
            ViewData["Users"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Admin/ApiKeys/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,UserId,ExpiresAt,DailyRequestLimit,MonthlyRequestLimit,AllowedIpAddresses,Scopes")] ApiKey apiKey)
        {
            if (ModelState.IsValid)
            {
                // Validate JSON formats
                if (!string.IsNullOrEmpty(apiKey.AllowedIpAddresses) && !IsValidJson(apiKey.AllowedIpAddresses))
                {
                    ModelState.AddModelError("AllowedIpAddresses", "Invalid JSON format for IP addresses.");
                }
                if (!string.IsNullOrEmpty(apiKey.Scopes) && !IsValidJson(apiKey.Scopes))
                {
                    ModelState.AddModelError("Scopes", "Invalid JSON format for scopes.");
                }

                if (!ModelState.IsValid)
                {
                    ViewData["Title"] = "Create New API Key";
                    ViewData["Users"] = new SelectList(_context.Users, "Id", "Email", apiKey.UserId);
                    return View(apiKey);
                }

                // Generate API key
                var keyValue = GenerateApiKey();
                apiKey.KeyValue = HashApiKey(keyValue);
                apiKey.KeyPrefix = keyValue.Substring(0, 8);
                apiKey.CreatedAt = DateTime.UtcNow;

                _context.Add(apiKey);
                await _context.SaveChangesAsync();

                SetSuccessMessage($"API Key '{apiKey.Name}' has been created successfully. Key: {keyValue} (Save this key - it won't be shown again!)");
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Create New API Key";
            ViewData["Users"] = new SelectList(_context.Users, "Id", "Email", apiKey.UserId);
            return View(apiKey);
        }

        // GET: Admin/ApiKeys/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiKey = await _context.ApiKeys.FindAsync(id);
            if (apiKey == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit API Key - {apiKey.Name}";
            ViewData["Users"] = new SelectList(_context.Users, "Id", "Email", apiKey.UserId);
            return View(apiKey);
        }

        // POST: Admin/ApiKeys/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,UserId,IsActive,CreatedAt,ExpiresAt,LastUsedAt,RequestCount,DailyRequestLimit,MonthlyRequestLimit,AllowedIpAddresses,Scopes,KeyPrefix")] ApiKey apiKey)
        {
            if (id != apiKey.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate JSON formats
                    if (!string.IsNullOrEmpty(apiKey.AllowedIpAddresses) && !IsValidJson(apiKey.AllowedIpAddresses))
                    {
                        ModelState.AddModelError("AllowedIpAddresses", "Invalid JSON format for IP addresses.");
                    }
                    if (!string.IsNullOrEmpty(apiKey.Scopes) && !IsValidJson(apiKey.Scopes))
                    {
                        ModelState.AddModelError("Scopes", "Invalid JSON format for scopes.");
                    }

                    if (!ModelState.IsValid)
                    {
                        ViewData["Title"] = $"Edit API Key - {apiKey.Name}";
                        ViewData["Users"] = new SelectList(_context.Users, "Id", "Email", apiKey.UserId);
                        return View(apiKey);
                    }

                    // Get the original key value to preserve it
                    var originalApiKey = await _context.ApiKeys.AsNoTracking().FirstAsync(a => a.Id == id);
                    apiKey.KeyValue = originalApiKey.KeyValue;

                    _context.Update(apiKey);
                    await _context.SaveChangesAsync();
                    SetSuccessMessage($"API Key '{apiKey.Name}' has been updated successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApiKeyExists(apiKey.Id))
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
            ViewData["Title"] = $"Edit API Key - {apiKey.Name}";
            ViewData["Users"] = new SelectList(_context.Users, "Id", "Email", apiKey.UserId);
            return View(apiKey);
        }

        // GET: Admin/ApiKeys/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiKey = await _context.ApiKeys
                .Include(a => a.User)
                .Include(a => a.ApiUsages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (apiKey == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Delete API Key - {apiKey.Name}";
            return View(apiKey);
        }

        // POST: Admin/ApiKeys/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apiKey = await _context.ApiKeys
                .Include(a => a.ApiUsages)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apiKey != null)
            {
                // Delete related usage records first
                _context.ApiUsages.RemoveRange(apiKey.ApiUsages);
                _context.ApiKeys.Remove(apiKey);
                await _context.SaveChangesAsync();
                SetSuccessMessage($"API Key '{apiKey.Name}' and its usage records have been deleted successfully.");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ApiKeyExists(int id)
        {
            return _context.ApiKeys.Any(e => e.Id == id);
        }

        private string GenerateApiKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 32)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string HashApiKey(string apiKey)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
            return Convert.ToBase64String(hashedBytes);
        }        // API endpoint for DataTables
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetApiKeysData()
        {
            var apiKeys = await _context.ApiKeys
                .Include(a => a.User)
                .Include(a => a.ApiUsages)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    id = a.Id,
                    name = a.Name ?? "",
                    user = a.User != null ? a.User.Email : "",
                    keyPrefix = a.KeyPrefix ?? "",
                    isActive = a.IsActive,
                    requestCount = a.RequestCount,
                    dailyRequestLimit = a.DailyRequestLimit,
                    monthlyRequestLimit = a.MonthlyRequestLimit,
                    createdAt = a.CreatedAt.ToString("yyyy-MM-dd"),
                    expiresAt = a.ExpiresAt != null ? a.ExpiresAt.Value.ToString("yyyy-MM-dd") : "",
                    lastUsedAt = a.LastUsedAt != null ? a.LastUsedAt.Value.ToString("yyyy-MM-dd HH:mm") : "Never",
                    usageCount = a.ApiUsages.Count
                })
                .ToListAsync();

            return Json(new { data = apiKeys });
        }
    }
}
