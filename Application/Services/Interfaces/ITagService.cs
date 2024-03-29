﻿using Application.Tags;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface ITagService
    {
        public Task Add(TagRequest request);
        public Task<IEnumerable<TagResponse>> GetAll();
        Task DeleteTag(string pictureId, string tagName);
    }
}
