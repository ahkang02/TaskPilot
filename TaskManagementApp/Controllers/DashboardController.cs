using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementApp.DAL;
using TaskManagementApp.Models;
using TaskManagementApp.ViewModels;
using TaskManagementApp.App_Start;

namespace TaskManagementApp.Controllers
{
    public class DashboardController : Controller
    {
        private TaskRepository _taskRepository;
        private UserStore<ApplicationUser> _userStore;
        private RoleStore<Roles> _roleStore;
        private TaskContext _taskContext;
        private UserManager<ApplicationUser> _userManager;

        public DashboardController()
        {
            _taskContext = TaskContext.Create();
            _taskRepository = new TaskRepository(_taskContext);
            _userStore = new UserStore<ApplicationUser>(_taskContext);
            _roleStore = new RoleStore<Roles>(_taskContext);
            _userManager = new UserManager<ApplicationUser>(_userStore);
        }

        public ActionResult Index()
        {
            var routeData = HttpContext.Request.RequestContext.RouteData.Values["controller"];
            var routeAction = HttpContext.Request.RequestContext.RouteData.Values["action"];
            var currentUser = User.Identity.GetUserId();
            var userTaskList = _taskRepository.GetAllInclude(includeProperties: "Status,Priority,AssignFrom,AssignTo").Where(u => u.AssignToId == currentUser && u.Status.Description != "Closed").OrderByDescending(u => u.Created).ToList();
            Tasks overDueTask = null;
            List<TaskDetailViewModel> taskDetail = new List<TaskDetailViewModel>();
            int dueDayRemaining = 0;

            if (userTaskList.Count > 0)
            {
                overDueTask = userTaskList.Where(u => u.DueDate.Value.Day >= DateTime.Now.Day && u.Status.Description != "Closed").OrderBy(u => u.DueDate).FirstOrDefault();
                if (overDueTask == null)
                {
                    overDueTask = null;
                    dueDayRemaining = 0;
                }
                else
                {
                    dueDayRemaining = (overDueTask.DueDate - DateTime.Now).Value.Days;
                }

            }

            foreach (var task in userTaskList)
            {
                taskDetail.Add(new TaskDetailViewModel
                {
                    Id = task.Id,
                    Name = task.Name,
                    Description = task.Description,
                    DueDate = task.DueDate,
                    CreatedDate = task.Created,
                    Priority = task.Priority.Description,
                    Status = task.Status.Description,
                    AssignTo = task.AssignTo.UserName,
                    AssignFrom = task.AssignFrom.UserName,
                    AssignFromRole = _userManager.GetRoles(task.AssignFromId)[0],
                });
            }

            DashboardViewModel viewModel = new DashboardViewModel
            {
                UserTaskList = taskDetail,
                OverDueTask = overDueTask,
                dayLeftDue = dueDayRemaining
            };

            return View(viewModel);
        }
    }
}