using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels.Customizer;

namespace WebGallery.UI.Controllers
{
    [Route("[controller]")]
    public class CustomizerController : Controller
    {
        private readonly ITagService _tagService;

        public CustomizerController(ITagService tagService)
        {
            _tagService = tagService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Customizer";

            var tags = await _tagService.GetAll();

            var vm = new CustomizerViewModel
            {
                Tags = tags.Select(t => new Models.Tag { Name = t }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Generate(CustomizerViewModel vm)
        {
            string tagFilterMode = ParseTagMode(vm.RadioTagModeOption, vm.RadioTagFilterOption);
            string tags = "";
            if(vm.SelectedTags != null)
                tags = string.Join(",", vm.SelectedTags);

            return Redirect($"/SingleRandom/Custom?nbr={vm.NumberOfPictures}&tags={tags}&tagFilterMode={tagFilterMode}");
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
                            throw new ArgumentException("Unkown tag mode selected");
                    }
                default:
                    return "undefined";
            }
        }
    }
}
