using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TaskManagementApp.DAL;
using TaskManagementApp.DTO;
using TaskManagementApp.Models;

namespace TaskManagementApp.API
{
    public class TaskController : ApiController
    {
        private TaskContext _taskContext;
        private TaskRepository _taskRepository;
        private StatusesRepository _statusesRepository;
        private NotificationRepository _notificationRepository;

        public TaskController()
        {
            _taskContext = TaskContext.Create();
            _taskRepository = new TaskRepository(_taskContext);
            _statusesRepository = new StatusesRepository(_taskContext);
            _notificationRepository = new NotificationRepository(_taskContext);
        }

        public IEnumerable<TaskDTO> GetAllTasks()
        {
            var task = _taskRepository.GetAllInclude(includeProperties: "Status,Priority,AssignTo");
            List<TaskDTO> tasks = new List<TaskDTO>();
            foreach (var t in task)
            {
                tasks.Add(new TaskDTO
                {
                    Id = t.Id,
                    TaskName = t.Name,
                    Priority = t.Priority.Description,
                    Status = t.Status.Description,
                    AssignTo = t.AssignTo.UserName,
                    dueDate = t.DueDate
                });
            }
            return tasks;
        }

    }
}
