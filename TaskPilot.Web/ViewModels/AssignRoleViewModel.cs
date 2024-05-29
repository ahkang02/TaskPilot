using System.ComponentModel.DataAnnotations;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.ViewModels
{
    public class AssignRoleViewModel
    {
        [Display(Name = "New Role")]
        public string? RoleId { get; set; }

        [Display(Name = "User")]
        public string? Username { get; set; }

        [Display(Name = "Current User Role")]
        public string? CurrentUserRole { get; set; }

        public List<ApplicationRole>? RoleToSelect { get; set; }
    }
}
