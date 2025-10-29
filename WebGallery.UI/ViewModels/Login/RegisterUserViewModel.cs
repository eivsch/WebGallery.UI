using System.ComponentModel.DataAnnotations;

namespace WebGallery.UI.ViewModels.Login
{
    public class RegisterUserViewModel
    {
        [Required(ErrorMessage = "Your username cannot be empty.")]
        [Display(Name = "Username", Prompt = "Create a username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Your password cannot be empty.")]
        [Display(Name = "Password", Prompt = "Create a password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
