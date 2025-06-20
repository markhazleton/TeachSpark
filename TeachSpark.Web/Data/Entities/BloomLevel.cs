using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Data.Entities
{
    public class BloomLevel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; } = string.Empty; // Remember, Understand, Apply, Analyze, Evaluate, Create

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int Order { get; set; } // 1-6 for hierarchy

        [Required]
        public string ActionVerbs { get; set; } = string.Empty; // Comma-separated action verbs

        public string? Examples { get; set; }

        [StringLength(7)]
        public string ColorCode { get; set; } = "#6c757d"; // Hex color for UI

        // Navigation Properties
        public virtual ICollection<Worksheet> Worksheets { get; set; } = new List<Worksheet>();
    }
}
