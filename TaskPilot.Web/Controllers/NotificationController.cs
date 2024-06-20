using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UpdateStatusRead(Guid Id, Guid? taskId)
        {
            var notif = _notificationService.GetNotificationById(Id);
            _notificationService.DeleteNotification(notif);

            if (taskId == null)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }else
            {
                return RedirectToAction("Detail", "Task", new { id = taskId });
            }
        }

        public IActionResult ReadAll()
        {
            var username = User.Identity!.Name;
            var currentUser = _userManager.Users.First(u => u.UserName == username);
            var notifs = _notificationService.GetNotificationByUserId(currentUser.Id);
            _notificationService.DeleteAllNotification(notifs);
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
