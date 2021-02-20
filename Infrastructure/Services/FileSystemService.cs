using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class FileSystemService : IFileSystemService
    {

        private readonly string _rootPath;
        private List<SavedFileInfo> _savedFiles;
        
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

        public async Task<SavedFileInfo> CopyFileToDisk(string folderName, string fileName, Stream stream)
        {
            var newDir = Path.Combine(_rootPath, folderName);
            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);

            var filePath = Path.Combine(newDir, fileName);
            using (var fileStream = File.Create(filePath))
            {
                await stream.CopyToAsync(fileStream);

                return new SavedFileInfo
                    {
                        FileName = fileName,
                        FilePathFull = filePath,
                        FileSize = stream.Length
                    };
            }
        }

        public async Task<byte[]> DownloadImageFromFileServer(string imageIdentifier)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                
                using (var client = new HttpClient(httpClientHandler))
                {
                    var response = await client.GetAsync("https://localhost:5001/files?file=pic1.jpg");
                    
                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
        }

        // TODO: Handle HttpClient and cert validation in a better way
        public async Task<SavedFileInfo> UploadFileToFileServer(string albumname, string filename, Stream file)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                
                using (var client = new HttpClient(httpClientHandler))
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(file), albumname, filename);

                    // TODO: Handle file server connection string
                    using (var message = await client.PostAsync("https://localhost:5001/files", content))
                    {
                        var input = await message.Content.ReadAsStringAsync();

                        // TODO:
                        return new SavedFileInfo
                        {
                            FileName = "test",
                            FilePathFull = "test",
                            FileSize = 1
                        };
                    }
                }
            }
        }
    }
}