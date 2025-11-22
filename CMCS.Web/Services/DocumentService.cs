using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using CMCS.Web.Data;
using CMCS.Web.Models;

namespace CMCS.Web.Services
{
    public interface IDocumentService
    {
        Task<List<Document>> UploadDocumentsAsync(int claimId, List<IFormFile> files);
        Task<Document?> GetDocumentByIdAsync(int documentId);
        Task<bool> DeleteDocumentAsync(int documentId);
        bool ValidateFile(IFormFile file);
        string GetUploadsPath();
    }

    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadsPath;
        private const long MaxFileSize = 5 * 1024 * 1024;
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx", ".png", ".jpg", ".jpeg" };

        public DocumentService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _uploadsPath = Path.Combine(environment.WebRootPath, "uploads");

            if (!Directory.Exists(_uploadsPath))
            {
                Directory.CreateDirectory(_uploadsPath);
            }
        }

        public async Task<List<Document>> UploadDocumentsAsync(int claimId, List<IFormFile> files)
        {
            var documents = new List<Document>();

            foreach (var file in files)
            {
                if (!ValidateFile(file))
                    continue;

                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadsPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var document = new Document
                {
                    ClaimId = claimId,
                    FileName = file.FileName,
                    FileType = fileExtension,
                    FileSize = file.Length,
                    FilePath = filePath,
                    UploadDate = DateTime.Now
                };

                _context.Documents.Add(document);
                documents.Add(document);
            }

            await _context.SaveChangesAsync();
            return documents;
        }

        public async Task<Document?> GetDocumentByIdAsync(int documentId)
        {
            return await _context.Documents.FindAsync(documentId);
        }

        public async Task<bool> DeleteDocumentAsync(int documentId)
        {
            var document = await GetDocumentByIdAsync(documentId);
            if (document == null)
                return false;

            if (File.Exists(document.FilePath))
            {
                File.Delete(document.FilePath);
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return true;
        }

        public bool ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > MaxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
                return false;

            return true;
        }

        public string GetUploadsPath()
        {
            return _uploadsPath;
        }
    }
}