using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using TaskManagementApp.DAL;
using TaskManagementApp.ViewModels;

namespace TaskManagementApp.Controllers
{
    public class NotificationController : Controller
    {
        private NotificationRepository _notificationRepository;

        public NotificationController()
        {
            _notificationRepository = new NotificationRepository(new TaskContext());
        }

        public ActionResult Index()
        {
            var currentUser = User.Identity.GetUserId();
            var notification = _notificationRepository.GetAll().Where(x => x.UserId == currentUser && x.Status == "New").OrderByDescending(u => u.CreatedAt);

            List<NotificationViewModels> viewModel = new List<NotificationViewModels>();
            foreach (var notif in notification)
            {
                viewModel.Add(new NotificationViewModels
                {
                    Id = notif.Id,
                    Status = notif.Status,
                    taskId = notif.TasksId,
                    Title = notif.Description,
                    CreatedAt = notif.CreatedAt,
                    User = notif.User
                });
            }

            return View(viewModel);
        }

        public ActionResult UpdateStatusRead(Guid Id, Guid? taskId)
        {
            var notif = _notificationRepository.GetById(Id);
            _notificationRepository.Delete(notif);
            _notificationRepository.Save();
            _notificationRepository.Dispose();

            if (taskId == null)
            {
                return Redirect(Request.UrlReferrer.ToString());

            }
            else
            {
                return RedirectToAction("Detail", "Task", new { id = taskId });
            }
        }
    }
}