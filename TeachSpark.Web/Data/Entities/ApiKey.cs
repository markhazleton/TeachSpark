using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Data.Entities
{
    public class ApiKey
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // User-friendly name

        [Required]
        [StringLength(64)]
        public string KeyValue { get; set; } = string.Empty; // The actual API key (hashed)

        [Required]
        [StringLength(32)]
        public string KeyPrefix { get; set; } = string.Empty; // First 8 chars for display

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public DateTime? LastUsedAt { get; set; }

        public int RequestCount { get; set; } = 0;

        public int DailyRequestLimit { get; set; } = 1000;

        public int MonthlyRequestLimit { get; set; } = 10000;

        public string? AllowedIpAddresses { get; set; } // JSON array

        public string? Scopes { get; set; } // JSON array of allowed operations

        // Navigation Properties
        public virtual ICollection<ApiUsage> ApiUsages { get; set; } = new List<ApiUsage>();
    }
}
