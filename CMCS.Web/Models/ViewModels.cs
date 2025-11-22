using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CMCS.Web.Models.ViewModels
{
    // Authentication ViewModels
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = "Lecturer";

        [Required(ErrorMessage = "Department is required")]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;
    }

    // Claim ViewModels
    public class SubmitClaimViewModel
    {
        [Required(ErrorMessage = "Hours worked is required")]
        [Range(0.01, 744, ErrorMessage = "Hours must be between 0.01 and 744")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(0.01, 10000, ErrorMessage = "Rate must be between R0.01 and R10,000")]
        [Display(Name = "Hourly Rate (R)")]
        public decimal HourlyRate { get; set; } = 350; // Default rate

        [Display(Name = "Total Amount")]
        public decimal TotalAmount => HoursWorked * HourlyRate;

        [Required(ErrorMessage = "Notes are required")]
        [MaxLength(1000)]
        [Display(Name = "Notes / Description")]
        public string Notes { get; set; } = string.Empty;

        [Display(Name = "Supporting Documents")]
        public List<IFormFile>? Documents { get; set; }
    }

    public class ClaimDetailsViewModel
    {
        public Claim Claim { get; set; } = null!;
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class ClaimListViewModel
    {
        public List<Claim> Claims { get; set; } = new List<Claim>();
        public string? StatusFilter { get; set; }
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public decimal TotalApprovedAmount { get; set; }
    }

    // Dashboard ViewModels
    public class DashboardViewModel
    {
        public User CurrentUser { get; set; } = null!;
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalApprovedAmount { get; set; }
        public List<Claim> RecentClaims { get; set; } = new List<Claim>();
    }

    public class ReviewDashboardViewModel
    {
        public User CurrentUser { get; set; } = null!;
        public int PendingClaimsCount { get; set; }
        public int ApprovedThisMonth { get; set; }
        public int RejectedThisMonth { get; set; }
        public decimal TotalAmountPending { get; set; }
        public List<Claim> PendingClaims { get; set; } = new List<Claim>();
    }

    // HR ViewModels
    public class HRDashboardViewModel
    {
        public int TotalLecturers { get; set; }
        public int TotalClaims { get; set; }
        public decimal TotalPaymentsThisMonth { get; set; }
        public decimal TotalPaymentsAllTime { get; set; }
        public List<Claim> RecentApprovedClaims { get; set; } = new List<Claim>();
        public List<LecturerSummary> LecturerSummaries { get; set; } = new List<LecturerSummary>();
    }

    public class LecturerSummary
    {
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int TotalClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public int PendingClaims { get; set; }
    }

    public class InvoiceViewModel
    {
        public List<Claim> ApprovedClaims { get; set; } = new List<Claim>();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
    }
}