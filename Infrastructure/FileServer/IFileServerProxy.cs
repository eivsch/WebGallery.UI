using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Infrastructure.FileServer
{
    public interface IFileServerProxy
    {
        Task<SavedFileInfo> UploadFileToFileServer(string albumname, string filename, Stream file);
        Task<byte[]> DownloadImageFromFileServer(string imageIdentifier);
        Task<byte[]> DownloadVideoFromFileServer(string videoIdentifier);
        Task DeleteFileFromFileServer(string albumName, string fileName);
        Task GenerateVideoThumbnailAsync(string appPathBase64, string seekTime = "00:00:01.000");
        Task<SavedFileInfo> GenerateVideoImageAsync(string appPathB64, string seekTime = "00:00:01.000");
        Task MergeFolders(string targetFolder, List<string> sourceFolders);
    }
}