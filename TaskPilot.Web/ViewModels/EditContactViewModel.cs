using System.ComponentModel.DataAnnotations;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.ViewModels
{
    public class EditContactViewModel
    {
        [RegularExpression("^(?:6)?0(([0-9]{2}(([0-9]{3,4}[0-9]{4})|([0-9]{3,4}[0-9]{4})|(-[0-9]{7,8})))|([0-9]{9,10}))$", ErrorMessage = "Phone number should be in 601234567890 format")]
        [Display(Name = "Phone Number")]
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public List<Permission> UserPermissions;
    }
}
