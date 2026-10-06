using System.Security.Cryptography;
using System.Text;

namespace MinimalGallery.API;

static class Globals
{
    public const string FILE_EXT = "dat";

    public static string StoragePath {get;set;} = "not set";
}