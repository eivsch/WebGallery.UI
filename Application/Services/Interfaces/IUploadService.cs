using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Interfaces
{
    public interface IUploadService
    {
        // Adds files to given folder if exists, or creates new one
        Task UploadFiles(string folderName, IEnumerable<IFormFile> folderFiles);
        Task UploadFile(string folderName, string fileName, int folderSortOrder, Stream file);
    }
}