using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Data.Entities;

/// <summary>
/// Represents academic standards from various educational frameworks
/// Based on Colorado Academic Standards for Reading, Writing, and Communicating
/// </summary>
public class AcademicStandard
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Grade { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Standard { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? PreparedGraduate1 { get; set; }

    [MaxLength(1000)]
    public string? PreparedGraduate2 { get; set; }

    [MaxLength(1000)]
    public string? PreparedGraduate3 { get; set; }

    [MaxLength(1000)]
    public string? PreparedGraduate4 { get; set; }

    [MaxLength(1000)]
    public string? PreparedGraduate5 { get; set; }

    [Required]
    [MaxLength(500)]
    public string GradeLevelExpectation { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string GleCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Subheading { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Statement { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties for potential future relationships
    public virtual ICollection<Worksheet> Worksheets { get; set; } = new List<Worksheet>();
}
