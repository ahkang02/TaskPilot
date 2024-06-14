using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Web.ViewModels
{
    public class ResendConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
