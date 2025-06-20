using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;
using TeachSpark.Web.Data.Entities;

namespace TeachSpark.Web.Areas.Admin.Controllers
{
    public class WorksheetTemplatesController(ApplicationDbContext context) : BaseAdminController
    {

        // GET: Admin/WorksheetTemplates
        public IActionResult Index()
        {
            ViewData["Title"] = "Worksheet Templates Management";
            return View();
        }

        // GET: Admin/WorksheetTemplates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await context.WorksheetTemplates
                .Include(t => t.User)
                .Include(t => t.Worksheets)
                    .ThenInclude(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Template Details - {template.Name}";
            return View(template);
        }

        // GET: Admin/WorksheetTemplates/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Create New Worksheet Template";
            ViewData["Users"] = new SelectList(context.Users, "Id", "Email");
            return View();
        }

        // POST: Admin/WorksheetTemplates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,TemplateType,LayoutJson,PreviewImageUrl,IsPublic,IsSystem,UserId")] WorksheetTemplate template)
        {
            if (ModelState.IsValid)
            {
                // Validate JSON format
                if (!string.IsNullOrEmpty(template.LayoutJson) && !IsValidJson(template.LayoutJson))
                {
                    ModelState.AddModelError("LayoutJson", "Invalid JSON format.");
                    ViewData["Title"] = "Create New Worksheet Template";
                    ViewData["Users"] = new SelectList(context.Users, "Id", "Email", template.UserId);
                    return View(template);
                }

                template.CreatedAt = DateTime.UtcNow;
                template.UpdatedAt = DateTime.UtcNow;
                context.Add(template);
                await context.SaveChangesAsync();
                SetSuccessMessage($"Worksheet Template '{template.Name}' has been created successfully.");
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Create New Worksheet Template";
            ViewData["Users"] = new SelectList(context.Users, "Id", "Email", template.UserId);
            return View(template);
        }

        // GET: Admin/WorksheetTemplates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await context.WorksheetTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit Template - {template.Name}";
            ViewData["Users"] = new SelectList(context.Users, "Id", "Email", template.UserId);
            return View(template);
        }

        // POST: Admin/WorksheetTemplates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,TemplateType,LayoutJson,PreviewImageUrl,IsPublic,IsSystem,UserId,CreatedAt,UsageCount")] WorksheetTemplate template)
        {
            if (id != template.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate JSON format
                    if (!string.IsNullOrEmpty(template.LayoutJson) && !IsValidJson(template.LayoutJson))
                    {
                        ModelState.AddModelError("LayoutJson", "Invalid JSON format.");
                        ViewData["Title"] = $"Edit Template - {template.Name}";
                        ViewData["Users"] = new SelectList(context.Users, "Id", "Email", template.UserId);
                        return View(template);
                    }

                    template.UpdatedAt = DateTime.UtcNow;
                    context.Update(template);
                    await context.SaveChangesAsync();
                    SetSuccessMessage($"Worksheet Template '{template.Name}' has been updated successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorksheetTemplateExists(template.Id))
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
            ViewData["Title"] = $"Edit Template - {template.Name}";
            ViewData["Users"] = new SelectList(context.Users, "Id", "Email", template.UserId);
            return View(template);
        }

        // GET: Admin/WorksheetTemplates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await context.WorksheetTemplates
                .Include(t => t.User)
                .Include(t => t.Worksheets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Delete Template - {template.Name}";
            return View(template);
        }

        // POST: Admin/WorksheetTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var template = await context.WorksheetTemplates
                .Include(t => t.Worksheets)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template != null)
            {
                if (template.Worksheets.Any())
                {
                    SetErrorMessage($"Cannot delete Template '{template.Name}' because it is used by {template.Worksheets.Count} worksheet(s).");
                    return RedirectToAction(nameof(Delete), new { id });
                }

                context.WorksheetTemplates.Remove(template);
                await context.SaveChangesAsync();
                SetSuccessMessage($"Worksheet Template '{template.Name}' has been deleted successfully.");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool WorksheetTemplateExists(int id)
        {
            return context.WorksheetTemplates.Any(e => e.Id == id);
        }        // API endpoint for DataTables
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetTemplatesData()
        {
            var templates = await context.WorksheetTemplates
                .Include(t => t.User)
                .Include(t => t.Worksheets)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name ?? string.Empty,
                    description = t.Description != null && t.Description.Length > 80
                        ? t.Description.Substring(0, 80) + "..."
                        : t.Description ?? string.Empty,
                    templateType = t.TemplateType ?? string.Empty,
                    isPublic = t.IsPublic,
                    isSystem = t.IsSystem,
                    createdBy = t.User != null ? t.User.Email : "System",
                    usageCount = t.UsageCount,
                    worksheetCount = t.Worksheets.Count,
                    createdAt = t.CreatedAt.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            return Json(new { data = templates });
        }
    }
}
