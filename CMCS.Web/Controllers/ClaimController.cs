using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CMCS.Web.Models;
using CMCS.Web.Models.ViewModels;
using CMCS.Web.Services;

namespace CMCS.Web.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class ClaimController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly IDocumentService _documentService;
        private readonly UserManager<User> _userManager;

        public ClaimController(
            IClaimService claimService,
            IDocumentService documentService,
            UserManager<User> userManager)
        {
            _claimService = claimService;
            _documentService = documentService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Submit()
        {
            return View(new SubmitClaimViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitClaimViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Create claim with automated calculation
                var claim = await _claimService.CreateClaimAsync(
                    user.Id,
                    model.HoursWorked,
                    model.HourlyRate,
                    model.Notes);

                // Upload documents if provided
                if (model.Documents != null && model.Documents.Any())
                {
                    await _documentService.UploadDocumentsAsync(claim.ClaimId, model.Documents);
                }

                TempData["SuccessMessage"] = "Claim submitted successfully!";
                return RedirectToAction("Details", new { id = claim.ClaimId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> History(string? status = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var claims = await _claimService.GetClaimsByLecturerAsync(user.Id, status);

            var viewModel = new ClaimListViewModel
            {
                Claims = claims,
                StatusFilter = status,
                TotalClaims = claims.Count,
                PendingClaims = claims.Count(c => c.Status == "Pending"),
                TotalApprovedAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount)
            };

            return View(viewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var canApprove = User.IsInRole("Coordinator") || User.IsInRole("Manager");
            var canReject = canApprove;

            var viewModel = new ClaimDetailsViewModel
            {
                Claim = claim,
                CanApprove = canApprove && claim.Status == "Pending",
                CanReject = canReject && claim.Status == "Pending"
            };

            return View(viewModel);
        }

        // Auto-calculation endpoint for AJAX
        [HttpPost]
        public async Task<IActionResult> CalculateTotal([FromBody] CalculationRequest request)
        {
            try
            {
                var total = await _claimService.CalculateTotalAmountAsync(
                    request.HoursWorked,
                    request.HourlyRate);

                return Json(new { success = true, total });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class CalculationRequest
    {
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
    }
}