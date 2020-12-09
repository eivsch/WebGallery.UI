using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public class FileSystemService : IFileSystemService
    {
        public FileSystemService()
        {
        }

        public async Task CopyFilesToDisk(string folderName, IEnumerable<IFormFile> folderFiles)
        {
            var rootPath = "/var/www/pics";
            
            var newDir = Path.Combine(rootPath, folderName);
            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);

            foreach (var file in folderFiles)
            {
                var filePath = Path.Combine(newDir, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
        }
    }
}