using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void CreateNotification(Notifications notification)
        {
            _unitOfWork.Notification.Add(notification);
            _unitOfWork.Save();
        }

        public void DeleteAllNotification(IEnumerable<Notifications> notifications)
        {
            _unitOfWork.Notification.RemoveRange(notifications);
            _unitOfWork.Save();
        }

        public void DeleteNotification(Notifications notifications)
        {
            _unitOfWork.Notification.Remove(notifications);
            _unitOfWork.Save();
        }

        public IEnumerable<Notifications> GetAllNotifications()
        {
            return _unitOfWork.Notification.GetAll();
        }

        public Notifications GetNotificationById(Guid Id)
        {
            return _unitOfWork.Notification.Get(n => n.Id == Id);
        }

        public IEnumerable<Notifications> GetNotificationByUserId(string userId)
        {
            return _unitOfWork.Notification.Find(n => n.UserId == userId);
        }

        public IEnumerable<Notifications> GetNotificationsByTaskId(Guid Id)
        {
           return _unitOfWork.Notification.Find(n => n.TasksId == Id);
        }
    }
}
