using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Data.Entities
{
    public class Worksheet
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public virtual ApplicationUser User { get; set; } = null!;

        public int? CommonCoreStandardId { get; set; }
        public virtual CommonCoreStandard? CommonCoreStandard { get; set; }

        public int? BloomLevelId { get; set; }
        public virtual BloomLevel? BloomLevel { get; set; }

        public int? TemplateId { get; set; }
        public virtual WorksheetTemplate? Template { get; set; }

        [Required]
        public string ContentJson { get; set; } = string.Empty; // Generated worksheet content

        public string? SourceText { get; set; } // Original text used for generation

        [StringLength(50)]
        public string WorksheetType { get; set; } = string.Empty; // reading-comprehension, vocabulary, etc.

        [StringLength(20)]
        public string DifficultyLevel { get; set; } = "standard"; // standard, simplified, advanced

        public string? AccessibilityOptions { get; set; } // JSON array of accessibility features

        public string? Tags { get; set; } // Comma-separated tags for organization

        public bool IsPublic { get; set; } = false; // Can be shared with other teachers

        public bool IsFavorite { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastAccessedAt { get; set; }

        public int ViewCount { get; set; } = 0;

        public int DownloadCount { get; set; } = 0;

        // Generation metadata
        public string? LlmModel { get; set; } // Which LLM model was used
        public string? GenerationPrompt { get; set; } // The prompt used for generation
        public decimal? GenerationCost { get; set; } // Cost in tokens/credits
        public TimeSpan? GenerationTime { get; set; } // How long it took to generate

        // Navigation Properties
        public virtual ICollection<WorksheetExport> Exports { get; set; } = new List<WorksheetExport>();
    }
}
