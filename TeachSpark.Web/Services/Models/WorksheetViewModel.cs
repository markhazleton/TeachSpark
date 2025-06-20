using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Services.Models
{
    public class WorksheetViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? UserDisplayName { get; set; }

        public int? CommonCoreStandardId { get; set; }
        public string? CommonCoreStandardCode { get; set; }
        public string? CommonCoreStandardDescription { get; set; }

        public int? AcademicStandardId { get; set; }
        public string? AcademicStandardCode { get; set; }
        public string? AcademicStandardDescription { get; set; }

        public int? BloomLevelId { get; set; }
        public string? BloomLevelName { get; set; }

        public int? TemplateId { get; set; }
        public string? TemplateName { get; set; }

        [Required]
        public string ContentJson { get; set; } = string.Empty;

        public string? SourceText { get; set; }

        [StringLength(50)]
        public string WorksheetType { get; set; } = string.Empty;

        [StringLength(20)]
        public string DifficultyLevel { get; set; } = "standard";

        public string? AccessibilityOptions { get; set; }

        public string? Tags { get; set; }

        public bool IsPublic { get; set; } = false;

        public bool IsFavorite { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public class CreateWorksheetViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? CommonCoreStandardId { get; set; }

        public int? AcademicStandardId { get; set; }

        public int? BloomLevelId { get; set; }

        public int? TemplateId { get; set; }

        [Required]
        public string ContentJson { get; set; } = string.Empty;

        public string? SourceText { get; set; }

        [StringLength(50)]
        public string WorksheetType { get; set; } = string.Empty;

        [StringLength(20)]
        public string DifficultyLevel { get; set; } = "standard";

        public string? AccessibilityOptions { get; set; }

        public string? Tags { get; set; }

        public bool IsPublic { get; set; } = false;

        public bool IsFavorite { get; set; } = false;
    }

    public class UpdateWorksheetViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? CommonCoreStandardId { get; set; }

        public int? AcademicStandardId { get; set; }

        public int? BloomLevelId { get; set; }

        public int? TemplateId { get; set; }

        [Required]
        public string ContentJson { get; set; } = string.Empty;

        public string? SourceText { get; set; }

        [StringLength(50)]
        public string WorksheetType { get; set; } = string.Empty;

        [StringLength(20)]
        public string DifficultyLevel { get; set; } = "standard";

        public string? AccessibilityOptions { get; set; }

        public string? Tags { get; set; }

        public bool IsPublic { get; set; } = false;

        public bool IsFavorite { get; set; } = false;
    }
}
