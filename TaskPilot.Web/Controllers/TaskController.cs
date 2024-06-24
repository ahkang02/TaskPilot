using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Implementation;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    [Authorize(Policy = "CustomPolicy")]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IUserPermissionService _userPermissionService;
        private readonly INotificationService _notificationService;
        private readonly IStatusService _statusService;
        private readonly IPriorityService _priorityService;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;

        public TaskController(UserManager<ApplicationUser> userManager, BlobServiceClient blobServiceClient, IUserPermissionService userPermissionService, ITaskService taskService, INotificationService notificationService, IStatusService statusService, IPriorityService priorityService)
        {
            _userManager = userManager;
            _blobServiceClient = blobServiceClient;
            _containerClient = _blobServiceClient.GetBlobContainerClient("file-container");
            _userPermissionService = userPermissionService;
            _taskService = taskService;
            _notificationService = notificationService;
            _statusService = statusService;
            _priorityService = priorityService;
        }
        private async Task<ApplicationUser?> GetCurrentUser()
        {
            var username = User.Identity!.Name;
            return await _userManager.FindByNameAsync(username!) ?? null;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            UserPermissionViewModel viewModel = new UserPermissionViewModel
            {
                UserPermissions = _userPermissionService.GetUserPermission(claimsIdentity).ToList()
            };
            return View(viewModel);
        }

        public IActionResult Detail(Guid Id)
        {
            var task = _taskService.GetTasksWithId(Id);
            TaskDetailViewModel viewModel = new TaskDetailViewModel
            {
                Id = task.Id,
                AssignFrom = task.AssignFrom!.UserName!,
                AssignTo = task.AssignTo!.UserName!,
                CreatedDate = task.Created,
                Description = task.Description,
                Name = task.Name,
                DueDate = task.DueDate,
                Priority = task.Priority!.Description,
                Status = task.Status!.Description,
                PriorityColorCode = task.Priority!.ColorCode,
                StatusColorCode = task.Status!.ColorCode,
            };

            return View(viewModel);
        }

        public IActionResult New()
        {
            EditTaskViewModel viewModel = new EditTaskViewModel
            {
                PriorityList = _priorityService.GetAllPriority().ToList(),
                StatusList = _statusService.GetAllStatuses().ToList(),
                AssigneeList = _userManager.Users.ToList(),
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(EditTaskViewModel viewModel)
        {
            var currentUser = await GetCurrentUser();

            if (ModelState.IsValid)
            {
                if (viewModel.Id == null)
                {

                    if (!viewModel.IsRecurring)
                    {
                        Tasks task = new Tasks
                        {
                            Created = DateTime.Now,
                            AssignToId = viewModel.AssignToId!,
                            Description = viewModel.TaskDescription!,
                            DueDate = viewModel.DueDate,
                            Name = viewModel.TaskName!,
                            PriorityId = viewModel.PriorityId.GetValueOrDefault(),
                            StatusId = viewModel.StatusId.GetValueOrDefault(),
                            AssignFromId = currentUser!.Id,
                            Updated = DateTime.Now,
                        };

                        _taskService.CreateTask(task);

                        Notifications notif = new Notifications
                        {
                            CreatedAt = DateTime.Now,
                            Status = SD.NEW_NOTIF_STATUS,
                            Description = Message.TASK_NOTIF_CREATION,
                            UserId = task.AssignToId,
                            TasksId = task.Id
                        };

                        _notificationService.CreateNotification(notif);
                        TempData["SuccessMsg"] = Message.TASK_CREATION;
                    }
                    else
                    {
                        List<Tasks> tasks = GetRecurringTask(viewModel, currentUser!);
                        _taskService.AddRangeTasks(tasks);

                        foreach (var task in tasks)
                        {
                            _notificationService.CreateNotification(new Notifications
                            {
                                CreatedAt = DateTime.Now,
                                Status = SD.NEW_NOTIF_STATUS,
                                Description = Message.TASK_NOTIF_CREATION,
                                UserId = task.AssignToId,
                                TasksId = task.Id
                            });
                        }
                        TempData["SuccessMsg"] = tasks.Count + Message.TASK_CREATION_RECURR;
                    }
                }
                else
                {
                    if (viewModel.DependencyId != null)
                    {
                        var dependentTask = _taskService.GetTasksWithId(viewModel.DependencyId.Value);

                        if (dependentTask.Status!.Description != "Closed")
                        {

                            viewModel.PriorityList = _priorityService.GetAllPriority().ToList();
                            viewModel.StatusList = _statusService.GetAllStatuses().ToList();
                            viewModel.AssigneeList = _userManager.Users.ToList();

                            TempData["ErrorMsg"] = Message.TASK_UPDATE_FAIL;
                            return View(viewModel);
                        }
                        else
                        {
                            var taskToEdit = _taskService.GetTasksWithId(viewModel.Id.Value);
                            taskToEdit.PriorityId = viewModel.PriorityId.GetValueOrDefault();
                            taskToEdit.Name = viewModel.TaskName!;
                            taskToEdit.StatusId = viewModel.StatusId.GetValueOrDefault();
                            taskToEdit.AssignToId = viewModel.AssignToId!;
                            taskToEdit.DueDate = viewModel.DueDate;
                            taskToEdit.Description = viewModel.TaskDescription!;
                            taskToEdit.Updated = DateTime.Now;
                            _taskService.UpdateTask(taskToEdit);

                            if (viewModel.IsRecurring)
                            {
                                List<Tasks> tasks = GetRecurringTask(viewModel, currentUser!);
                                _taskService.AddRangeTasks(tasks);

                                foreach (var task in tasks)
                                {
                                    _notificationService.CreateNotification(new Notifications
                                    {
                                        CreatedAt = DateTime.Now,
                                        Status = SD.NEW_NOTIF_STATUS,
                                        Description = Message.TASK_NOTIF_CREATION,
                                        UserId = task.AssignToId,
                                        TasksId = task.Id
                                    });
                                }

                                TempData["SuccessMsg"] = "Task #" + taskToEdit.Name + Message.TASK_UPDATE + " and " + tasks.Count + Message.TASK_CREATION_RECURR;
                            }
                            else
                            {
                                Notifications notif = new Notifications
                                {
                                    CreatedAt = DateTime.Now,
                                    Status = SD.NEW_NOTIF_STATUS,
                                    Description = taskToEdit.Name + "'s has been edited",
                                    TasksId = taskToEdit.Id,
                                    UserId = taskToEdit.AssignToId!
                                };

                                _notificationService.CreateNotification(notif);
                                TempData["SuccessMsg"] = "Task #" + taskToEdit.Name + Message.TASK_UPDATE;
                            }
                        }
                    }
                    else
                    {
                        var taskToEdit = _taskService.GetTasksWithId(viewModel.Id.Value);
                        taskToEdit.PriorityId = viewModel.PriorityId.GetValueOrDefault();
                        taskToEdit.Name = viewModel.TaskName!;
                        taskToEdit.StatusId = viewModel.StatusId.GetValueOrDefault();
                        taskToEdit.AssignToId = viewModel.AssignToId!;
                        taskToEdit.DueDate = viewModel.DueDate;
                        taskToEdit.Description = viewModel.TaskDescription!;
                        taskToEdit.Updated = DateTime.Now;
                        _taskService.UpdateTask(taskToEdit);

                        if (viewModel.IsRecurring)
                        {
                            List<Tasks> tasks = GetRecurringTask(viewModel, currentUser!);
                            _taskService.AddRangeTasks(tasks);

                            foreach (var task in tasks)
                            {
                                _notificationService.CreateNotification(new Notifications
                                {
                                    CreatedAt = DateTime.Now,
                                    Status = SD.NEW_NOTIF_STATUS,
                                    Description = Message.TASK_NOTIF_CREATION,
                                    UserId = task.AssignToId,
                                    TasksId = task.Id
                                });
                            }

                            TempData["SuccessMsg"] = "Task #" + taskToEdit.Name + Message.TASK_UPDATE + " and " + tasks.Count + Message.TASK_CREATION_RECURR;
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

                            _notificationService.CreateNotification(notif);
                            TempData["SuccessMsg"] = "Task #" + taskToEdit.Name + Message.TASK_UPDATE;
                        }
                    }
                }
            }
            return RedirectToAction("Index", "Task");
        }

        public IActionResult Update(Guid Id)
        {
            var task = _taskService.GetTasksWithId(Id);
            EditTaskViewModel viewModel = new EditTaskViewModel
            {
                Id = task.Id,
                TaskName = task.Name,
                TaskDescription = task.Description,
                StatusList = _statusService.GetAllStatuses().ToList(),
                StatusId = task.StatusId,
                PriorityList = _priorityService.GetAllPriority().ToList(),
                PriorityId = task.PriorityId,
                DueDate = task.DueDate,
                AssignToId = task.AssignToId,
                AssigneeList = _userManager.Users.ToList(),
                EndDate = null,
                StartDate = null,
                DependencyId = task.DependencyId,
            };

            return View("New", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid[] taskId)
        {
            var currentUser = await GetCurrentUser();
            var taskToDelete = new List<Tasks>();

            if (taskId.Length > 0)
            {
                for (int i = 0; i < taskId.Length; i++)
                {
                    Guid Id = taskId[i];
                    taskToDelete.Add(_taskService.GetTasksWithId(Id));
                }

                foreach (var task in taskToDelete)
                {
                    var notifInTask = _notificationService.GetNotificationsByTaskId(task.Id!.Value) != null ? _notificationService.GetNotificationsByTaskId(task.Id!.Value) : null;

                    if (notifInTask != null)
                    {
                        _notificationService.DeleteAllNotification(notifInTask);
                    }

                    _notificationService.CreateNotification(new Notifications
                    {
                        CreatedAt = DateTime.Now,
                        Description = task.Name + " task's has been deleted by " + currentUser!.UserName,
                        Status = "New",
                        TasksId = null,
                        UserId = task.AssignToId
                    });

                    _taskService.DeleteTask(task);
                }
            }
            TempData["SuccessMsg"] = taskId.Length + Message.TASK_DELETION;
            return Json(Url.Action("Index", "Task"));
        }

        public IActionResult MarkAsDone(Guid Id)
        {
            var taskToUpdate = _taskService.GetTasksWithId(Id);
            var statusToUpdate = _statusService.GetStatusByName("Closed");
            if (taskToUpdate != null)
            {
                if (taskToUpdate.Status!.Description != "Closed")
                {
                    _notificationService.CreateNotification(new Notifications
                    {
                        CreatedAt = DateTime.Now,
                        Description = taskToUpdate.Name + " task's has been closed",
                        TasksId = taskToUpdate.Id,
                        Status = "New",
                        UserId = taskToUpdate.AssignToId
                    });

                    taskToUpdate.StatusId = statusToUpdate.Id;
                    taskToUpdate.Updated = DateTime.Now;
                    _taskService.UpdateTask(taskToUpdate);
                }
                else
                {
                    TempData["ErrorMsg"] = Message.TASK_ALREADY_CLOSED;
                    return RedirectToAction("Detail", "Task", new { Id = Id });
                }
            }

            TempData["SuccessMsg"] = Message.TASK_CLOSED;

            return RedirectToAction("Detail", "Task", new { Id = Id });
        }

        [HttpPost]
        public IActionResult MarkAsDone(Guid[] taskId)
        {
            var taskToUpdate = new List<Tasks>();
            var closeStatus = _statusService.GetStatusByName("Closed");

            if (taskId.Length > 0)
            {
                for (int i = 0; i < taskId.Length; i++)
                {
                    Guid Id = taskId[i];
                    _taskService.GetTasksWithId(Id!);
                }

                foreach (var task in taskToUpdate)
                {
                    if (task.Status!.Description != "Closed")
                    {

                        task.StatusId = closeStatus.Id;

                        _notificationService.CreateNotification(new Notifications
                        {
                            CreatedAt = DateTime.Now,
                            Description = task.Name + " task's has been closed",
                            TasksId = task.Id,
                            Status = "New",
                            UserId = task.AssignToId
                        });

                        task.Updated = DateTime.Now;
                        _taskService.UpdateTask(task);
                    }
                    else
                    {
                        TempData["ErrorMsg"] = Message.TASK_ALREADY_CLOSED;
                        return Json(Url.Action("Index", "Task"));
                    }
                }
            }
            TempData["SuccessMsg"] = taskId.Length + Message.TASK_CLOSED;          
            return Json(Url.Action("Index", "Task"));
        }

        public IActionResult ManageDependency(Guid Id)
        {
            var taskToAssign = _taskService.GetTasksWithId(Id);
            var listOfTask = _taskService.GetTasksFilterById(Id); ;

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
                var taskToUpdate = _taskService.GetTasksByTaskName(viewModel.CurrentTask!);
                taskToUpdate.DependencyId = viewModel.DependencyID;
                taskToUpdate.Updated = DateTime.Now;
                _taskService.UpdateTask(taskToUpdate);
                TempData["SuccessMsg"] = Message.TASK_DEPENDENCY;
                return RedirectToAction("Index", "Task");
            }
            TempData["ErrorMsg"] = Message.COMMON_ERROR;
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
        public async Task<IActionResult> ImportTask(IFormFile formFile)
        {
            ImportTaskViewModel viewModel = new ImportTaskViewModel
            {
                ImportInfo = new List<TaskImportInfo>()
            };

            if (formFile != null)
            {
                StreamReader csvReader = new StreamReader(formFile.OpenReadStream(), Encoding.UTF8);
                csvReader.ReadLine();
                while (!csvReader.EndOfStream)
                {
                    var line = csvReader.ReadLine();
                    var value = line!.Split(',');

                    var status = _statusService.GetStatusByName(value[3]) != null ? _statusService.GetStatusByName(value[3]) : null;
                    var priority = _priorityService.GetPrioritiesByName(value[2]) != null ? _priorityService.GetPrioritiesByName(value[2]) : null;
                    var assignee = await _userManager.FindByNameAsync(value[5]) != null ? await _userManager.FindByNameAsync(value[5]) : null;


                    viewModel.ImportInfo.Add(new TaskImportInfo
                    {
                        Name = value[0],
                        Description = value[1],
                        PriorityLevel = value[2],
                        Status = value[3],
                        DueDate = DateTime.Parse(value[4]).Date >= DateTime.Now.Date ? DateTime.Parse(value[4]) : DateTime.Now.Date,
                        AssignToUser = value[5],
                        AssigeeList = _userManager.Users.ToList(),
                        PriorityList = _priorityService.GetAllPriority().ToList(),
                        StatusList = _statusService.GetAllStatuses().ToList(),
                        PriorityId = priority != null ? priority.Id : null,
                        StatusId = status != null ? status.Id : null,
                        UserId = assignee != null ? assignee.Id: null,
                    });

                }
                TempData["SuccessMsg"] = Message.TASK_READ_CSV;
            }
            else
            {
                TempData["ErrorMsg"] = Message.TASK_IMPORT_FAIL;
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncToDB(ImportTaskViewModel viewModel)
        {
            var currentUser = await GetCurrentUser();
            if (ModelState.IsValid)
            {
                int i = 1;
                foreach (var item in viewModel.ImportInfo!)
                {
                    var status = _statusService.GetStatusByName(item.Status!);
                    var priority = _priorityService.GetPrioritiesByName(item.PriorityLevel!);
                    var user = await _userManager.FindByNameAsync(item.AssignToUser!);

                    var task = new Tasks
                    {
                        Name = item.Name + " - " + i,
                        StatusId = status.Id,
                        PriorityId = priority.Id,
                        DueDate = item.DueDate,
                        Description = item.Description!,
                        Created = DateTime.Now,
                        AssignToId = user.Id,
                        AssignFromId = currentUser!.Id,
                        Updated = DateTime.Now,
                    };

                    _taskService.CreateTask(task);
                    i++;
                }
                TempData["SuccessMsg"] = viewModel.ImportInfo.Count + Message.TASK_IMPORT;
                return RedirectToAction("Index", "Task");
            }
            TempData["ErrorMsg"] = Message.COMMON_ERROR;
            return RedirectToAction("ImportTask", "Task", viewModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetTemplate()
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient("ImportTemplate.csv");
                var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0;
                var contentType = blobClient.GetProperties().Value.ContentType;
                return File(memoryStream, contentType, "ImportTemplate.csv");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("ImportTask", "Task");
            }
        }

        private List<Tasks> GetRecurringTask(EditTaskViewModel viewModel, ApplicationUser currentUser)
        {
            List<Tasks> tasks = new List<Tasks>();
            switch (viewModel.RecurringType)
            {
                case "Daily":
                    for (int i = 0; i <= viewModel.FrequencyCount; i++)
                    {
                        tasks.Add(new Tasks
                        {
                            Created = DateTime.Now,
                            AssignToId = viewModel.AssignToId!,
                            Description = viewModel.TaskDescription!,
                            DueDate = viewModel.StartDate,
                            Name = viewModel.TaskName + " " + viewModel.StartDate.Value.ToString("yyyy-MM-dd"),
                            PriorityId = viewModel.PriorityId.GetValueOrDefault(),
                            StatusId = viewModel.StatusId.GetValueOrDefault(),
                            AssignFromId = currentUser.Id,
                            Updated = DateTime.Now,
                        });
                        viewModel.StartDate = viewModel.StartDate.Value.AddDays(1);
                    }
                    break;
                case "Weekly":
                    for (int i = 0; i <= viewModel.FrequencyCount; i++)
                    {
                        tasks.Add(new Tasks
                        {
                            Created = DateTime.Now,
                            AssignToId = viewModel.AssignToId!,
                            Description = viewModel.TaskDescription!,
                            DueDate = viewModel.StartDate,
                            Name = viewModel.TaskName + " " + viewModel.StartDate.Value.ToString("yyyy-MM-dd"),
                            PriorityId = viewModel.PriorityId.GetValueOrDefault(),
                            StatusId = viewModel.StatusId.GetValueOrDefault(),
                            AssignFromId = currentUser.Id,
                            Updated = DateTime.Now,
                        });
                        viewModel.StartDate = viewModel.StartDate.Value.AddDays(7);
                    }
                    break;
                case "Monthly":
                    for (int i = 0; i <= viewModel.FrequencyCount; i++)
                    {
                        tasks.Add(new Tasks
                        {
                            Created = DateTime.Now,
                            AssignToId = viewModel.AssignToId!,
                            Description = viewModel.TaskDescription!,
                            DueDate = viewModel.StartDate,
                            Name = viewModel.TaskName + " " + viewModel.StartDate.Value.ToString("yyyy-MM-dd"),
                            PriorityId = viewModel.PriorityId.GetValueOrDefault(),
                            StatusId = viewModel.StatusId.GetValueOrDefault(),
                            AssignFromId = currentUser.Id,
                            Updated = DateTime.Now,
                        });
                        viewModel.StartDate = viewModel.StartDate.Value.AddMonths(1);
                    }
                    break;
                case "Yearly":
                    for (int i = 0; i <= viewModel.FrequencyCount; i++)
                    {
                        tasks.Add(new Tasks
                        {
                            Created = DateTime.Now,
                            AssignToId = viewModel.AssignToId!,
                            Description = viewModel.TaskDescription!,
                            DueDate = viewModel.StartDate,
                            Name = viewModel.TaskName + " " + viewModel.StartDate.Value.ToString("yyyy-MM-dd"),
                            PriorityId = viewModel.PriorityId.GetValueOrDefault(),
                            StatusId = viewModel.StatusId.GetValueOrDefault(),
                            AssignFromId = currentUser.Id,
                            Updated = DateTime.Now,
                        });
                        viewModel.StartDate = viewModel.StartDate.Value.AddYears(1);
                    }
                    break;

            }

            // To Be Removed
            //switch (viewModel.RecurringType)
            //{
            //    case "Daily":
            //        for (var date = viewModel.StartDate; date!.Value.Date <= viewModel.EndDate!.Value.Date; date = date.Value.AddDays(1))
            //        {
            //            tasks.Add(new Tasks
            //            {
            //                Created = DateTime.Now,
            //                AssignToId = viewModel.AssignToId!,
            //                Description = viewModel.TaskDescription!,
            //                DueDate = date,
            //                Name = viewModel.TaskName + " " + date.Value.ToString("yyyy-MM-dd"),
            //                PriorityId = viewModel.PriorityId.GetValueOrDefault(),
            //                StatusId = viewModel.StatusId.GetValueOrDefault(),
            //                AssignFromId = currentUser.Id,
            //                Updated = DateTime.Now,
            //            });

            //        }
            //        break;
            //    case "Weekly":
            //        for (var date = viewModel.StartDate; date!.Value.Date <= viewModel.EndDate!.Value.Date; date = date.Value.AddDays(7))
            //        {
            //            tasks.Add(new Tasks
            //            {
            //                Created = DateTime.Now,
            //                AssignToId = viewModel.AssignToId!,
            //                Description = viewModel.TaskDescription!,
            //                DueDate = date,
            //                Name = viewModel.TaskName + " " + date.Value.ToString("yyyy-MM-dd"),
            //                PriorityId = viewModel.PriorityId!.Value,
            //                StatusId = viewModel.StatusId!.Value,
            //                AssignFromId = currentUser.Id,
            //                Updated = DateTime.Now,
            //            });
            //        }
            //        break;
            //    case "Monthly":
            //        for (var month = viewModel.StartDate; month!.Value.Month <= viewModel.EndDate!.Value.Month; month = month.Value.AddMonths(1))
            //        {
            //            tasks.Add(new Tasks
            //            {
            //                Created = DateTime.Now,
            //                AssignToId = viewModel.AssignToId!,
            //                Description = viewModel.TaskDescription!,
            //                DueDate = month,
            //                Name = viewModel.TaskName + " " + month.Value.ToString("MMM"),
            //                PriorityId = viewModel.PriorityId!.Value,
            //                StatusId = viewModel.StatusId!.Value,
            //                AssignFromId = currentUser.Id,
            //                Updated = DateTime.Now,
            //            });


            //        }
            //        break;
            //    case "Yearly":
            //        for (var year = viewModel.StartDate; year!.Value.Year <= viewModel.EndDate!.Value.Year; year = year.Value.AddYears(1))
            //        {
            //            tasks.Add(new Tasks
            //            {
            //                Created = DateTime.Now,
            //                AssignToId = viewModel.AssignToId!,
            //                Description = viewModel.TaskDescription!,
            //                DueDate = year,
            //                Name = viewModel.TaskName + " " + year.Value.ToString("yyyy"),
            //                PriorityId = viewModel.PriorityId!.Value,
            //                StatusId = viewModel.StatusId!.Value,
            //                AssignFromId = currentUser.Id,
            //                Updated = DateTime.Now,
            //            });

            //        }
            //        break;
            //}
            return tasks;
        }
    }
}


