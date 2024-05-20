using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Web.ViewModels
{
    public class EditUserViewModel
    {
        public string? UserId { get; set; }

        public string? UserName { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Password & Confirm Password Not Match")]
        public string? ConfirmPassword { get; set; }

        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Your password must be have at least 8 character long,  1 uppercase, 1 lowercase, 1 number & 1 special character")]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string LastName { get; set; }
    }
}
