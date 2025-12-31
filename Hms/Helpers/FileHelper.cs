using Hms.Extension;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hms.Helpers
{
    public static class FileHelper
    {
        public static string GenerateUniqueFileName(string originalFileName)
        {
            var fileExtension = Path.GetExtension(originalFileName);
            var uniqueId = Guid.NewGuid().GenerateUniqueId();
            return $"{uniqueId}{fileExtension}";
        }

        public static bool IsImageFile(string fileName)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return imageExtensions.Contains(extension);
        }

        public static bool IsValidFileSize(IFormFile file, long maxSizeInBytes)
        {
            return file.Length <= maxSizeInBytes;
        }
    }
}

