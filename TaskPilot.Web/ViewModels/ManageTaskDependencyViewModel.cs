using System.ComponentModel.DataAnnotations;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.ViewModels
{
    public class ManageTaskDependencyViewModel
    {
        [Display(Name = "Current Task")]
        public string CurrentTask { get; set; }

        public List<Tasks>? ListOfTasks { get; set; }

        [Display(Name = "Assign Dependency")]
        public Guid? DependencyID { get; set; }
    }
}
