using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Web.ViewModels
{
    public class EditPriorityViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
