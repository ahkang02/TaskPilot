using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Your password must be have at least 8 character long,  1 uppercase, 1 lowercase, 1 number & 1 special character")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password & Confirm Password Not Match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

}
