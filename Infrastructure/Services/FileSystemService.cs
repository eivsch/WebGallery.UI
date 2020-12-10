using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class FileSystemService : IFileSystemService
    {
        private readonly string _rootPath;
        
        public FileSystemService(IConfiguration configuration)
        {
            _rootPath = configuration.GetValue("ConnectionStrings:FileServerRoot", "");
            if (string.IsNullOrEmpty(_rootPath))
                throw new Exception("Missing file system root path. Check configuration.");
        }

        public async Task CopyFileToDisk(string folderName, IFormFile folderFile)
        {
            var newDir = Path.Combine(_rootPath, folderName);
            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);

            var filePath = Path.Combine(newDir, folderFile.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await folderFile.CopyToAsync(stream);
            }
        }

        public async Task CopyFileToDisk(string folderName, string fileName, Stream stream)
        {
            var newDir = Path.Combine(_rootPath, folderName);
            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);

            var filePath = Path.Combine(newDir, fileName);
            using (var fileStream = File.Create(filePath))
            {
                await stream.CopyToAsync(fileStream);
            }
        }   
    }
}