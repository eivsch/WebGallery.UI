using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public interface IFileSystemService
    {
        // Adds files to given folder on the local file system if exists, or creates new one
        Task CopyFilesToDisk(string folderName, IEnumerable<IFormFile> folderFiles);
    }
}