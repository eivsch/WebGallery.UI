using Application.Enums;

namespace Application.Galleries
{
    public sealed class GalleryPicture
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public MediaType MediaType { get; set; }
    }
}
