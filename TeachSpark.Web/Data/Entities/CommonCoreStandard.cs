using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Data.Entities
{
    public class CommonCoreStandard
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty; // e.g., "K.RL.1", "11-12.WHST.10"

        [Required]
        [StringLength(10)]
        public string Grade { get; set; } = string.Empty; // e.g., "K", "1", "11-12"

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty; // e.g., "English Language Arts"

        [Required]
        [StringLength(50)]
        public string Domain { get; set; } = string.Empty; // e.g., "Reading Literature", "Reading Informational"

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
