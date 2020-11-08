﻿using Application.Services.Interfaces;
using Application.Tags;
using AutoMapper;
using DomainModel.Aggregates.Tags;
using DomainModel.Aggregates.Tags.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IMapper _mapper;

        public TagService(ITagRepository tagRepository, IMapper mapper)
        {
            _tagRepository = tagRepository;
            _mapper = mapper;
        }

        public async Task Add(TagRequest request)
        {
            Tag tag = Tag.Create(tagName: request.TagName);
            tag.AddMediaItem(request.PictureId, globalIndex: request.PictureIndex);

            await _tagRepository.Save(tag);
        }

        public async Task<IEnumerable<TagResponse>> GetAll()
        {
            var tags = await _tagRepository.GetAll();

            return tags.Select(s => _mapper.Map<TagResponse>(s));
        }
    }
}
