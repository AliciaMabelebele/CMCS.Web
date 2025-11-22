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

            try
            {
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

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading dashboard: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        private async Task<IActionResult> LecturerDashboard(User user)
        {
            try
            {
                var claims = await _context.Claims
                    .Where(c => c.LecturerId == user.Id)
                    .OrderByDescending(c => c.SubmissionDate)
                    .Take(5)
                    .ToListAsync();

                var allClaims = await _context.Claims
                    .Where(c => c.LecturerId == user.Id)
                    .ToListAsync();

                var approvedClaims = allClaims.Where(c => c.Status == "Approved").ToList();

                var viewModel = new DashboardViewModel
                {
                    CurrentUser = user,
                    TotalClaims = allClaims.Count,
                    PendingClaims = allClaims.Count(c => c.Status == "Pending"),
                    ApprovedClaims = allClaims.Count(c => c.Status == "Approved"),
                    RejectedClaims = allClaims.Count(c => c.Status == "Rejected"),
                    TotalApprovedAmount = approvedClaims.Any() ? approvedClaims.Sum(c => c.TotalAmount) : 0,
                    RecentClaims = claims
                };

                return View("LecturerDashboard", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        private async Task<IActionResult> ReviewerDashboard(User user)
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var pendingClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Where(c => c.Status == "Pending")
                    .OrderBy(c => c.SubmissionDate)
                    .Take(10)
                    .ToListAsync();

                var allPendingClaims = await _context.Claims
                    .Where(c => c.Status == "Pending")
                    .ToListAsync();

                var viewModel = new ReviewDashboardViewModel
                {
                    CurrentUser = user,
                    PendingClaimsCount = allPendingClaims.Count,
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
                    TotalAmountPending = allPendingClaims.Any() ? allPendingClaims.Sum(c => c.TotalAmount) : 0,
                    PendingClaims = pendingClaims
                };

                return View("ReviewerDashboard", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading reviewer dashboard: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}