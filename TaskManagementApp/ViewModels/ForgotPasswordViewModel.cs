using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
