using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.ViewModels
{
    public class DashboardViewModel
    {
        public List<TaskDetailViewModel>? UserTaskList { get; set; }

        public Tasks? OverDueTask { get; set; }

        public int? dayLeftDue { get; set; }
    }
}
