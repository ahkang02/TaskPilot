 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    public class DashboardController : Controller
    {
        private IUnitOfWork _unitOfWork;
        private UserManager<ApplicationUser> _userManager;

        public DashboardController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [Authorize(Policy = "CustomPolicy")]
        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim!.Value);
            var userTaskList = _unitOfWork.Tasks.GetAllInclude(u => u.AssignToId == currentUser.Id && u.Status!.Description != "Closed", "Status,Priority,AssignFrom,AssignTo").OrderByDescending(u => u.Created).ToList();

            Tasks? overDueTask = null;
            List<TaskDetailViewModel> taskDetail = new List<TaskDetailViewModel>();

            int dueDayRemaining = 0;

            if (userTaskList.Count > 0)
            {
                overDueTask = userTaskList.Where(u => u.DueDate!.Value.Day >= DateTime.Now.Day && u.Status!.Description != "Closed").OrderBy(u => u.DueDate).FirstOrDefault();
                if (overDueTask == null)
                {
                    overDueTask = null;
                    dueDayRemaining = 0;
                }
                else
                {
                    dueDayRemaining = (overDueTask.DueDate!.Value.Date - DateTime.Now.Date).Days;
                }

            }

            foreach (var task in userTaskList)
            {
                var userRole = await _userManager.GetRolesAsync(task.AssignFrom!);

                taskDetail.Add(new TaskDetailViewModel
                {
                    Id = task.Id,
                    Name = task.Name,
                    Description = task.Description,
                    DueDate = task.DueDate,
                    CreatedDate = task.Created,
                    Priority = task.Priority!.Description,
                    Status = task.Status!.Description,
                    AssignTo = task.AssignTo!.UserName!,
                    AssignFrom = task.AssignFrom!.UserName!,
                    AssignFromRole = userRole[0],
                });
            }

            DashboardViewModel viewModel = new DashboardViewModel
            {
                UserTaskList = taskDetail,
                OverDueTask = overDueTask!,
                dayLeftDue = dueDayRemaining
            };

            return View(viewModel);
        }
    }
}
