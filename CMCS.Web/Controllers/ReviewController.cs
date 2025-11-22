using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CMCS.Web.Models;
using CMCS.Web.Services;

namespace CMCS.Web.Controllers
{
    [Authorize(Roles = "Coordinator,Manager")]
    public class ReviewController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly UserManager<User> _userManager;

        public ReviewController(IClaimService claimService, UserManager<User> userManager)
        {
            _claimService = claimService; 
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Pending()
        {
            try
            {
                var pendingClaims = await _claimService.GetPendingClaimsAsync();
                return View(pendingClaims);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading pending claims: {ex.Message}";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _claimService.ApproveClaimAsync(id, user.FullName);

            if (result)
            {
                TempData["SuccessMessage"] = "Claim approved successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve claim.";
            }

            return RedirectToAction(nameof(Pending));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Please provide a rejection reason.";
                return RedirectToAction("Details", "Claim", new { id });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _claimService.RejectClaimAsync(id, user.FullName, reason);

            if (result)
            {
                TempData["SuccessMessage"] = "Claim rejected successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject claim.";
            }

            return RedirectToAction(nameof(Pending));
        }
    }
}