using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Login form. Identity validates credentials; this VM only shapes the view.
    /// </summary>
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
