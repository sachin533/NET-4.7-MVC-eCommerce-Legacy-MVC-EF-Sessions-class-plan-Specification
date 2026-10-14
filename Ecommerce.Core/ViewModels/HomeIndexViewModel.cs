using System.Collections.Generic;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Home page: hero categories + featured product cards.
    /// </summary>
    public class HomeIndexViewModel
    {
        public HomeIndexViewModel()
        {
            Featured = new List<ProductCardViewModel>();
            Categories = new List<CategoryTreeNodeViewModel>();
        }

        public IList<ProductCardViewModel> Featured { get; set; }
        public IList<CategoryTreeNodeViewModel> Categories { get; set; }
    }
}
