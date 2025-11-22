using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS.Web.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }

        [Required]
        public int ClaimId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FileType { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadDate { get; set; } = DateTime.Now;

        [ForeignKey("ClaimId")]
        public virtual Claim? Claim { get; set; }

        public string FileSizeFormatted
        {
            get
            {
                if (FileSize < 1024)
                    return $"{FileSize} B";
                else if (FileSize < 1024 * 1024)
                    return $"{FileSize / 1024.0:F2} KB";
                else
                    return $"{FileSize / (1024.0 * 1024.0):F2} MB";
            }
        }

        public string FileIcon => FileType.ToLower() switch
        {
            ".pdf" => "bi bi-file-pdf-fill text-danger",
            ".docx" or ".doc" => "bi bi-file-word-fill text-primary",
            ".xlsx" or ".xls" => "bi bi-file-excel-fill text-success",
            ".png" or ".jpg" or ".jpeg" => "bi bi-file-image-fill text-info",
            _ => "bi bi-file-earmark-fill text-secondary"
        };
    }
}