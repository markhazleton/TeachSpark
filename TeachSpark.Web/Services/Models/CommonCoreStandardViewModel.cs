using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Services.Models
{
    public class CommonCoreStandardViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Grade { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Domain { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }
        public string? ExampleActivities { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }
    }

    public class CreateCommonCoreStandardViewModel
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Grade { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Domain { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        public string? ExampleActivities { get; set; }
    }

    public class UpdateCommonCoreStandardViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Grade { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Domain { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        public string? ExampleActivities { get; set; }
    }
}
