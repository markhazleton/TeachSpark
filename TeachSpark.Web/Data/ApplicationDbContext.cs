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
    }    // DbSets for worksheet generator functionality
    public DbSet<CommonCoreStandard> CommonCoreStandards { get; set; }
    public DbSet<BloomLevel> BloomLevels { get; set; }
    public DbSet<WorksheetTemplate> WorksheetTemplates { get; set; }
    public DbSet<Worksheet> Worksheets { get; set; }
    public DbSet<WorksheetExport> WorksheetExports { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<ApiUsage> ApiUsages { get; set; }
    public DbSet<AcademicStandard> AcademicStandards { get; set; }

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
            .OnDelete(DeleteBehavior.Cascade); builder.Entity<Worksheet>()
            .HasOne(w => w.CommonCoreStandard)
            .WithMany(s => s.Worksheets)
            .HasForeignKey(w => w.CommonCoreStandardId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Worksheet>()
            .HasOne(w => w.AcademicStandard)
            .WithMany(a => a.Worksheets)
            .HasForeignKey(w => w.AcademicStandardId)
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

        builder.Entity<Worksheet>()
            .HasOne(w => w.Template)
            .WithMany(t => t.Worksheets)
            .HasForeignKey(w => w.TemplateId)
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
            .IsUnique(); builder.Entity<ApiUsage>()
            .HasIndex(u => u.RequestedAt); builder.Entity<AcademicStandard>()
            .HasIndex(a => a.GleCode);

        builder.Entity<AcademicStandard>()
            .HasIndex(a => new { a.Subject, a.Grade });

    }
}
