using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Repository;
using TaskPilot.Web.ViewModels;
using static Vonage.ProactiveConnect.Lists.SyncStatus;

namespace TaskPilot.Web.Controllers
{
    [Authorize(Policy = "CustomPolicy")]
    public class TaskController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            UserPermissionViewModel viewModel = new UserPermissionViewModel
            {
                UserPermissions = new List<Permission>()
            };

            if (claim != null)
            {
                var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);
                var currentUserRole = await _userManager.GetRolesAsync(currentUser);
                var roles = _unitOfWork.Roles.GetAllInclude(r => r.Name == currentUserRole[0], "Permissions").Single();
                var permissions = _unitOfWork.Permissions.GetAllInclude(filter: null, includeProperties: "Features,Roles");

                foreach (var permission in permissions)
                {
                    if (permission.Roles.Any(r => r.Id == roles.Id))
                    {
                        viewModel.UserPermissions.Add(permission);
                    }
                }

            }

            return View(viewModel);
        }

        public IActionResult Detail(Guid Id)
        {
            var task = _unitOfWork.Tasks.GetAllInclude(t => t.Id == Id, "Status,Priority,AssignTo,AssignFrom").First();
            TaskDetailViewModel viewModel = new TaskDetailViewModel
            {
                Id = task.Id,
                AssignFrom = task.AssignFrom.UserName,
                AssignTo = task.AssignTo.UserName,
                CreatedDate = task.Created,
                Description = task.Description,
                Name = task.Name,
                DueDate = task.DueDate,
                Priority = task.Priority.Description,
                Status = task.Status.Description
            };

            return View(viewModel);
        }

        public IActionResult New()
        {
            EditTaskViewModel viewModel = new EditTaskViewModel
            {
                PriorityList = _unitOfWork.Priority.GetAll().ToList(),
                StatusList = _unitOfWork.Status.GetAll().ToList(),
                AssigneeList = _unitOfWork.Users.GetAll().ToList(),
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(EditTaskViewModel viewModel)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);

            if (ModelState.IsValid)
            {
                if (viewModel.Id == null)
                {

                    if (!viewModel.IsRecurring)
                    {
                        Tasks task = new Tasks
                        {
                            Created = DateTime.Now,
                            AssignToId = viewModel.AssignToId,
                            Description = viewModel.TaskDescription,
                            DueDate = viewModel.DueDate,
                            Name = viewModel.TaskName,
                            PriorityId = viewModel.PriorityId,
                            StatusId = viewModel.StatusId,
                            AssignFromId = currentUser.Id,
                        };

                        _unitOfWork.Tasks.Add(task);
                        _unitOfWork.Save();

                        Notifications notif = new Notifications
                        {
                            CreatedAt = DateTime.Now,
                            Status = "New",
                            Description = "A new task has been created for you",
                            UserId = task.AssignToId,
                            TasksId = task.Id
                        };

                        _unitOfWork.Notification.Add(notif);
                        TempData["SuccessMsg"] = "A new task has been created successfully.";
                    }
                    else
                    {
                        List<Tasks> tasks = new List<Tasks>();

                        // Different Recurring Handle Differently
                        switch (viewModel.RecurringType)
                        {
                            case "Daily":
                                for (var date = viewModel.StartDate; date.Value.Date <= viewModel.EndDate.Value.Date; date = date.Value.AddDays(1))
                                {
                                    tasks.Add(new Tasks
                                    {
                                        Created = DateTime.Now,
                                        AssignToId = viewModel.AssignToId,
                                        Description = viewModel.TaskDescription,
                                        DueDate = date,
                                        Name = viewModel.TaskName + " " + date.Value.ToString("yyyy-MM-dd"),
                                        PriorityId = viewModel.PriorityId,
                                        StatusId = viewModel.StatusId,
                                        AssignFromId = currentUser.Id,
                                    });

                                }
                                break;
                            case "Weekly":
                                for (var date = viewModel.StartDate; date.Value.Date <= viewModel.EndDate.Value.Date; date = date.Value.AddDays(7))
                                {
                                    tasks.Add(new Tasks
                                    {
                                        Created = DateTime.Now,
                                        AssignToId = viewModel.AssignToId,
                                        Description = viewModel.TaskDescription,
                                        DueDate = date,
                                        Name = viewModel.TaskName + " " + date.Value.ToString("yyyy-MM-dd"),
                                        PriorityId = viewModel.PriorityId,
                                        StatusId = viewModel.StatusId,
                                        AssignFromId = currentUser.Id,
                                    });
                                }
                                break;
                            case "Monthly":
                                for (var month = viewModel.StartDate; month.Value.Month <= viewModel.EndDate.Value.Month; month = month.Value.AddMonths(1))
                                {
                                    tasks.Add(new Tasks
                                    {
                                        Created = DateTime.Now,
                                        AssignToId = viewModel.AssignToId,
                                        Description = viewModel.TaskDescription,
                                        DueDate = month,
                                        Name = viewModel.TaskName + " " + month.Value.ToString("MMM"),
                                        PriorityId = viewModel.PriorityId,
                                        StatusId = viewModel.StatusId,
                                        AssignFromId = currentUser.Id,
                                    });


                                }
                                break;
                            case "Yearly":
                                for (var year = viewModel.StartDate; year.Value.Year <= viewModel.EndDate.Value.Year; year = year.Value.AddYears(1))
                                {
                                    tasks.Add(new Tasks
                                    {
                                        Created = DateTime.Now,
                                        AssignToId = viewModel.AssignToId,
                                        Description = viewModel.TaskDescription,
                                        DueDate = year,
                                        Name = viewModel.TaskName + " " + year.Value.ToString("yyyy"),
                                        PriorityId = viewModel.PriorityId,
                                        StatusId = viewModel.StatusId,
                                        AssignFromId = currentUser.Id,
                                    });

                                }
                                break;
                        }

                        _unitOfWork.Tasks.AddRange(tasks);
                        _unitOfWork.Save();

                        foreach (var task in tasks)
                        {
                            _unitOfWork.Notification.Add(new Notifications
                            {
                                CreatedAt = DateTime.Now,
                                Status = "New",
                                Description = "A new task has been created for you",
                                UserId = task.AssignToId,
                                TasksId = task.Id
                            });
                        }

                        TempData["SuccessMsg"] = tasks.Count + " Recurring Task's has been created successfully";
                    }
                }
                else
                {
                    if (viewModel.DependencyId != null)
                    {
                        var dependentTask = _unitOfWork.Tasks.GetAllInclude(t => t.Id == viewModel.DependencyId, includeProperties: "Status").First();

                        if (dependentTask.Status.Description != "Closed")
                        {
                            viewModel.PriorityList = _unitOfWork.Priority.GetAll().ToList();
                            viewModel.StatusList = _unitOfWork.Status.GetAll().ToList();
                            viewModel.AssigneeList = _unitOfWork.Users.GetAll().ToList();
                            TempData["ErrorMsg"] = "Oops! Dependencies Found: You cannot edit the task as the dependent task are not completed yet.";
                            return View(viewModel);
                        }
                        else
                        {
                            var taskToEdit = _unitOfWork.Tasks.Get(t => t.Id == viewModel.Id);
                            taskToEdit.PriorityId = viewModel.PriorityId;
                            taskToEdit.Name = viewModel.TaskName;
                            taskToEdit.StatusId = viewModel.StatusId;
                            taskToEdit.AssignToId = viewModel.AssignToId;
                            taskToEdit.DueDate = viewModel.DueDate;
                            taskToEdit.Description = viewModel.TaskDescription;
                            _unitOfWork.Tasks.Update(taskToEdit);

                            if (viewModel.IsRecurring)
                            {
                                List<Tasks> tasks = new List<Tasks>();

                                // Different Recurring Handle Differently
                                switch (viewModel.RecurringType)
                                {
                                    case "Daily":
                                        for (var date = viewModel.StartDate; date.Value.Date <= viewModel.EndDate.Value.Date; date = date.Value.AddDays(1))
                                        {
                                            tasks.Add(new Tasks
                                            {
                                                Created = DateTime.Now,
                                                AssignToId = viewModel.AssignToId,
                                                Description = viewModel.TaskDescription,
                                                DueDate = date,
                                                Name = viewModel.TaskName + " " + date.Value.ToString("yyyy-MM-dd"),
                                                PriorityId = viewModel.PriorityId,
                                                StatusId = viewModel.StatusId,
                                                AssignFromId = currentUser.Id,
                                            });

                                        }
                                        break;
                                    case "Weekly":
                                        for (var date = viewModel.StartDate; date.Value.Date <= viewModel.EndDate.Value.Date; date = date.Value.AddDays(7))
                                        {
                                            tasks.Add(new Tasks
                                            {
                                                Created = DateTime.Now,
                                                AssignToId = viewModel.AssignToId,
                                                Description = viewModel.TaskDescription,
                                                DueDate = date,
                                                Name = viewModel.TaskName + " " + date.Value.ToString("yyyy-MM-dd"),
                                                PriorityId = viewModel.PriorityId,
                                                StatusId = viewModel.StatusId,
                                                AssignFromId = currentUser.Id,
                                            });
                                        }
                                        break;
                                    case "Monthly":
                                        for (var month = viewModel.StartDate; month.Value.Month <= viewModel.EndDate.Value.Month; month = month.Value.AddMonths(1))
                                        {
                                            tasks.Add(new Tasks
                                            {
                                                Created = DateTime.Now,
                                                AssignToId = viewModel.AssignToId,
                                                Description = viewModel.TaskDescription,
                                                DueDate = month,
                                                Name = viewModel.TaskName + " " + month.Value.ToString("MMM"),
                                                PriorityId = viewModel.PriorityId,
                                                StatusId = viewModel.StatusId,
                                                AssignFromId = currentUser.Id,
                                            });


                                        }
                                        break;
                                    case "Yearly":
                                        for (var year = viewModel.StartDate; year.Value.Year <= viewModel.EndDate.Value.Year; year = year.Value.AddYears(1))
                                        {
                                            tasks.Add(new Tasks
                                            {
                                                Created = DateTime.Now,
                                                AssignToId = viewModel.AssignToId,
                                                Description = viewModel.TaskDescription,
                                                DueDate = year,
                                                Name = viewModel.TaskName + " " + year.Value.ToString("yyyy"),
                                                PriorityId = viewModel.PriorityId,
                                                StatusId = viewModel.StatusId,
                                                AssignFromId = currentUser.Id,
                                            });

                                        }
                                        break;
                                }

                                _unitOfWork.Tasks.AddRange(tasks);
                                _unitOfWork.Save();

                                foreach (var task in tasks)
                                {
                                    _unitOfWork.Notification.Add(new Notifications
                                    {
                                        CreatedAt = DateTime.Now,
                                        Status = "New",
                                        Description = "A new task has been created for you",
                                        UserId = task.AssignToId,
                                        TasksId = task.Id
                                    });
                                }

                                TempData["SuccessMsg"] = "Task #" + taskToEdit.Name + " has been updated and " + tasks.Count + " Recurring Task's has been created successfully";
                            }
                            else
                            {
                                Notifications notif = new Notifications
                                {
                                    CreatedAt = DateTime.Now,
                                    Status = "New",
                                    Description = taskToEdit.Name + "'s has been edited",
                                    TasksId = taskToEdit.Id,
                                    UserId = taskToEdit.AssignToId
                                };

                                _unitOfWork.Notification.Add(notif);
                                TempData["SuccessMsg"] = "Task #" + taskToEdit.Name + " has been updated";

                                _unitOfWork.Save();
                            }
                        }
                    }
                    else
                    {
                        var taskToEdit = _unitOfWork.Tasks.Get(t => t.Id == viewModel.Id);
                        taskToEdit.PriorityId = viewModel.PriorityId;
                        taskToEdit.Name = viewModel.TaskName;
                        taskToEdit.StatusId = viewModel.StatusId;
                        taskToEdit.AssignToId = viewModel.AssignToId;
                        taskToEdit.DueDate = viewModel.DueDate;
                        taskToEdit.Description = viewModel.TaskDescription;
                        _unitOfWork.Tasks.Update(taskToEdit);

                        if (viewModel.IsRecurring)
                        {
                            List<Tasks> tasks = new List<Tasks>();

                            // Different Recurring Handle Differently
                            switch (viewModel.RecurringType)
                            {
                                case "Daily":
                                    for (var date = viewModel.StartDate; date.Value.Date <= viewModel.EndDate.Value.Date; date = date.Value.AddDays(1))
                                    {
                                        tasks.Add(new Tasks
                                        {
                                            Created = DateTime.Now,
                                            AssignToId = viewModel.AssignToId,
                                            Description = viewModel.TaskDescription,
                                            DueDate = date,
                                            Name = viewModel.TaskName + " " + date.Value.ToString("yyyy-MM-dd"),
                                            PriorityId = viewModel.PriorityId,
                                            StatusId = viewModel.StatusId,
                                            AssignFromId = currentUser.Id,
                                        });

                                    }
                                    break;
                                case "Weekly":
                                    for (var date = viewModel.StartDate; date.Value.Date <= viewModel.EndDate.Value.Date; date = date.Value.AddDays(7))
                                    {
                                        tasks.Add(new Tasks
                                        {
                                            Created = DateTime.Now,
                                            AssignToId = viewModel.AssignToId,
                                            Description = viewModel.TaskDescription,
                                            DueDate = date,
                                            Name = viewModel.TaskName + " " + date.Value.ToString("yyyy-MM-dd"),
                                            PriorityId = viewModel.PriorityId,
                                            StatusId = viewModel.StatusId,
                                            AssignFromId = currentUser.Id,
                                        });
                                    }
                                    break;
                                case "Monthly":
                                    for (var month = viewModel.StartDate; month.Value.Month <= viewModel.EndDate.Value.Month; month = month.Value.AddMonths(1))
                                    {
                                        tasks.Add(new Tasks
                                        {
                                            Created = DateTime.Now,
                                            AssignToId = viewModel.AssignToId,
                                            Description = viewModel.TaskDescription,
                                            DueDate = month,
                                            Name = viewModel.TaskName + " " + month.Value.ToString("MMM"),
                                            PriorityId = viewModel.PriorityId,
                                            StatusId = viewModel.StatusId,
                                            AssignFromId = currentUser.Id,
                                        });


                                    }
                                    break;
                                case "Yearly":
                                    for (var year = viewModel.StartDate; year.Value.Year <= viewModel.EndDate.Value.Year; year = year.Value.AddYears(1))
                                    {
                                        tasks.Add(new Tasks
                                        {
                                            Created = DateTime.Now,
                                            AssignToId = viewModel.AssignToId,
                                            Description = viewModel.TaskDescription,
                                            DueDate = year,
                                            Name = viewModel.TaskName + " " + year.Value.ToString("yyyy"),
                                            PriorityId = viewModel.PriorityId,
                                            StatusId = viewModel.StatusId,
                                            AssignFromId = currentUser.Id,
                                        });

                                    }
                                    break;
                            }

                            _unitOfWork.Tasks.AddRange(tasks);
                            _unitOfWork.Save();

                            foreach (var task in tasks)
                            {
                                _unitOfWork.Notification.Add(new Notifications
                                {
                                    CreatedAt = DateTime.Now,
                                    Status = "New",
                                    Description = "A new task has been created for you",
                                    UserId = task.AssignToId,
                                    TasksId = task.Id
                                });
                            }

                            TempData["SuccessMsg"] = "Task #" + taskToEdit.Name + " has been updated and " + tasks.Count + " Recurring Task's has been created successfully";
                        }
                        else
                        {
                            Notifications notif = new Notifications
                            {
                                CreatedAt = DateTime.Now,
                                Status = "New",
                                Description = taskToEdit.Name + "'s has been edited",
                                TasksId = taskToEdit.Id,
                                UserId = taskToEdit.AssignToId
                            };

                            _unitOfWork.Notification.Add(notif);
                            TempData["SuccessMsg"] = "Task #" + taskToEdit.Name + " has been updated";
                            _unitOfWork.Save();
                        }
                    }
                }
            }
            _unitOfWork.Save();
            return RedirectToAction("Index", "Task");
        }

        public IActionResult Update(Guid Id)
        {
            var task = _unitOfWork.Tasks.Get(t => t.Id == Id);
            EditTaskViewModel viewModel = new EditTaskViewModel
            {
                Id = task.Id,
                TaskName = task.Name,
                TaskDescription = task.Description,
                StatusList = _unitOfWork.Status.GetAll().ToList(),
                StatusId = task.StatusId,
                PriorityList = _unitOfWork.Priority.GetAll().ToList(),
                PriorityId = task.PriorityId,
                DueDate = task.DueDate,
                AssignToId = task.AssignToId,
                AssigneeList = _unitOfWork.Users.GetAll().ToList(),
                EndDate = null,
                StartDate = null,
                DependencyId = task.DependencyId,
            };

            return View("New", viewModel);
        }

        [HttpPost]
        public IActionResult Delete(Guid[] taskId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);
            var taskToDelete = new List<Tasks>();


            if (taskId.Length > 0)
            {
                for (int i = 0; i < taskId.Length; i++)
                {
                    Guid Id = taskId[i];
                    taskToDelete.Add(_unitOfWork.Tasks.Get(t => t.Id == Id));
                }

                foreach (var task in taskToDelete)
                {
                    var notifInTask = _unitOfWork.Notification.GetAll().Where(t => t.TasksId == task.Id);

                    if (notifInTask != null)
                    {
                        _unitOfWork.Notification.RemoveRange(notifInTask);
                    }

                    _unitOfWork.Notification.Add(new Notifications
                    {
                        CreatedAt = DateTime.Now,
                        Description = task.Name + " task's has been deleted by " + currentUser.UserName,
                        Status = "New",
                        TasksId = null,
                        UserId = task.AssignToId
                    });

                    _unitOfWork.Tasks.Remove(task);
                }
            }
            _unitOfWork.Save();
            TempData["SuccessMsg"] = taskId.Length + " tasks has been deleted successfully.";
            return Json(Url.Action("Index", "Task"));
        }

        public IActionResult MarkAsDone(Guid Id)
        {
            var taskToUpdate = _unitOfWork.Tasks.Get(t => t.Id == Id);
            var statusToUpdate = _unitOfWork.Status.Get(s => s.Description == "Closed");
            if (taskToUpdate != null)
            {
                _unitOfWork.Notification.Add(new Notifications
                {
                    CreatedAt = DateTime.Now,
                    Description = taskToUpdate.Name + " task's has been closed",
                    TasksId = taskToUpdate.Id,
                    Status = "New",
                    UserId = taskToUpdate.AssignToId
                });

                taskToUpdate.StatusId = statusToUpdate.Id;
                _unitOfWork.Tasks.Update(taskToUpdate);
                _unitOfWork.Save();
            }

            TempData["SuccessMsg"] = "Tasks has been closed";

            return RedirectToAction("Detail", "Task", new { Id = Id });
        }

        [HttpPost]
        public IActionResult MarkAsDone(Guid[] taskId)
        {
            var taskToUpdate = new List<Tasks>();
            var closeStatus = _unitOfWork.Status.Get(s => s.Description == "Closed");

            if (taskId.Length > 0)
            {
                for (int i = 0; i < taskId.Length; i++)
                {
                    Guid Id = taskId[i];
                    taskToUpdate.Add(_unitOfWork.Tasks.Get(t => t.Id == Id));
                }

                foreach (var task in taskToUpdate)
                {
                    task.StatusId = closeStatus.Id;

                    _unitOfWork.Notification.Add(new Notifications
                    {
                        CreatedAt = DateTime.Now,
                        Description = task.Name + " task's has been closed",
                        TasksId = task.Id,
                        Status = "New",
                        UserId = task.AssignToId
                    });

                    _unitOfWork.Tasks.Update(task);
                }
            }
            _unitOfWork.Save();
            TempData["SuccessMsg"] = taskId.Length + " tasks has been closed";
            return Json(Url.Action("Index", "Task"));
        }

        public IActionResult ManageDependency(Guid Id)
        {
            var taskToAssign = _unitOfWork.Tasks.Get(t => t.Id == Id);
            var listOfTask = _unitOfWork.Tasks.Find(t => t.Id != Id);

            ManageTaskDependencyViewModel viewModel = new ManageTaskDependencyViewModel
            {
                CurrentTask = taskToAssign.Name,
                ListOfTasks = listOfTask.ToList(),
                DependencyID = taskToAssign.DependencyId,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ManageDependency(ManageTaskDependencyViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var taskToUpdate = _unitOfWork.Tasks.Get(t => t.Name == viewModel.CurrentTask);
                taskToUpdate.DependencyId = viewModel.DependencyID;
                _unitOfWork.Tasks.Update(taskToUpdate);
                _unitOfWork.Save();
                TempData["SuccessMsg"] = "Dependency assigned successfully!";
                return RedirectToAction("Index", "Task");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong. Please go thru the error message.";
            return View(viewModel);
        }

        public IActionResult ImportTask()
        {
            ImportTaskViewModel viewModel = new ImportTaskViewModel
            {
                ImportInfo = new List<TaskImportInfo>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ImportTask(IFormFile formFile)
        {
            ImportTaskViewModel viewModel = new ImportTaskViewModel
            {
                ImportInfo = new List<TaskImportInfo>()
            };

            if (formFile != null)
            {
                StreamReader csvReader = new StreamReader(formFile.OpenReadStream());
                csvReader.ReadLine();
                while (!csvReader.EndOfStream)
                {
                    var line = csvReader.ReadLine();
                    var value = line.Split(',');

                    viewModel.ImportInfo.Add(new TaskImportInfo
                    {
                        Name = value[0],
                        Description = value[1],
                        PriorityLevel = value[2],
                        Status = value[3],
                        DueDate = DateTime.Now.Date,
                        AssignToUser = value[5]
                    });

                }
                TempData["SuccessMsg"] = "Data retrieved from CSV successfully!.";
            }else
            {
                TempData["ErrorMsg"] = "Oops! something went wrong, Did you input any file?";
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SyncToDB(ImportTaskViewModel viewModel)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var currentUser = _unitOfWork.Users.Get(u => u.Id == claim.Value);
            if (ModelState.IsValid)
            {
                int i = 1;
                foreach (var item in viewModel.ImportInfo)
                {
                    var status = _unitOfWork.Status.Get(s => s.Description == item.Status);
                    var priority = _unitOfWork.Priority.Get(p => p.Description == item.PriorityLevel);
                    var user = _unitOfWork.Users.Get(u => u.UserName == item.AssignToUser);

                    var task = new Tasks
                    {
                        Name = item.Name + " - " + i,
                        StatusId = status.Id,
                        PriorityId = priority.Id,
                        DueDate = item.DueDate,
                        Description = item.Description,
                        Created = DateTime.Now,
                        AssignToId = user.Id,
                        AssignFromId = currentUser.Id
                    };

                    _unitOfWork.Tasks.Add(task);
                    i++;
                }

                _unitOfWork.Save();
                TempData["SuccessMsg"] = viewModel.ImportInfo.Count + " tasks imported successfully.";
                return RedirectToAction("Index", "Task");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return RedirectToAction("ImportTask", "Task", viewModel);
        }

    }
}
