using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationViewComponent(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IViewComponentResult Invoke()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            List<NotificationViewModel> viewModel = new List<NotificationViewModel>();

            if (claim != null)
            {

                var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);
                var notifInUser = _unitOfWork.Notification.GetAllInclude(n => n.User == currentUser && n.Status == "New").OrderByDescending(u => u.CreatedAt);

                foreach (var notification in notifInUser)
                {
                    viewModel.Add(new NotificationViewModel
                    {
                        Id = notification.Id,
                        Status = notification.Status,
                        taskId = notification.TasksId,
                        Title = notification.Description,
                        CreatedAt = notification.CreatedAt,
                        User = notification.User!
                    });
                }
            }
            return View(viewModel);
        }

    }
}
