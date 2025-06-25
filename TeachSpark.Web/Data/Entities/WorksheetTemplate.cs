using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Data.Entities
{
    public class WorksheetTemplate
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "Default";

        public string? Description { get; set; } = "Default template description";

        [Required]
        [StringLength(50)]
        public string TemplateType { get; set; } = "default";

        public string? PreviewImageUrl { get; set; }

        public string? SystemPromptTemplate { get; set; } // System prompt template with substitution tokens

        public string? UserPromptTemplate { get; set; } // User prompt template with substitution tokens

        public bool IsPublic { get; set; } = false; // Can other users see this template?

        public bool IsSystem { get; set; } = false; // Built-in template vs user-created

        public string? UserId { get; set; } // Creator (null for system templates)

        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int UsageCount { get; set; } = 0; // Track popularity

        // Navigation Properties
        public virtual ICollection<Worksheet> Worksheets { get; set; } = new List<Worksheet>();
    }
}
