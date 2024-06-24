using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Interface
{
    public interface INotificationService
    {
        IEnumerable<Notifications> GetAllNotifications();

        Notifications GetNotificationById(Guid Id);

        IEnumerable<Notifications> GetNotificationByUserId(string userId);

        void CreateNotification(Notifications notification);

        void DeleteNotification(Notifications notifications);

        void DeleteAllNotification(IEnumerable<Notifications> notifications);

    }
}
