using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels.Customizer;
using WebGallery.UI.ViewModels.Single;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class CustomizerController : Controller
    {
        private readonly ITagService _tagService;
        readonly MinimalApiProxy _minimalApiProxy;
        readonly string _username;

        public CustomizerController(ITagService tagService, MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext)
        {
            _tagService = tagService;
            _minimalApiProxy = minimalApiProxy;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim.Value;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Customizer";

            List<SavedSearchDTO> searches = await _minimalApiProxy.GetSavedSearches(_username);
            SearchesViewModel vm = new ()
            {
                SavedSearches = searches
            };

            return View(vm);
        }

        [HttpPost("save-search")]
        public async Task<IActionResult> SaveSearch([FromBody] SaveSearchRequest searchDetails)
        {
            if (string.IsNullOrWhiteSpace(searchDetails.SearchName))
                return BadRequest("Search name cannot be empty.");

            var searchDto = new SavedSearchDTO
            {
                SearchName = searchDetails.SearchName,
                Albums = searchDetails.Albums,
                Tags = searchDetails.Tags,
                FileExtensions = searchDetails.FileExtensions,
                MediaNameContains = searchDetails.MediaNameContains,
                MaxSize = searchDetails.MaxSize,
                AllTagsMustMatch = searchDetails.AllTagsMustMatch
            };

            await _minimalApiProxy.SaveSearch(_username, searchDto);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Generate(CustomizerViewModel vm)
        {
            string tagFilterMode = ParseTagMode(vm.RadioTagModeOption, vm.RadioTagFilterOption);
            string tags = "";
            if(vm.SelectedTags != null)
                tags = string.Join(",", vm.SelectedTags);

            return Redirect($"/Single/Custom?nbr={vm.NumberOfPictures}&tags={tags}&tagFilterMode={tagFilterMode}&mediaFilterMode={vm.RadioMediaFilterModeOption}");
        }

        private string ParseTagMode(string tagMode, string tagModeCustomFilter)
        {
            switch (tagMode)
            {
                case "untagged":
                    return "onlyuntagged";
                case "onlytagged":
                    return "onlytagged";
                case "custom":
                    switch (tagModeCustomFilter)
                    {
                        case "inclusive":
                            return "custominclusive";
                        case "exclusive":
                            return "customexclusive";
                        default:
                            throw new ArgumentException("Unknown tag mode selected");
                    }
                default:
                    return "undefined";
            }
        }

        [HttpDelete("delete-saved-search")]
        public async Task<IActionResult> DeleteSavedSearch(string searchName)
        {
            if (string.IsNullOrWhiteSpace(searchName))
                return BadRequest("Search name required.");
            await _minimalApiProxy.DeleteSavedSearch(_username, searchName);
            return Ok();
        }
    }
}
