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
                .FirstOrDefaultAsync(m => m.Id == id); if (template == null)
            {
                return NotFound();
            }

            // Apply default prompts if they are null
            ApplyDefaultPrompts(template);

            ViewData["Title"] = $"Template Details - {template.Name}";
            return View(template);
        }        // GET: Admin/WorksheetTemplates/Create
        public IActionResult Create()
        {
            var template = new WorksheetTemplate();

            ViewData["Title"] = "Create New Worksheet Template";
            ViewData["Users"] = new SelectList(context.Users, "Id", "Email");
            return View(template);
        }

        // POST: Admin/WorksheetTemplates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,TemplateType,LayoutJson,PreviewImageUrl,SystemPromptTemplate,UserPromptTemplate,IsPublic,IsSystem,UserId")] WorksheetTemplate template)
        {
            if (ModelState.IsValid)
            {
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

            // Apply default prompts if they are null
            ApplyDefaultPrompts(template);

            ViewData["Title"] = $"Edit Template - {template.Name}";
            ViewData["Users"] = new SelectList(context.Users, "Id", "Email", template.UserId);
            return View(template);
        }

        // POST: Admin/WorksheetTemplates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,TemplateType,LayoutJson,PreviewImageUrl,SystemPromptTemplate,UserPromptTemplate,IsPublic,IsSystem,UserId,CreatedAt,UsageCount")] WorksheetTemplate template)
        {
            if (id != template.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
        }

        private static string GetDefaultSystemPromptTemplate(string templateType)
        {
            return templateType?.ToLowerInvariant() switch
            {
                "reading-comprehension" => @"You are an expert educational content creator specializing in reading comprehension worksheets. Create engaging, age-appropriate content for {{GRADE_LEVEL}} students that aligns with the {{BLOOM_LEVEL}} level of Bloom's taxonomy. 

Ensure all content meets the requirements of Common Core Standard: {{COMMON_CORE_STANDARD}}.

Focus on creating clear, structured questions and activities that promote critical thinking and reading skills. Use vocabulary and concepts appropriate for the target grade level. Include a variety of question types such as multiple choice, short answer, and extended response questions.",

                "vocabulary" => @"You are an expert educational content creator specializing in vocabulary development worksheets. Create engaging vocabulary activities for {{GRADE_LEVEL}} students that align with the {{BLOOM_LEVEL}} level of Bloom's taxonomy.

Ensure all content meets the requirements of Common Core Standard: {{COMMON_CORE_STANDARD}}.

Focus on building word knowledge through context clues, word relationships, morphology, and practical application. Include activities that help students understand word meanings, usage, and connections.",

                "grammar" => @"You are an expert educational content creator specializing in grammar and language arts worksheets. Create effective grammar exercises for {{GRADE_LEVEL}} students that align with the {{BLOOM_LEVEL}} level of Bloom's taxonomy.

Ensure all content meets the requirements of Common Core Standard: {{COMMON_CORE_STANDARD}}.

Focus on practical application of grammar rules through examples, exercises, and real-world contexts. Make grammar engaging and relevant to students' lives.",

                "math-practice" => @"You are an expert educational content creator specializing in mathematics worksheets. Create challenging and engaging math problems for {{GRADE_LEVEL}} students that align with the {{BLOOM_LEVEL}} level of Bloom's taxonomy.

Ensure all content meets the requirements of Common Core Standard: {{COMMON_CORE_STANDARD}}.

Focus on problem-solving strategies, mathematical reasoning, and real-world applications. Include a variety of problem types and difficulty levels.",

                "writing-prompt" => @"You are an expert educational content creator specializing in writing instruction. Create inspiring writing prompts and activities for {{GRADE_LEVEL}} students that align with the {{BLOOM_LEVEL}} level of Bloom's taxonomy.

Ensure all content meets the requirements of Common Core Standard: {{COMMON_CORE_STANDARD}}.

Focus on developing students' voice, organization, word choice, and conventions. Provide clear expectations and rubrics for assessment.",

                _ => @"You are an expert educational content creator specializing in {{TEMPLATE_TYPE}} worksheets. Create engaging, age-appropriate content for {{GRADE_LEVEL}} students that aligns with the {{BLOOM_LEVEL}} level of Bloom's taxonomy.

Ensure all content meets the requirements of Common Core Standard: {{COMMON_CORE_STANDARD}}.

Focus on creating clear, structured activities that promote critical thinking and learning. Use vocabulary and concepts appropriate for the target grade level."
            };
        }

        private static string GetDefaultUserPromptTemplate(string templateType)
        {
            return templateType?.ToLowerInvariant() switch
            {
                "reading-comprehension" => @"Create a reading comprehension worksheet with the following specifications:

Instructions: {{WORKSHEET_INSTRUCTIONS}}
Grade Level: {{GRADE_LEVEL}}
Bloom's Level: {{BLOOM_LEVEL}}
Common Core Standard: {{COMMON_CORE_STANDARD}}

Reading Passage:
{{INPUT_TEXT}}

Generate 8-10 comprehension questions that progressively increase in difficulty and thoroughly assess student understanding of the text. Include a mix of:
- Literal comprehension questions (finding facts directly stated in the text)
- Inferential questions (reading between the lines)
- Critical thinking questions (analyzing and evaluating the text)
- Vocabulary questions (understanding word meanings in context)

Format the output as a structured worksheet with:
1. Student information section (Name: _______ Date: _______) 
2. Reading passage (if not provided separately)
3. Clear instructions
4. Numbered questions with adequate space for answers
5. Optional extension activities for advanced students",

                "vocabulary" => @"Create a vocabulary worksheet with the following specifications:

Instructions: {{WORKSHEET_INSTRUCTIONS}}
Grade Level: {{GRADE_LEVEL}}
Bloom's Level: {{BLOOM_LEVEL}}
Common Core Standard: {{COMMON_CORE_STANDARD}}

Source Text/Context:
{{INPUT_TEXT}}

Generate 10-15 vocabulary activities that help students learn and apply new words. Include a variety of activities such as:
- Word definitions and usage
- Context clue exercises
- Word relationships (synonyms, antonyms, analogies)
- Word formation (prefixes, suffixes, root words)
- Application in original sentences or short paragraphs

Format the output as a structured worksheet with:
1. Student information section (Name: _______ Date: _______)
2. Vocabulary word list with definitions
3. Varied practice activities
4. Assessment section
5. Extension activities for enrichment",

                "grammar" => @"Create a grammar worksheet with the following specifications:

Instructions: {{WORKSHEET_INSTRUCTIONS}}
Grade Level: {{GRADE_LEVEL}}
Bloom's Level: {{BLOOM_LEVEL}}
Common Core Standard: {{COMMON_CORE_STANDARD}}

Focus Topic/Examples:
{{INPUT_TEXT}}

Generate 10-12 grammar exercises that help students understand and apply grammar concepts. Include:
- Clear explanation of the grammar rule
- Guided practice examples
- Independent practice exercises
- Application in writing tasks
- Error correction activities

Format the output as a structured worksheet with:
1. Student information section (Name: _______ Date: _______)
2. Grammar rule explanation with examples
3. Progressive practice exercises (easy to challenging)
4. Real-world application activities
5. Self-assessment section",

                "math-practice" => @"Create a mathematics practice worksheet with the following specifications:

Instructions: {{WORKSHEET_INSTRUCTIONS}}
Grade Level: {{GRADE_LEVEL}}
Bloom's Level: {{BLOOM_LEVEL}}
Common Core Standard: {{COMMON_CORE_STANDARD}}

Topic/Context:
{{INPUT_TEXT}}

Generate 12-15 math problems that progressively increase in difficulty. Include:
- Basic computational problems
- Word problems with real-world contexts
- Multi-step problems requiring reasoning
- Visual/graphical problems when appropriate
- Challenge problems for advanced students

Format the output as a structured worksheet with:
1. Student information section (Name: _______ Date: _______)
2. Clear instructions and any necessary formulas
3. Varied problem types with adequate workspace
4. Word problems with real-world applications
5. Challenge section for extension",

                "writing-prompt" => @"Create a writing prompt worksheet with the following specifications:

Instructions: {{WORKSHEET_INSTRUCTIONS}}
Grade Level: {{GRADE_LEVEL}}
Bloom's Level: {{BLOOM_LEVEL}}
Common Core Standard: {{COMMON_CORE_STANDARD}}

Writing Topic/Inspiration:
{{INPUT_TEXT}}

Generate a comprehensive writing assignment that includes:
- Engaging writing prompt
- Pre-writing activities (brainstorming, organizing)
- Writing process guidance
- Revision and editing checklist
- Assessment rubric

Format the output as a structured worksheet with:
1. Student information section (Name: _______ Date: _______)
2. Engaging writing prompt with clear expectations
3. Pre-writing graphic organizers
4. Writing process steps
5. Self-evaluation checklist and peer review guidelines",

                _ => @"Create a {{TEMPLATE_TYPE}} worksheet with the following specifications:

Instructions: {{WORKSHEET_INSTRUCTIONS}}
Grade Level: {{GRADE_LEVEL}}
Bloom's Level: {{BLOOM_LEVEL}}
Common Core Standard: {{COMMON_CORE_STANDARD}}

Content/Topic:
{{INPUT_TEXT}}

Generate 8-12 activities or questions that align with the specified Bloom's level and thoroughly assess student understanding. Include a variety of question types and activities appropriate for the {{BLOOM_LEVEL}} level.

Format the output as a structured worksheet with:
1. Student information section (Name: _______ Date: _______)
2. Clear instructions
3. Numbered activities with adequate space for responses
4. Progressive difficulty levels
5. Extension activities for advanced learners"
            };
        }

        private void ApplyDefaultPrompts(WorksheetTemplate template)
        {
            if (string.IsNullOrEmpty(template.SystemPromptTemplate))
            {
                template.SystemPromptTemplate = GetDefaultSystemPromptTemplate(template.TemplateType);
            }

            if (string.IsNullOrEmpty(template.UserPromptTemplate))
            {
                template.UserPromptTemplate = GetDefaultUserPromptTemplate(template.TemplateType);
            }
        }

        // API endpoint for getting default prompts for a template type
        [HttpGet]
        public IActionResult GetDefaultPrompts(string templateType)
        {
            if (string.IsNullOrEmpty(templateType))
            {
                return BadRequest("Template type is required");
            }

            return Json(new
            {
                systemPrompt = GetDefaultSystemPromptTemplate(templateType),
                userPrompt = GetDefaultUserPromptTemplate(templateType)
            });
        }

        // API endpoint for DataTables        [HttpPost]
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
                    createdAt = t.CreatedAt.ToString("yyyy-MM-dd"),
                    hasSystemPrompt = !string.IsNullOrEmpty(t.SystemPromptTemplate),
                    hasUserPrompt = !string.IsNullOrEmpty(t.UserPromptTemplate)
                })
                .ToListAsync();

            return Json(new { data = templates });
        }
    }
}
