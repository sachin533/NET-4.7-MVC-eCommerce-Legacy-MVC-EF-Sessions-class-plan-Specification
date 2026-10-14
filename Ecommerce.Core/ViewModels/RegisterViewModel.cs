using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Registration form. Password rules mirror ApplicationUserManager's Identity validators.
    /// </summary>
    public class RegisterViewModel
    {
        [Required]
        [StringLength(150)]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
