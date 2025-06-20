using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Services.Models
{
    public class WorksheetTemplateViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string TemplateType { get; set; } = string.Empty;

        [Required]
        public string LayoutJson { get; set; } = string.Empty;

        public string? PreviewImageUrl { get; set; }

        public bool IsPublic { get; set; } = false;

        public bool IsSystem { get; set; } = false;

        public string? UserId { get; set; }

        public string? UserDisplayName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public class CreateWorksheetTemplateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string TemplateType { get; set; } = string.Empty;

        [Required]
        public string LayoutJson { get; set; } = string.Empty;

        public string? PreviewImageUrl { get; set; }

        public bool IsPublic { get; set; } = false;

        public bool IsSystem { get; set; } = false;
    }

    public class UpdateWorksheetTemplateViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string TemplateType { get; set; } = string.Empty;

        [Required]
        public string LayoutJson { get; set; } = string.Empty;

        public string? PreviewImageUrl { get; set; }

        public bool IsPublic { get; set; } = false;

        public bool IsSystem { get; set; } = false;
    }
}
