using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.Common;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.FileServer
{
    public class FileServerProxy : IFileServerProxy
    {
        private readonly HttpClient _client;
        private readonly string _rootPath;
        private readonly string _fileServerUrl;

        public FileServerProxy(IConfiguration configuration, WebGalleryFileServerClient fileServerClient)
        {
            _rootPath = configuration.GetValue("ConnectionStrings:FileServerRoot", "");
            if (string.IsNullOrEmpty(_rootPath))
                throw new Exception("Missing file system root path. Check configuration.");

            _fileServerUrl = configuration.GetConnectionString("FileServerUrl");
            _client = fileServerClient.Client;
        }

        public async Task DeleteFileFromFileServer(string albumName, string fileName)
        {
            // Construct the file path as on the file server
            var filePath = Path.Combine(albumName, fileName);
            var filePathBytes = System.Text.Encoding.UTF8.GetBytes(filePath);
            var filePathBase64 = System.Convert.ToBase64String(filePathBytes);

            var response = await _client.DeleteAsync($"{_fileServerUrl}/files/delete?file={filePathBase64}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to delete file. The API returned a {response.StatusCode} status code.");
            }
        }

        public async Task<byte[]> DownloadImageFromFileServer(string imageIdentifier)
        {
            var response = await _client.GetAsync($"{_fileServerUrl}/files/image?file={imageIdentifier}");

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]> DownloadVideoFromFileServer(string videoIdentifier)
        {
            var response = await _client.GetAsync($"{_fileServerUrl}/files/video?file={videoIdentifier}");

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<SavedFileInfo> UploadFileToFileServer(string albumname, string filename, Stream file)
        {
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StreamContent(file), albumname, filename);

                var response = await _client.PostAsync($"{_fileServerUrl}/files", content);
                if (response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    var responseData = await JsonSerializer.DeserializeAsync<SavedFileInfo>(responseStream);

                    return new SavedFileInfo
                    {
                        FileName = responseData.FileName,
                        FilePathFull = responseData.FilePathFull,
                        FileSize = responseData.FileSize
                    };
                }

                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }

        public async Task GenerateVideoThumbnailAsync(string appPathBase64, string seekTime = "00:00:01.000")
        {
            var request = new
            {
                File = appPathBase64,
                SeekTime = seekTime
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{_fileServerUrl}/files/generate-thumbnail", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to generate thumbnail. The API returned a {response.StatusCode} status code.");
            }
        }
        
        public async Task<SavedFileInfo> GenerateVideoImageAsync(string appPathB64, string seekTime = "00:00:01.000")
        {
            var body = new { File = appPathB64, SeekTime = seekTime };
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{_fileServerUrl}/files/generate-video-image", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to generate video image. Status: {response.StatusCode}");

            using var responseStream = await response.Content.ReadAsStreamAsync();
            var responseData = await JsonSerializer.DeserializeAsync<SavedFileInfo>(responseStream);

            return new SavedFileInfo
            {
                FileName = responseData.FileName,
                FilePathFull = responseData.FilePathFull,
                FileSize = responseData.FileSize
            };
        }

        public async Task MergeFolders(string targetFolder, List<string> sourceFolders)
        {
            if (string.IsNullOrWhiteSpace(targetFolder)) throw new ArgumentException("targetFolder is required", nameof(targetFolder));
            if (sourceFolders == null || sourceFolders.Count == 0) throw new ArgumentException("sourceFolders must contain at least one folder", nameof(sourceFolders));

            var body = new
            {
                TargetFolder = targetFolder,
                SourceFolders = sourceFolders
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{_fileServerUrl}/files/merge-folders", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to merge folders. The file server returned a {response.StatusCode} status code.");
            }
        }
    }
}