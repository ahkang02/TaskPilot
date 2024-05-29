using System.ComponentModel.DataAnnotations;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.ViewModels
{
    public class EditContactViewModel
    {
        [RegularExpression("^(601)[0|1|2|3|4|6|7|8|9]\\-*[0-9]{7,8}$", ErrorMessage = "Phone number should be in 601234567890 format")]
        [Display(Name = "Phone Number")]
        [Required]
        [Phone]
        public required string PhoneNumber { get; set; }

        public required List<Permission> UserPermissions;
    }
}
