using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Web.ViewModels
{

    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public string? returnUrl { get; set; }
    }

}
