using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Data.Entities
{
    public class ApiUsage
    {
        public int Id { get; set; }

        [Required]
        public int ApiKeyId { get; set; }
        public virtual ApiKey ApiKey { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Endpoint { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string HttpMethod { get; set; } = string.Empty;

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public int ResponseStatusCode { get; set; }

        public TimeSpan ResponseTime { get; set; }

        public int? TokensUsed { get; set; }

        public decimal? CostIncurred { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public string? ErrorMessage { get; set; }
    }
}
