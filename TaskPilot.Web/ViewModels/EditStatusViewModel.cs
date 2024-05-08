using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Web.ViewModels
{
    public class EditStatusViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
