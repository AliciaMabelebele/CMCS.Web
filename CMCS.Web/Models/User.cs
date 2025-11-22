using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CMCS.Web.Models
{
    public class User : IdentityUser<int>
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Lecturer"; // Lecturer, Coordinator, Manager, HR

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

        // Full name property for display
        public string FullName => $"{FirstName} {LastName}";
    }
}