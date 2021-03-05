using System.ComponentModel.DataAnnotations;

namespace WebGallery.UI.ViewModels.Login
{
    public class LoginFormViewModel
    {
        [Required(ErrorMessage = "Your username cannot be empty.")]
        [Display(Name = "Username", Prompt = "Your username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Your password cannot be empty.")]
        [Display(Name = "Password", Prompt = "Your password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
