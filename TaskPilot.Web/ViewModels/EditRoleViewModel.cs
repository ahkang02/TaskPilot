using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Web.ViewModels
{
    public class EditRoleViewModel
    {
        public string? Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
