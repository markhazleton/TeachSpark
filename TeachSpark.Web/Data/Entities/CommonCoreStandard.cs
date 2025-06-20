using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Data.Entities
{
    public class CommonCoreStandard
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty; // e.g., "CCSS.ELA-LITERACY.RL.8.1"

        [Required]
        [StringLength(10)]
        public string Grade { get; set; } = string.Empty; // e.g., "8"

        [Required]
        [StringLength(50)]
        public string Subject { get; set; } = string.Empty; // e.g., "English Language Arts"

        [Required]
        [StringLength(20)]
        public string Domain { get; set; } = string.Empty; // e.g., "Reading Literature", "Writing"

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; } // e.g., "Key Ideas and Details"

        public string? ExampleActivities { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }

        // Navigation Properties
        public virtual ICollection<Worksheet> Worksheets { get; set; } = new List<Worksheet>();
    }
}
