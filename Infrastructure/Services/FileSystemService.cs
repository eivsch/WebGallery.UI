using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.Common;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class FileSystemService : IFileSystemService
    {
        private readonly HttpClient _client;
        private readonly string _rootPath;
        private readonly string _fileServerUrl;
        
        public FileSystemService(IConfiguration configuration, WebGalleryFileServerClient fileServerClient)
        {
            _rootPath = configuration.GetValue("ConnectionStrings:FileServerRoot", "");
            if (string.IsNullOrEmpty(_rootPath))
                throw new Exception("Missing file system root path. Check configuration.");

            _fileServerUrl = configuration.GetConnectionString("FileServerUrl");
            _client = fileServerClient.Client;
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
    }
}