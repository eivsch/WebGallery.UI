using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public interface IFileSystemService
    {
        Task<SavedFileInfo> UploadFileToFileServer(string albumname, string filename, Stream file);
        Task<byte[]> DownloadImageFromFileServer(string imageIdentifier);
        Task<byte[]> DownloadVideoFromFileServer(string videoIdentifier);
    }
}