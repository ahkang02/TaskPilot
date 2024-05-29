using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Web.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string? Email { get; set; }
    }

}
