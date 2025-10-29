using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Application.Uploads;

namespace Application.Services.Interfaces
{
    public interface IFileService
    {
        Task UploadFile(string folderName, string fileName, Stream file);
        UploadResponse GetUploadRequestResult();
        
        //Task<byte[]> DownloadFile(string identifier, MediaType mediaType);  
    }
}