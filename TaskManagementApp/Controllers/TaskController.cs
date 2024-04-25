using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementApp.DAL;
using TaskManagementApp.Models;
using TaskManagementApp.ViewModels;
using TaskManagementApp.App_Start;
using System.Web.UI.WebControls;
using System.Data.SqlTypes;
using System.Drawing;
using System.Threading.Tasks;
using System.Data.Entity;

namespace TaskManagementApp.Controllers
{
    [CustomAuthorize]
    public class TaskController : Controller
    {
        private TaskRepository _taskRepository;
        private StatusesRepository _statusesRepository;
        private PrioritiesRepository _prioritiesRepository;
        private NotificationRepository _notificationRepository;
        private UserStore<ApplicationUser> _userStore;
        private PermissionRepository _permissionRepository;
        private UserManager<ApplicationUser> _userManager;
        private RoleStore<Roles> _roleStore;

        public TaskController()
        {
            _taskRepository = new TaskRepository(new TaskContext());
            _statusesRepository = new StatusesRepository(new TaskContext());
            _prioritiesRepository = new PrioritiesRepository(new TaskContext());
            _notificationRepository = new NotificationRepository(new TaskContext());
            _permissionRepository = new PermissionRepository(new TaskContext());
            _userStore = new UserStore<ApplicationUser>(new TaskContext());
            _userManager = new UserManager<ApplicationUser>(_userStore);
            _roleStore = new RoleStore<Roles>(new TaskContext());
        }

        public ActionResult Index()
        {
            var currentUser = User.Identity.GetUserId();
            var currentUserRole = _userManager.GetRoles(currentUser)[0];
            var roles = _roleStore.Roles.Include("Permissions").SingleOrDefault(r => r.Name == currentUserRole);
            var permissions = _permissionRepository.GetAllInclude(includeProperties: "Features, Roles").ToList();


            UserPermissionViewModel viewModel = new UserPermissionViewModel
            {
                UserPermissions = new List<Permission>()
            };

            foreach (var permission in permissions)
            {
                if (permission.Roles.Any(r => r.Id == roles.Id))
                {
                    viewModel.UserPermissions.Add(permission);
                }
            }

            return View(viewModel);
        }

        public ActionResult Detail(Guid Id)
        {
            var task = _taskRepository.GetAllInclude(includeProperties: "Status,Priority,AssignTo,AssignFrom").SingleOrDefault(t => t.Id == Id);

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

        public ActionResult NewTask()
        {
            EditTaskViewModel viewModel = new EditTaskViewModel
            {
                PriorityList = _prioritiesRepository.GetAll().ToList(),
                StatusList = _statusesRepository.GetAll().ToList(),
                AssigneeList = _userStore.Users.ToList(),
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult NewTask(EditTaskViewModel viewModel)
        {
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
                            AssignFromId = User.Identity.GetUserId(),
                        };
                        _taskRepository.Insert(task);
                        _taskRepository.Save();
                        _taskRepository.Dispose();

                        Notifications notif = new Notifications
                        {
                            CreatedAt = DateTime.Now,
                            Status = "New",
                            Description = "A new task has been created for you",
                            UserId = task.AssignToId,
                            TasksId = task.Id
                        };

                        _notificationRepository.Insert(notif);
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
                                        AssignFromId = User.Identity.GetUserId(),
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
                                        AssignFromId = User.Identity.GetUserId(),
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
                                        AssignFromId = User.Identity.GetUserId(),
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
                                        AssignFromId = User.Identity.GetUserId(),
                                    });

                                }
                                break;
                        }

                        foreach (var task in tasks)
                        {
                            _taskRepository.Insert(task);
                        }

                        _taskRepository.Save();
                        _taskRepository.Dispose();

                        foreach (var task in tasks)
                        {
                            _notificationRepository.Insert(new Notifications
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
                        var dependentTask = _taskRepository.GetAllInclude(includeProperties: "Status").SingleOrDefault(t => t.Id == viewModel.DependencyId);

                        if (dependentTask.Status.Description != "Closed")
                        {
                            viewModel.PriorityList = _prioritiesRepository.GetAll().ToList();
                            viewModel.StatusList = _statusesRepository.GetAll().ToList();
                            viewModel.AssigneeList = _userStore.Users.ToList();
                            TempData["ErrorMsg"] = "Oops! Dependencies Found: You cannot edit the task as the dependent task are not completed yet.";
                            return View(viewModel);
                        }
                        else
                        {
                            var taskToEdit = _taskRepository.GetById(viewModel.Id);
                            taskToEdit.PriorityId = viewModel.PriorityId;
                            taskToEdit.Name = viewModel.TaskName;
                            taskToEdit.StatusId = viewModel.StatusId;
                            taskToEdit.AssignToId = viewModel.AssignToId;
                            taskToEdit.DueDate = viewModel.DueDate;
                            taskToEdit.Description = viewModel.TaskDescription;
                            _taskRepository.Update(taskToEdit);

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
                                                AssignFromId = User.Identity.GetUserId(),
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
                                                AssignFromId = User.Identity.GetUserId(),
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
                                                AssignFromId = User.Identity.GetUserId(),
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
                                                Name = viewModel.TaskName  + " " + year.Value.ToString("yyyy"),
                                                PriorityId = viewModel.PriorityId,
                                                StatusId = viewModel.StatusId,
                                                AssignFromId = User.Identity.GetUserId(),
                                            });

                                        }
                                        break;
                                }
                                foreach (var task in tasks)
                                {
                                    _taskRepository.Insert(task);
                                }

                                _taskRepository.Save();
                                _taskRepository.Dispose();

                                foreach (var task in tasks)
                                {
                                    _notificationRepository.Insert(new Notifications
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

                                _notificationRepository.Insert(notif);
                                TempData["SuccessMsg"] = "Task #" + taskToEdit.Name + " has been updated";

                                _taskRepository.Save();
                                _taskRepository.Dispose();
                            }
                        }
                    }
                    else
                    {
                        var taskToEdit = _taskRepository.GetById(viewModel.Id);
                        taskToEdit.PriorityId = viewModel.PriorityId;
                        taskToEdit.Name = viewModel.TaskName;
                        taskToEdit.StatusId = viewModel.StatusId;
                        taskToEdit.AssignToId = viewModel.AssignToId;
                        taskToEdit.DueDate = viewModel.DueDate;
                        taskToEdit.Description = viewModel.TaskDescription;
                        _taskRepository.Update(taskToEdit);

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
                                            AssignFromId = User.Identity.GetUserId(),
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
                                            AssignFromId = User.Identity.GetUserId(),
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
                                            AssignFromId = User.Identity.GetUserId(),
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
                                            AssignFromId = User.Identity.GetUserId(),
                                        });

                                    }
                                    break;
                            }
                            foreach (var task in tasks)
                            {
                                _taskRepository.Insert(task);
                            }

                            _taskRepository.Save();
                            _taskRepository.Dispose();

                            foreach (var task in tasks)
                            {
                                _notificationRepository.Insert(new Notifications
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

                            _notificationRepository.Insert(notif);
                            TempData["SuccessMsg"] = "Task #" + taskToEdit.Name + " has been updated";

                            _taskRepository.Save();
                            _taskRepository.Dispose();
                        }
                    }
                }
                _notificationRepository.Save();
                _notificationRepository.Dispose();
                return RedirectToAction("Index", "Task");
            }

            viewModel.PriorityList = _prioritiesRepository.GetAll().ToList();
            viewModel.StatusList = _statusesRepository.GetAll().ToList();
            viewModel.AssigneeList = _userStore.Users.ToList();
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public ActionResult EditTask(Guid Id)
        {
            var task = _taskRepository.GetById(Id);
            EditTaskViewModel viewModel = new EditTaskViewModel
            {
                Id = task.Id,
                TaskName = task.Name,
                TaskDescription = task.Description,
                StatusList = _statusesRepository.GetAll().ToList(),
                StatusId = task.StatusId,
                PriorityList = _prioritiesRepository.GetAll().ToList(),
                PriorityId = task.PriorityId,
                DueDate = task.DueDate,
                AssignToId = task.AssignToId,
                AssigneeList = _userStore.Users.ToList(),
                EndDate = null,
                StartDate = null,
                DependencyId = task.DependencyId,
            };

            return View("NewTask", viewModel);
        }

        public ActionResult DeleteTask(Guid Id)
        {
            var taskInDb = _taskRepository.GetById(Id);
            if (taskInDb != null)
            {
                var notif = _notificationRepository.GetByTaskId(taskInDb.Id.Value);

                if (notif != null)
                {
                    _notificationRepository.Delete(notif);
                    _notificationRepository.Save();
                }

                _notificationRepository.Insert(new Notifications
                {
                    CreatedAt = DateTime.Now,
                    Description = taskInDb.Name + " task's has been deleted by " + User.Identity.GetUserName(),
                    Status = "New",
                    TasksId = null,
                    UserId = taskInDb.AssignToId
                });

                _taskRepository.Delete(taskInDb);
            }
            else
            {
                TempData["ErrorMsg"] = "The task you trying to delete is not found in the database.";
            }

            _taskRepository.Save();
            _notificationRepository.Save();

            _notificationRepository.Dispose();
            _taskRepository.Dispose();

            TempData["ErrorMsg"] = "Task has been deleted successfully";
            return RedirectToAction("Index", "Task");
        }

        public ActionResult ImportTask()
        {
            ImportTaskViewModel viewModel = new ImportTaskViewModel()
            {
                ImportInfo = new List<TaskImportInfo>()
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult ImportTask(HttpPostedFileBase file)
        {
            ImportTaskViewModel viewModel = new ImportTaskViewModel
            {
                ImportInfo = new List<TaskImportInfo>()
            };
            if (file != null)
            {
                StreamReader csvReader = new StreamReader(file.InputStream);
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
                        DueDate = DateTime.Parse(value[4]),
                        AssignToUser = value[5]
                    });

                }
                TempData["SuccessMsg"] = "Data retrieved from CSV successfully!.";
            }
            else
            {
                TempData["ErrorMsg"] = "Oops! something went wrong, Did you input any file?";
            }
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult SyncToDB(ImportTaskViewModel viewModel)
        {

            if (ModelState.IsValid)
            {
                int i = 1;
                foreach (var item in viewModel.ImportInfo)
                {
                    var status = _statusesRepository.GetByName(item.Status);
                    var priority = _prioritiesRepository.GetByName(item.PriorityLevel);
                    var user = _userStore.Users.SingleOrDefault(u => u.UserName == item.AssignToUser);

                    var task = new Tasks
                    {
                        Name = item.Name + " - " + i,
                        StatusId = status.Id,
                        PriorityId = priority.Id,
                        DueDate = item.DueDate,
                        Description = item.Description,
                        Created = DateTime.Now,
                        AssignToId = user.Id,
                        AssignFromId = User.Identity.GetUserId(),
                    };

                    _taskRepository.Insert(task);
                    i++;
                }

                _taskRepository.Save();
                _taskRepository.Dispose();
                TempData["SuccessMsg"] = viewModel.ImportInfo.Count + " tasks imported successfully.";
                return RedirectToAction("Index", "Task");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return RedirectToAction("ImportTask", "Task", viewModel);
        }

        public ActionResult ManageDependency(Guid Id)
        {
            var taskToAssign = _taskRepository.GetById(Id);
            var listOfTask = _taskRepository.GetAll().Where(u => u.Id != taskToAssign.Id);

            ManageTaskDependencyViewModel viewModel = new ManageTaskDependencyViewModel
            {
                CurrentTask = taskToAssign.Name,
                ListOfTasks = listOfTask.ToList(),
                DependencyID = taskToAssign.DependencyId,
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult ManageDependency(ManageTaskDependencyViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Retrieve Task In DB
                var taskInDB = _taskRepository.GetByTaskName(viewModel.CurrentTask).SingleOrDefault();

                taskInDB.DependencyId = viewModel.DependencyID;
                _taskRepository.Update(taskInDB);
                _taskRepository.Save();
                _taskRepository.Dispose();
                TempData["SuccessMsg"] = "Dependency assigned successfully!";
                return RedirectToAction("Index", "Task");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong. Please go thru the error message.";
            return View(viewModel);
        }
    }
}