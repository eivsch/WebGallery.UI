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
            var tags = await _tagService.GetAll();

            var vm = new CustomizerViewModel
            {
                Tags = tags.Select(t => new Models.Tag { Name = t }).ToList()
            };

            return View(vm);
        }

        // TODO: Create service method "GenerateUrl" or something
        [HttpPost]
        public IActionResult Generate(CustomizerViewModel vm)
        {
            string tags = string.Join(",", vm.SelectedTags);
            string tagMode = ParseTagMode(vm);

            return Redirect($"/SingleRandom/Custom?num={vm.NumberOfPictures}&tags={tags}&tagMode={tagMode}");
        }

        private string ParseTagMode(CustomizerViewModel vm)
        {
            if (vm.RadioTagModeOption == "custom")
            {
                switch (vm.RadioTagFilterOption)
                {
                    case "inclusive":
                        return "custominclusive";
                    case "exclusive":
                        return "customexclusive";
                    default:
                        throw new ArgumentException("Unkown tag mode selected");
                }
            }

            switch (vm.RadioTagModeOption)
            {
                case "untagged":
                    return "onlyuntagged";
                case "onlytagged":
                    return "onlytagged";
                default:
                    return "undefined";
            }
        }
    }
}
