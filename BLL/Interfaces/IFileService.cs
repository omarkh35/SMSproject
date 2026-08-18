using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string subFolder, string[]? allowedExtensions = null, long maxSizeBytes = 5 * 1024 * 1024);
        bool DeleteFile(string? relativeFilePath);
        string GetFullUrl(string? relativeFilePath, string? schemeAndHost = null);
    }
}