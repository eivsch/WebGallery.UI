using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Interfaces
{
    public interface IUploadService
    {
        // Adds files to given folder if exists, or creates new one
        Task UploadFiles(string folderName, IEnumerable<IFormFile> folderFiles);
    }
}