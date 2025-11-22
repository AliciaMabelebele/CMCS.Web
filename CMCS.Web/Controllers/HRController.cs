using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS.Web.Data;
using CMCS.Web.Models.ViewModels;

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
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var viewModel = new HRDashboardViewModel
            {
                TotalLecturers = await _context.Users.CountAsync(u => u.Role == "Lecturer"),
                TotalClaims = await _context.Claims.CountAsync(),
                TotalPaymentsThisMonth = await _context.Claims
                    .Where(c => c.Status == "Approved" &&
                           c.ApprovalDate.HasValue &&
                           c.ApprovalDate.Value.Month == currentMonth &&
                           c.ApprovalDate.Value.Year == currentYear)
                    .SumAsync(c => c.TotalAmount),
                TotalPaymentsAllTime = await _context.Claims
                    .Where(c => c.Status == "Approved")
                    .SumAsync(c => c.TotalAmount),
                RecentApprovedClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Where(c => c.Status == "Approved")
                    .OrderByDescending(c => c.ApprovalDate)
                    .Take(10)
                    .ToListAsync()
            };

            // Get lecturer summaries
            viewModel.LecturerSummaries = await _context.Users
                .Where(u => u.Role == "Lecturer")
                .Select(u => new LecturerSummary
                {
                    LecturerId = u.Id,
                    LecturerName = u.FirstName + " " + u.LastName,
                    Department = u.Department,
                    TotalClaims = u.Claims.Count,
                    TotalAmount = u.Claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount),
                    PendingClaims = u.Claims.Count(c => c.Status == "Pending")
                })
                .ToListAsync();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateInvoice(DateTime? fromDate, DateTime? toDate)
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
                TotalAmount = approvedClaims.Sum(c => c.TotalAmount),
                GeneratedBy = User.Identity?.Name ?? "System",
                GeneratedDate = DateTime.Now
            };

            return View(viewModel);
        }
    }
}