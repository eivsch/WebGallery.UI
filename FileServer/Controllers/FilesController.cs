using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebGallery.FileServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<FilesController> _logger;
        private readonly string _rootPath;
        private readonly string _certPath;
        private readonly string _ffmpegPath = "ffmpeg";

        private bool _useEncryption;

        public FilesController(ILogger<FilesController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _useEncryption = configuration.GetValue("UseEncryption", false);

            _rootPath = configuration.GetValue("ConnectionStrings:FileSystemRoot", "");
            _certPath = configuration.GetValue("EncryptionCertificate", "");
            _ffmpegPath = configuration.GetValue("FfmpegPath", "ffmpeg");


            logger.LogInformation("Using root path: {RootPath}", _rootPath);
            if (!Path.Exists(_ffmpegPath)) throw new FileNotFoundException("FFmpeg executable not found.", _ffmpegPath);
        }

        [HttpGet("image")]
        public async Task<IActionResult> DownloadImage(string file)
        {
            var userRootPath = ResolveUserRootPath();

            var base64EncodedBytes = System.Convert.FromBase64String(file);
            var appPath = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            appPath = UnifyAppPath(appPath);

            var path = Path.Combine(userRootPath, appPath);

            byte[] fileBytes;
            if (_useEncryption)
            {
                using var decryptedFileStream = await Decrypter.Decrypt(path, _certPath);
                fileBytes = decryptedFileStream.ToArray();
            }
            else
            {
                fileBytes = System.IO.File.ReadAllBytes(path);
            }

            return new FileContentResult(fileBytes, "image/jpeg");
        }

        [HttpGet("video")]
        public async Task<IActionResult> DownloadVideo(string file)
        {
            var userRootPath = ResolveUserRootPath();

            var base64EncodedBytes = Convert.FromBase64String(file);
            var appPath = Encoding.UTF8.GetString(base64EncodedBytes);
            appPath = UnifyAppPath(appPath);

            var path = Path.Combine(userRootPath, appPath);

            if (_useEncryption)
            {
                var decryptedFileStream = await Decrypter.DecryptToFileStream(path, _certPath);

                // Works only if decryptedFileStream is seekable and has a known length.
                return File(decryptedFileStream, "video/mp4", enableRangeProcessing: true);
            }

            var fileStream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                FileOptions.Asynchronous);

            return File(fileStream, "video/mp4", enableRangeProcessing: true);
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            var userRootPath = ResolveUserRootPath();
            FileUploadResponse response = null;

            foreach (var file in Request.Form.Files)
            {
                string filename = Path.GetFileName(file.FileName);
                string folder = file.Name;

                var dir = Path.Combine(userRootPath, folder);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var filePath = Path.Combine(dir, filename);

                // Override 'filePath' and 'filename' if a file with the same name already exists
                if (System.IO.File.Exists(filePath))
                {
                    filename = HandleExistingFilename(dir, filename);
                    filePath = Path.Combine(dir, filename);
                }

                using (var fileStream = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fileStream);

                    response = new FileUploadResponse
                    {
                        FileName = filename,
                        FilePathFull = filePath,
                        FileSize = fileStream.Length
                    };
                }

                if (filename.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    // Generate a thumbnail for the video
                    string thumbnailDir = Path.Combine(dir, "thumbs");
                    if (!Directory.Exists(thumbnailDir)) Directory.CreateDirectory(thumbnailDir);
                    string thumbnailPath = Path.Combine(thumbnailDir, $"{Path.GetFileNameWithoutExtension(filename)}.jpg");
                    try
                    {
                        await GenerateVideoThumbnailAsync(filePath, thumbnailPath);
                        response.ThumbnailPath = thumbnailPath;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to generate video thumbnail for {FilePath}", filePath);
                    }
                }

                if (_useEncryption)
                    await Encrypter.Encrypt(filePath, _certPath);
            }

            return Ok(response);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(string file)
        {
            var userRootPath = ResolveUserRootPath();

            var base64EncodedBytes = Convert.FromBase64String(file);
            var appPath = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            appPath = UnifyAppPath(appPath);

            var filePath = Path.Combine(userRootPath, appPath);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File does not exist.");

            var deletedDir = Path.Combine(userRootPath, "deleted");
            if (!Directory.Exists(deletedDir))
                Directory.CreateDirectory(deletedDir);

            var fileName = Path.GetFileName(filePath);
            var deletedFilePath = Path.Combine(deletedDir, fileName);

            // Ensure unique name in deleted folder
            int count = 1;
            string nameOnly = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            while (System.IO.File.Exists(deletedFilePath))
            {
                deletedFilePath = Path.Combine(deletedDir, $"{nameOnly}_{count}{extension}");
                count++;
            }

            System.IO.File.Move(filePath, deletedFilePath);

            return Ok(new { DeletedPath = deletedFilePath });
        }

        public class RenameFolderRequest
        {
            public string OldName { get; set; }
            public string NewName { get; set; }
        }

        [HttpPost("rename-folder")]
        public IActionResult Rename([FromBody] RenameFolderRequest request)
        {
            string userRootPath = ResolveUserRootPath();
            string oldPath = Path.Combine(userRootPath, request.OldName);
            string newPath = Path.Combine(userRootPath, request.NewName);
            Directory.Move(oldPath, newPath);

            return Ok();
        }

        public class MergeFoldersRequest
        {
            public string TargetFolder { get; set; }
            public List<string> SourceFolders { get; set; }
        }

        [HttpPost("merge-folders")]
        public IActionResult MergeFolders(MergeFoldersRequest request)
        {
            string userRootPath = ResolveUserRootPath();
            string targetFolder = Path.Combine(userRootPath, request.TargetFolder);

            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

            foreach (var sourceFolderName in request.SourceFolders)
            {
                string sourceFolder = Path.Combine(userRootPath, sourceFolderName);
                if (!Directory.Exists(sourceFolder)) continue;

                if (string.Equals(sourceFolder, targetFolder, StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (string sourceFile in Directory.EnumerateFiles(sourceFolder))
                {
                    string filename = Path.GetFileName(sourceFile);
                    string destinationFile = Path.Combine(targetFolder, filename);
                    if (System.IO.File.Exists(destinationFile))
                    {
                        filename = HandleExistingFilename(targetFolder, filename);
                        destinationFile = Path.Combine(targetFolder, filename);
                    }

                    System.IO.File.Move(sourceFile, destinationFile, false);
                }

                Directory.Delete(sourceFolder, true);
            }

            return Ok();
        }

        public class ThumbnailRequest
        {
            public string File { get; set; }
            public string SeekTime { get; set; } = "00:00:01.000";
        }

        [HttpPost("generate-thumbnail")]
        public async Task<IActionResult> GenerateThumbnail([FromBody] ThumbnailRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.File))
                return BadRequest("Missing file parameter.");

            var userRootPath = ResolveUserRootPath();

            var base64EncodedBytes = Convert.FromBase64String(request.File);
            var appPath = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            appPath = UnifyAppPath(appPath);

            var filePath = Path.Combine(userRootPath, appPath);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File does not exist.");

            var thumbnailDir = Path.Combine(Path.GetDirectoryName(filePath), "thumbs");
            if (!Directory.Exists(thumbnailDir))
                Directory.CreateDirectory(thumbnailDir);

            var thumbnailPath = Path.Combine(
                thumbnailDir,
                $"{Path.GetFileNameWithoutExtension(filePath)}.jpg"
            );

            try
            {
                await GenerateVideoThumbnailAsync(filePath, thumbnailPath, request.SeekTime ?? "00:00:01");
                return Ok(new { ThumbnailPath = thumbnailPath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate video thumbnail for {FilePath} at {SeekTime}", filePath, request.SeekTime);
                return StatusCode(500, "Failed to generate thumbnail.");
            }
        }

        [HttpPost("generate-video-image")]
        public async Task<IActionResult> GenerateVideoImage([FromBody] ThumbnailRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.File))
                return BadRequest("Missing file parameter.");

            var userRootPath = ResolveUserRootPath();

            var base64EncodedBytes = Convert.FromBase64String(request.File);
            var appPath = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            appPath = UnifyAppPath(appPath);

            var filePath = Path.Combine(userRootPath, appPath);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File does not exist.");

            var videoDirectory = Path.GetDirectoryName(filePath);
            var videoDirectoryName = new DirectoryInfo(videoDirectory).Name;

            // Place the images directory directly under the user folder
            var videoImagesDir = Path.Combine(userRootPath, $"{videoDirectoryName}_video_images");
            if (!Directory.Exists(videoImagesDir))
                Directory.CreateDirectory(videoImagesDir);

            var baseName = Path.GetFileNameWithoutExtension(filePath);
            var extension = ".jpg";
            string seekTimeStr = request.SeekTime.Replace(":", "");
            string imagePath = Path.Combine(videoImagesDir, $"{baseName}_{seekTimeStr}{extension}");

            try
            {
                await GenerateVideoThumbnailAsync(filePath, imagePath, request.SeekTime ?? "00:00:01");
                var fileInfo = new FileInfo(imagePath);

                return Ok(new
                {
                    FileName = Path.GetFileName(imagePath),
                    FilePathFull = imagePath,
                    FileSize = fileInfo.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate video image for {FilePath} at {SeekTime}", filePath, request.SeekTime);
                return StatusCode(500, "Failed to generate video image.");
            }
        }

        public record MoveFileRequest(string SourceFolder, string TargetFolder, string FileName, string NewFileName = null);

        [HttpPost("move")]
        public IActionResult MoveFile([FromBody] MoveFileRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            var userRootPath = ResolveUserRootPath();
            var destinationFileName = string.IsNullOrWhiteSpace(request.NewFileName)
                ? request.FileName
                : request.NewFileName;
            var sourcePath = Path.Combine(userRootPath, request.SourceFolder, request.FileName);
            var destinationPath = Path.Combine(userRootPath, request.TargetFolder, destinationFileName);
            var isVideo = Path.GetExtension(request.FileName).Equals(".mp4", StringComparison.OrdinalIgnoreCase);

            if (!System.IO.File.Exists(sourcePath))
                return NotFound("Source file does not exist.");

            try
            {
                if (!Directory.Exists(Path.Combine(userRootPath, request.TargetFolder)))
                    Directory.CreateDirectory(Path.Combine(userRootPath, request.TargetFolder));

                System.IO.File.Move(sourcePath, destinationPath);

                if (isVideo)
                {
                    var sourceThumbnailFileName = $"{Path.GetFileNameWithoutExtension(request.FileName)}.jpg";
                    var destinationThumbnailFileName = $"{Path.GetFileNameWithoutExtension(destinationFileName)}.jpg";
                    var sourceThumbnailPath = Path.Combine(userRootPath, request.SourceFolder, "thumbs", sourceThumbnailFileName);
                    var destinationThumbnailDirectory = Path.Combine(userRootPath, request.TargetFolder, "thumbs");
                    var destinationThumbnailPath = Path.Combine(destinationThumbnailDirectory, destinationThumbnailFileName);

                    if (System.IO.File.Exists(sourceThumbnailPath))
                    {
                        if (!Directory.Exists(destinationThumbnailDirectory))
                            Directory.CreateDirectory(destinationThumbnailDirectory);

                        System.IO.File.Move(sourceThumbnailPath, destinationThumbnailPath);
                    }
                }

                return Ok(new { Message = "File moved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move file from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);
                return StatusCode(500, "Failed to move file.");
            }
        }

        private string UnifyAppPath(string appPath)
        {
            if (Path.DirectorySeparatorChar == '/')
                appPath = appPath.Replace('\\', '/');
            else
                appPath = appPath.Replace('/', '\\');

            return appPath;
        }

        private string ResolveUserRootPath()
        {
            if (Request.Headers.ContainsKey("Gallery-User"))
            {
                var userId = Request.Headers["Gallery-User"];

                return Path.Combine(_rootPath, userId);
            }

            return _rootPath;
        }

        private async Task<string> GenerateVideoThumbnailAsync(string videoPath, string thumbnailPath, string seekTime = "00:00:01.000")
        {
            // Example: ffmpeg -y -ss 00:00:01 -i input.mp4 -frames:v 1 -q:v 2 output.jpg
            // -y is to overwrite output file if it exists
            var args = $"-y -ss {seekTime} -i \"{videoPath}\" -frames:v 1 -q:v 2 \"{thumbnailPath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception($"FFmpeg failed: {output}");

            return thumbnailPath;
        }
        
        private static string HandleExistingFilename(string directoryPath, string filename)
        {
            string namePart = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);

            int count = 1;
            string candidate;
            do
            {
                candidate = $"{namePart}_{count}{extension}";
                count++;
            }
            while (System.IO.File.Exists(Path.Combine(directoryPath, candidate)));

            return candidate;
        }
    }
}