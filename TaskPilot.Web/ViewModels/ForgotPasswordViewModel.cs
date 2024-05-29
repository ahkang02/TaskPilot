using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Web.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
