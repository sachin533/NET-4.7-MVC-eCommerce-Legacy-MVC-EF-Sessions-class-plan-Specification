using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// One node of the sidebar category tree (recursive).
    /// </summary>
    public class CategoryTreeNodeViewModel
    {
        public CategoryTreeNodeViewModel()
        {
            Children = new List<CategoryTreeNodeViewModel>();
        }

        public int Id { get; set; }

        [Display(Name = "Category")]
        public string Name { get; set; }

        public string Slug { get; set; }
        public int ProductCount { get; set; }
        public bool IsActive { get; set; }
        public IList<CategoryTreeNodeViewModel> Children { get; set; }
    }
}
