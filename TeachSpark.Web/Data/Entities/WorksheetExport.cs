using System.ComponentModel.DataAnnotations;

namespace TeachSpark.Web.Data.Entities
{
    public class WorksheetExport
    {
        public int Id { get; set; }

        [Required]
        public int WorksheetId { get; set; }
        public virtual Worksheet Worksheet { get; set; } = null!;

        [Required]
        [StringLength(10)]
        public string ExportFormat { get; set; } = string.Empty; // PDF, DOCX, HTML

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FileName { get; set; }

        public long FileSizeBytes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; } // For temporary files

        public bool IsTemporary { get; set; } = true;

        public int DownloadCount { get; set; } = 0;
    }
}
