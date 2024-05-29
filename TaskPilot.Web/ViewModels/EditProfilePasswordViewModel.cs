using System.ComponentModel.DataAnnotations;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.ViewModels
{
    public class EditProfilePasswordViewModel
    {
        public required string Id { get; set; }

        [Required]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Your password must be have at least 8 character long,  1 uppercase, 1 lowercase, 1 number & 1 special character")]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [Compare(nameof(NewPassword), ErrorMessage = "New Password & Confirm Password Not Match")]
        public string? ConfirmPassword { get; set; }

        public List<Permission>? UserPermissions;
    }
}
