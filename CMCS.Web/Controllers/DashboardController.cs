using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CMCS.Web.Data;
using CMCS.Web.Models;
using CMCS.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (user.Role == "Lecturer")
            {
                return await LecturerDashboard(user);
            }
            else if (user.Role == "Coordinator" || user.Role == "Manager")
            {
                return await ReviewerDashboard(user);
            }
            else if (user.Role == "HR")
            {
                return RedirectToAction("Index", "HR");
            }

            return View();
        }

        private async Task<IActionResult> LecturerDashboard(User user)
        {
            var claims = await _context.Claims
                .Where(c => c.LecturerId == user.Id)
                .OrderByDescending(c => c.SubmissionDate)
                .Take(5)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                CurrentUser = user,
                TotalClaims = await _context.Claims.CountAsync(c => c.LecturerId == user.Id),
                PendingClaims = await _context.Claims.CountAsync(c => c.LecturerId == user.Id && c.Status == "Pending"),
                ApprovedClaims = await _context.Claims.CountAsync(c => c.LecturerId == user.Id && c.Status == "Approved"),
                RejectedClaims = await _context.Claims.CountAsync(c => c.LecturerId == user.Id && c.Status == "Rejected"),
                TotalApprovedAmount = await _context.Claims
                    .Where(c => c.LecturerId == user.Id && c.Status == "Approved")
                    .SumAsync(c => c.TotalAmount),
                RecentClaims = claims
            };

            return View("LecturerDashboard", viewModel);
        }

        private async Task<IActionResult> ReviewerDashboard(User user)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var viewModel = new ReviewDashboardViewModel
            {
                CurrentUser = user,
                PendingClaimsCount = await _context.Claims.CountAsync(c => c.Status == "Pending"),
                ApprovedThisMonth = await _context.Claims.CountAsync(c =>
                    c.Status == "Approved" &&
                    c.ApprovalDate.HasValue &&
                    c.ApprovalDate.Value.Month == currentMonth &&
                    c.ApprovalDate.Value.Year == currentYear),
                RejectedThisMonth = await _context.Claims.CountAsync(c =>
                    c.Status == "Rejected" &&
                    c.ApprovalDate.HasValue &&
                    c.ApprovalDate.Value.Month == currentMonth &&
                    c.ApprovalDate.Value.Year == currentYear),
                TotalAmountPending = await _context.Claims
                    .Where(c => c.Status == "Pending")
                    .SumAsync(c => c.TotalAmount),
                PendingClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Where(c => c.Status == "Pending")
                    .OrderBy(c => c.SubmissionDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View("ReviewerDashboard", viewModel);
        }
    }
}