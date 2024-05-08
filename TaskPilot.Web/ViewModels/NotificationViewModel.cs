using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.ViewModels
{
    public class NotificationViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Status { get; set; }

        public Guid? taskId { get; set; }

        public DateTime CreatedAt { get; set; }

        public ApplicationUser User { get; set; }
    }
}
