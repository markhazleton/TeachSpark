using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data.Entities;

namespace TeachSpark.Web.Data;

/// <summary>
/// Application database context for Identity and worksheet generator entities
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for worksheet generator functionality
    public DbSet<CommonCoreStandard> CommonCoreStandards { get; set; }
    public DbSet<BloomLevel> BloomLevels { get; set; }
    public DbSet<WorksheetTemplate> WorksheetTemplates { get; set; }
    public DbSet<Worksheet> Worksheets { get; set; }
    public DbSet<WorksheetExport> WorksheetExports { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<ApiUsage> ApiUsages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser entity
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.School)
                .HasMaxLength(100);

            entity.Property(e => e.District)
                .HasMaxLength(50);

            entity.Property(e => e.GradeLevel)
                .HasMaxLength(20);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.HasIndex(e => e.Email)
                .IsUnique();
        });

        // Configure relationships
        builder.Entity<Worksheet>()
            .HasOne(w => w.User)
            .WithMany(u => u.Worksheets)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Worksheet>()
            .HasOne(w => w.CommonCoreStandard)
            .WithMany(s => s.Worksheets)
            .HasForeignKey(w => w.CommonCoreStandardId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Worksheet>()
            .HasOne(w => w.BloomLevel)
            .WithMany(b => b.Worksheets)
            .HasForeignKey(w => w.BloomLevelId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<WorksheetTemplate>()
            .HasOne(t => t.User)
            .WithMany(u => u.WorksheetTemplates)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ApiKey>()
            .HasOne(a => a.User)
            .WithMany(u => u.ApiKeys)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApiUsage>()
            .HasOne(u => u.ApiKey)
            .WithMany(k => k.ApiUsages)
            .HasForeignKey(u => u.ApiKeyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WorksheetExport>()
            .HasOne(e => e.Worksheet)
            .WithMany(w => w.Exports)
            .HasForeignKey(e => e.WorksheetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.Entity<Worksheet>()
            .HasIndex(w => w.UserId);

        builder.Entity<Worksheet>()
            .HasIndex(w => w.CreatedAt);

        builder.Entity<CommonCoreStandard>()
            .HasIndex(s => s.Code)
            .IsUnique();

        builder.Entity<BloomLevel>()
            .HasIndex(b => b.Order)
            .IsUnique();

        builder.Entity<ApiKey>()
            .HasIndex(a => a.KeyValue)
            .IsUnique();

        builder.Entity<ApiUsage>()
            .HasIndex(u => u.RequestedAt);

        // Seed Data
        SeedBloomLevels(builder);
        SeedCommonCoreStandards(builder);
        SeedWorksheetTemplates(builder);
    }

    private void SeedBloomLevels(ModelBuilder builder)
    {
        builder.Entity<BloomLevel>().HasData(
            new BloomLevel
            {
                Id = 1,
                Name = "Remember",
                Description = "Retrieving, recognizing, and recalling relevant knowledge from long-term memory",
                Order = 1,
                ActionVerbs = "define, duplicate, list, memorize, recall, repeat, reproduce, state, identify, name, recognize, select, match",
                Examples = "Define literary terms, list character names, recall plot events",
                ColorCode = "#6f42c1"
            },
            new BloomLevel
            {
                Id = 2,
                Name = "Understand",
                Description = "Constructing meaning from oral, written, and graphic messages",
                Order = 2,
                ActionVerbs = "classify, describe, discuss, explain, express, identify, indicate, locate, recognize, report, restate, review, select, translate, paraphrase, summarize",
                Examples = "Explain theme, summarize plot, describe character traits",
                ColorCode = "#0d6efd"
            },
            new BloomLevel
            {
                Id = 3,
                Name = "Apply",
                Description = "Carrying out or using a procedure through executing or implementing",
                Order = 3,
                ActionVerbs = "apply, carry out, demonstrate, dramatize, employ, illustrate, interpret, operate, practice, schedule, sketch, solve, use, write, implement",
                Examples = "Apply grammar rules, use literary devices, implement writing strategies",
                ColorCode = "#198754"
            },
            new BloomLevel
            {
                Id = 4,
                Name = "Analyze",
                Description = "Breaking material into constituent parts and determining how parts relate to each other",
                Order = 4,
                ActionVerbs = "analyze, appraise, calculate, categorize, compare, contrast, criticize, differentiate, discriminate, distinguish, examine, experiment, question, test, deconstruct",
                Examples = "Compare characters, analyze author's purpose, examine text structure",
                ColorCode = "#fd7e14"
            },
            new BloomLevel
            {
                Id = 5,
                Name = "Evaluate",
                Description = "Making judgments based on criteria and standards through checking and critiquing",
                Order = 5,
                ActionVerbs = "appraise, argue, assess, attach, choose, compare, defend, estimate, judge, predict, rate, select, support, value, evaluate, critique",
                Examples = "Critique author's argument, evaluate source credibility, judge character decisions",
                ColorCode = "#dc3545"
            },
            new BloomLevel
            {
                Id = 6,
                Name = "Create",
                Description = "Putting elements together to form a coherent or functional whole; reorganizing into a new pattern",
                Order = 6,
                ActionVerbs = "arrange, assemble, collect, compose, construct, create, design, develop, formulate, manage, organize, plan, prepare, propose, set up, write, generate",
                Examples = "Write alternative ending, create character analysis, design presentation",
                ColorCode = "#20c997"
            }
        );
    }

    private void SeedCommonCoreStandards(ModelBuilder builder)
    {
        // Sample 8th Grade ELA Standards - add more as needed
        builder.Entity<CommonCoreStandard>().HasData(
            new CommonCoreStandard
            {
                Id = 1,
                Code = "CCSS.ELA-LITERACY.RL.8.1",
                Grade = "8",
                Subject = "English Language Arts",
                Domain = "Reading: Literature",
                Category = "Key Ideas and Details",
                Description = "Cite the textual evidence that most strongly supports an analysis of what the text says explicitly as well as inferences drawn from the text.",
                SortOrder = 1
            },
            new CommonCoreStandard
            {
                Id = 2,
                Code = "CCSS.ELA-LITERACY.RL.8.2",
                Grade = "8",
                Subject = "English Language Arts",
                Domain = "Reading: Literature",
                Category = "Key Ideas and Details",
                Description = "Determine a theme or central idea of a text and analyze its development over the course of the text, including its relationship to the characters, setting, and plot.",
                SortOrder = 2
            },
            new CommonCoreStandard
            {
                Id = 3,
                Code = "CCSS.ELA-LITERACY.RL.8.3",
                Grade = "8",
                Subject = "English Language Arts",
                Domain = "Reading: Literature",
                Category = "Key Ideas and Details",
                Description = "Analyze how particular lines of dialogue or incidents in a story or drama propel the action, reveal aspects of a character, or provoke a decision.",
                SortOrder = 3
            },
            new CommonCoreStandard
            {
                Id = 4,
                Code = "CCSS.ELA-LITERACY.W.8.1",
                Grade = "8",
                Subject = "English Language Arts",
                Domain = "Writing",
                Category = "Text Types and Purposes",
                Description = "Write arguments to support claims with clear reasons and relevant evidence.",
                SortOrder = 4
            },
            new CommonCoreStandard
            {
                Id = 5,
                Code = "CCSS.ELA-LITERACY.L.8.1",
                Grade = "8",
                Subject = "English Language Arts",
                Domain = "Language",
                Category = "Conventions of Standard English",
                Description = "Demonstrate command of the conventions of standard English grammar and usage when writing or speaking.",
                SortOrder = 5
            }
        );
    }
    private void SeedWorksheetTemplates(ModelBuilder builder)
    {
        builder.Entity<WorksheetTemplate>().HasData(
            new WorksheetTemplate
            {
                Id = 1,
                Name = "Reading Comprehension - Basic",
                Description = "Standard reading comprehension worksheet with passage and questions",
                TemplateType = "reading-comprehension",
                LayoutJson = """{"sections": [{"type": "passage", "title": "Reading Passage"}, {"type": "questions", "title": "Comprehension Questions", "questionCount": 5}]}""",
                IsPublic = true,
                IsSystem = true,
                UsageCount = 0,
                CreatedAt = new DateTime(2025, 6, 19, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 6, 19, 0, 0, 0, DateTimeKind.Utc)
            },
            new WorksheetTemplate
            {
                Id = 2,
                Name = "Vocabulary Builder",
                Description = "Vocabulary worksheet with definitions and context practice",
                TemplateType = "vocabulary",
                LayoutJson = """{"sections": [{"type": "vocabulary", "title": "New Words"}, {"type": "definitions", "title": "Match Definitions"}, {"type": "sentences", "title": "Use in Sentences"}]}""",
                IsPublic = true,
                IsSystem = true,
                UsageCount = 0,
                CreatedAt = new DateTime(2025, 6, 19, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 6, 19, 0, 0, 0, DateTimeKind.Utc)
            },
            new WorksheetTemplate
            {
                Id = 3,
                Name = "Grammar Practice",
                Description = "Grammar exercises with examples and practice problems",
                TemplateType = "grammar",
                LayoutJson = """{"sections": [{"type": "explanation", "title": "Grammar Rule"}, {"type": "examples", "title": "Examples"}, {"type": "practice", "title": "Practice Exercises"}]}""",
                IsPublic = true,
                IsSystem = true,
                UsageCount = 0,
                CreatedAt = new DateTime(2025, 6, 19, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 6, 19, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
