using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS.Web.Data;
using CMCS.Web.Models.ViewModels;
using System.Linq;

namespace CMCS.Web.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                // Get all claims and calculate on client side
                var allClaims = await _context.Claims.ToListAsync();
                var approvedClaims = allClaims.Where(c => c.Status == "Approved").ToList();

                var approvedThisMonth = approvedClaims
                    .Where(c => c.ApprovalDate.HasValue &&
                           c.ApprovalDate.Value.Month == currentMonth &&
                           c.ApprovalDate.Value.Year == currentYear)
                    .ToList();

                var recentApprovedClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Where(c => c.Status == "Approved")
                    .OrderByDescending(c => c.ApprovalDate)
                    .Take(10)
                    .ToListAsync();

                var viewModel = new HRDashboardViewModel
                {
                    TotalLecturers = await _context.Users.CountAsync(u => u.Role == "Lecturer"),
                    TotalClaims = allClaims.Count,
                    TotalPaymentsThisMonth = approvedThisMonth.Any() ? approvedThisMonth.Sum(c => c.TotalAmount) : 0,
                    TotalPaymentsAllTime = approvedClaims.Any() ? approvedClaims.Sum(c => c.TotalAmount) : 0,
                    RecentApprovedClaims = recentApprovedClaims
                };

                // Get lecturer summaries
                var lecturers = await _context.Users
                    .Where(u => u.Role == "Lecturer")
                    .Include(u => u.Claims)
                    .ToListAsync();

                viewModel.LecturerSummaries = lecturers.Select(u => new LecturerSummary
                {
                    LecturerId = u.Id,
                    LecturerName = u.FirstName + " " + u.LastName,
                    Department = u.Department,
                    TotalClaims = u.Claims.Count,
                    TotalAmount = u.Claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount),
                    PendingClaims = u.Claims.Count(c => c.Status == "Pending")
                }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading HR dashboard: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateInvoice(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-1);
                var to = toDate ?? DateTime.Now;

                var approvedClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Where(c => c.Status == "Approved" &&
                           c.ApprovalDate >= from &&
                           c.ApprovalDate <= to)
                    .OrderBy(c => c.Lecturer!.LastName)
                    .ToListAsync();

                var viewModel = new InvoiceViewModel
                {
                    ApprovedClaims = approvedClaims,
                    FromDate = from,
                    ToDate = to,
                    TotalAmount = approvedClaims.Any() ? approvedClaims.Sum(c => c.TotalAmount) : 0,
                    GeneratedBy = User.Identity?.Name ?? "System",
                    GeneratedDate = DateTime.Now
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating invoice: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}