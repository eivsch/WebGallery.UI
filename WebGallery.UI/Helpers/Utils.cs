using Application.Enums;
using System.IO;
using System.Text;

namespace WebGallery.UI.Helpers
{
    public static class Utils
    {
        public static MediaType DetermineMediaType(string name)
        {
            string ext = Path.GetExtension(name);
            switch (ext)
            {
                case ".mp4":
                    return MediaType.Video;
                case ".gif":
                    return MediaType.Gif;
                default:
                    return MediaType.Image;
            }
        }

        public const string ThumbnailFolder = "thumbs";

        /// <summary>
        /// Returns the base64-encoded thumbnail path for a given media file path.
        /// </summary>
        public static string GetThumbsBase64(string appPath)
        {
            string thumbsName = Path.GetFileNameWithoutExtension(appPath) + ".jpg";
            string thumbsDir = Path.GetDirectoryName(appPath);
            string thumbsPath = Path.Combine(thumbsDir, ThumbnailFolder, thumbsName);
            byte[] thumbsPathBytes = Encoding.UTF8.GetBytes(thumbsPath);
            return System.Convert.ToBase64String(thumbsPathBytes);
        }
    }
}
