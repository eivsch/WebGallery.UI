using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Application.Enums;
using Application.Uploads;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Interfaces
{
    public interface IFileService
    {
        // Adds files to given folder if exists, or creates new one
        Task UploadFiles(string folderName, IEnumerable<IFormFile> folderFiles);
        Task UploadFile(string folderName, string fileName, Stream file);
        UploadResponse GetUploadRequestResult();
        
        Task<byte[]> DownloadFile(string identifier, MediaType mediaType);  
    }
}