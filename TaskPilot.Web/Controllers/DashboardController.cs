using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(UserManager<ApplicationUser> userManager, ITaskService taskService)
        {
            _userManager = userManager;
            _taskService = taskService;
        }

        [Authorize(Policy = "CustomPolicy")]
        public async Task<IActionResult> Index()
        {
            var username = User.Identity!.Name;
            var currentUser = await _userManager.FindByNameAsync(username!);
            var userTaskList = _taskService.GetNotClosedTaskSortByCreatedDateInDescFilterByUserId(currentUser!.Id).ToList();

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
                    PriorityColorCode = task.Priority!.ColorCode,
                    StatusColorCode = task.Status!.ColorCode
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
