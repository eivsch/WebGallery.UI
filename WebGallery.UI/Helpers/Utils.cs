using Application.Enums;
using System.IO;

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
    }
}
