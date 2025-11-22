using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS.Web.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        public int LecturerId { get; set; }

        [Required]
        [Display(Name = "Submission Date")]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        [Required]
        [Range(0.01, 744, ErrorMessage = "Hours worked must be between 0.01 and 744")]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "Hourly rate must be between R0.01 and R10,000")]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "Approval Date")]
        public DateTime? ApprovalDate { get; set; }

        [MaxLength(200)]
        [Display(Name = "Approved By")]
        public string? ApprovedBy { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }

        [ForeignKey("LecturerId")]
        public virtual User? Lecturer { get; set; }

        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

        public string StatusBadgeClass => Status switch
        {
            "Approved" => "badge bg-success",
            "Rejected" => "badge bg-danger",
            "Pending" => "badge bg-warning text-dark",
            _ => "badge bg-secondary"
        };
    }
}