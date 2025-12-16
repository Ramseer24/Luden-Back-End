using Microsoft.AspNetCore.Http;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveAudioFileAsync(IFormFile file);
        Task<string> SaveImageFileAsync(IFormFile file);
        Task<string> SaveVideoFileAsync(IFormFile file);
        Task<string> SaveImageFileAsync(Stream fileStream, string fileName, string category = "image");
        Task<byte[]> GetFile(string relativePath);
        Task<bool> DeleteFile(string relativePath);
        string GetPublicUrl(string storagePath);
    }
}

