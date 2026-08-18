using BLL.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly string[] _defaultImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".svg" };

        public FileService(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subFolder, string[]? allowedExtensions = null, long maxSizeBytes = 5 * 1024 * 1024)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف المرفوع فارغ أو غير موجود.");

            if (file.Length > maxSizeBytes)
                throw new ArgumentException($"حجم الملف يتجاوز الحد الأقصى المسموح به ({(maxSizeBytes / (1024 * 1024))} ميجابايت).");

            var allowed = allowedExtensions ?? _defaultImageExtensions;
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowed.Contains(extension))
                throw new ArgumentException($"نوع الملف ({extension}) غير مسموح به. الأنواع المسموحة: {string.Join(", ", allowed)}");

            // مسار مجلد wwwroot
            string webRootPath = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            string targetFolder = Path.Combine(webRootPath, "uploads", subFolder);
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            // توليد اسم ملف فريد لمنع التصادم
            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
            string fullPath = Path.Combine(targetFolder, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // المسار النسبي للتخزين في قاعدة البيانات
            return $"uploads/{subFolder}/{uniqueFileName}".Replace("\\", "/");
        }

        public bool DeleteFile(string? relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(relativeFilePath))
                return false;

            try
            {
                string webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string fullPath = Path.Combine(webRootPath, relativeFilePath.TrimStart('/').Replace("/", "\\"));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
            }
            catch
            {
                // تجاهل أخطاء الحذف في حال كان الملف قيد الاستخدام
            }

            return false;
        }

        public string GetFullUrl(string? relativeFilePath, string? schemeAndHost = null)
        {
            if (string.IsNullOrWhiteSpace(relativeFilePath))
                return string.Empty;

            // إذا كان المسار بالفعل رابط ويب كامل خارجي
            if (relativeFilePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                relativeFilePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return relativeFilePath;
            }

            string host = schemeAndHost ?? string.Empty;
            if (string.IsNullOrEmpty(host) && _httpContextAccessor.HttpContext != null)
            {
                var req = _httpContextAccessor.HttpContext.Request;
                host = $"{req.Scheme}://{req.Host}";
            }

            string cleanRelative = relativeFilePath.TrimStart('/').Replace("\\", "/");
            return string.IsNullOrEmpty(host) ? $"/{cleanRelative}" : $"{host}/{cleanRelative}";
        }
    }
}