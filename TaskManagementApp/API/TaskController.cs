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
        private readonly TaskContext _taskContext;
        private readonly TaskRepository _taskRepository;

        public TaskController()
        {
            _taskContext = TaskContext.Create();
            _taskRepository = new TaskRepository(_taskContext);
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
