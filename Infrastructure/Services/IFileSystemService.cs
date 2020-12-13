using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public interface IFileSystemService
    {
        // Adds file to given folder on the local file system if exists, or creates new folder
        Task CopyFileToDisk(string folderName, IFormFile folderFile);
        Task<SavedFileInfo> CopyFileToDisk(string folderName, string fileName, Stream file);
    }
}