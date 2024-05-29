using System.ComponentModel.DataAnnotations;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.ViewModels
{
    public class EditProfileViewModel
    {
        public required string Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "User Role")]
        public required string? UserRole { get; set; }

        [Display(Name = "Last Logged In")]
        public DateTime? LastLogin { get; set; }

        public List<Permission>? UserPermissions;
    }
}
