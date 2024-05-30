using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Common.Interfaces;

namespace TaskPilot.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UpdateStatusRead(Guid Id, Guid? taskId)
        {
            var notif = _unitOfWork.Notification.Get(n => n.Id == Id);
            _unitOfWork.Notification.Remove(notif);
            _unitOfWork.Save();

            if (taskId == null)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }else
            {
                return RedirectToAction("Detail", "Task", new { id = taskId });
            }
        }
    }
}
