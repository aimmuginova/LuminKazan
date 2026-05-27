using LuminKazan.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuminKazan.ViewModels
{
    public class PortfolioItemFormViewModel
    {
        public PortfolioItem PortfolioItem { get; set; } = new PortfolioItem();

        public IFormFile? CoverImageFile { get; set; }

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> Studios { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> Photographers { get; set; } = new List<SelectListItem>();
    }
}