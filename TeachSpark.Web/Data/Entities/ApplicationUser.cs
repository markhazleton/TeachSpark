using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Data.Entities;

/// <summary>
/// Custom application user extending IdentityUser
/// </summary>
public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? School { get; set; }

    [StringLength(50)]
    public string? District { get; set; }

    [StringLength(20)]
    public string? GradeLevel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<Worksheet> Worksheets { get; set; } = new List<Worksheet>();
    public virtual ICollection<WorksheetTemplate> WorksheetTemplates { get; set; } = new List<WorksheetTemplate>();
    public virtual ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();

    // Computed Properties
    public string FullName => $"{FirstName} {LastName}";
}
