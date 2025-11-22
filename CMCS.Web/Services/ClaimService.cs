using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CMCS.Web.Data;
using CMCS.Web.Models;

namespace CMCS.Web.Services
{
    public interface IClaimService
    {
        Task<Claim> CreateClaimAsync(int lecturerId, decimal hoursWorked, decimal hourlyRate, string notes);
        Task<Claim?> GetClaimByIdAsync(int claimId);
        Task<List<Claim>> GetClaimsByLecturerAsync(int lecturerId, string? statusFilter = null);
        Task<List<Claim>> GetPendingClaimsAsync();
        Task<List<Claim>> GetAllClaimsAsync(string? statusFilter = null);
        Task<bool> ApproveClaimAsync(int claimId, string approvedBy);
        Task<bool> RejectClaimAsync(int claimId, string rejectedBy, string reason);
        Task<bool> ValidateClaimAsync(Claim claim);
        Task<decimal> CalculateTotalAmountAsync(decimal hoursWorked, decimal hourlyRate);
    }

    public class ClaimService : IClaimService
    {
        private readonly ApplicationDbContext _context;

        public ClaimService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Claim> CreateClaimAsync(int lecturerId, decimal hoursWorked, decimal hourlyRate, string notes)
        {
            var totalAmount = await CalculateTotalAmountAsync(hoursWorked, hourlyRate);

            var claim = new Claim
            {
                LecturerId = lecturerId,
                SubmissionDate = DateTime.Now,
                HoursWorked = hoursWorked,
                HourlyRate = hourlyRate,
                TotalAmount = totalAmount,
                Status = "Pending",
                Notes = notes
            };

            if (!await ValidateClaimAsync(claim))
            {
                throw new InvalidOperationException("Claim validation failed");
            }

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return claim;
        }

        public async Task<Claim?> GetClaimByIdAsync(int claimId)
        {
            return await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);
        }

        public async Task<List<Claim>> GetClaimsByLecturerAsync(int lecturerId, string? statusFilter = null)
        {
            var query = _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.LecturerId == lecturerId);

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                query = query.Where(c => c.Status == statusFilter);
            }

            return await query
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetPendingClaimsAsync()
        {
            return await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.Documents)
                .Where(c => c.Status == "Pending")
                .OrderBy(c => c.SubmissionDate)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetAllClaimsAsync(string? statusFilter = null)
        {
            var query = _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.Documents)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                query = query.Where(c => c.Status == statusFilter);
            }

            return await query
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();
        }

        public async Task<bool> ApproveClaimAsync(int claimId, string approvedBy)
        {
            var claim = await GetClaimByIdAsync(claimId);
            if (claim == null || claim.Status != "Pending")
                return false;

            claim.Status = "Approved";
            claim.ApprovalDate = DateTime.Now;
            claim.ApprovedBy = approvedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectClaimAsync(int claimId, string rejectedBy, string reason)
        {
            var claim = await GetClaimByIdAsync(claimId);
            if (claim == null || claim.Status != "Pending")
                return false;

            claim.Status = "Rejected";
            claim.ApprovalDate = DateTime.Now;
            claim.ApprovedBy = rejectedBy;
            claim.RejectionReason = reason;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateClaimAsync(Claim claim)
        {
            if (claim.HoursWorked <= 0 || claim.HoursWorked > 744)
                return false;

            if (claim.HourlyRate <= 0 || claim.HourlyRate > 10000)
                return false;

            if (string.IsNullOrWhiteSpace(claim.Notes))
                return false;

            var lecturer = await _context.Users.FindAsync(claim.LecturerId);
            if (lecturer == null)
                return false;

            return await Task.FromResult(true);
        }

        public async Task<decimal> CalculateTotalAmountAsync(decimal hoursWorked, decimal hourlyRate)
        {
            var total = hoursWorked * hourlyRate;

            const decimal maxClaimAmount = 50000; 
            if (total > maxClaimAmount)
            {
                throw new InvalidOperationException($"Claim amount exceeds maximum allowed: R{maxClaimAmount:N2}");
            }

            return await Task.FromResult(total);
        }
    }
}