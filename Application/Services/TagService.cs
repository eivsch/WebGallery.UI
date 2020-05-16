using Application.Services.Interfaces;
using Application.Tags;
using DomainModel.Aggregates.Tags;
using DomainModel.Aggregates.Tags.Interfaces;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task Add(TagRequest request)
        {
            Tag tag = Tag.Create(
                tagName: request.TagName, 
                pictureId: request.PictureId, 
                pictureIndex: request.PictureIndex
            );

            await _tagRepository.Save(tag);
        }
    }
}
