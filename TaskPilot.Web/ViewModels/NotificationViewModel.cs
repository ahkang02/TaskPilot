using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.ViewModels
{
    public class NotificationViewModel
    {
        public Guid Id { get; set; }

        public required string Title { get; set; }

        public required string Status { get; set; }

        public Guid? taskId { get; set; }

        public DateTime CreatedAt { get; set; }

        public required ApplicationUser User { get; set; }
    }
}
