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

        [HttpDelete]
        public IHttpActionResult Delete(Guid Id)
        {
            var taskInDb = _taskRepository.GetById(Id);
            if (taskInDb != null)
            {
                var notif = _notificationRepository.GetByTaskId(taskInDb.Id.Value);

                if(notif != null)
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
                return NotFound();
            }

            _taskRepository.Save();
            _notificationRepository.Save();

            _notificationRepository.Dispose();
            _taskRepository.Dispose();
            return Ok();
        }

        [HttpPut]
        public IHttpActionResult UpdateTaskComplete(Guid Id)
        {
            var task = _taskRepository.GetById(Id);
            var status = _statusesRepository.GetByName("Closed");
            if (task != null)
            {
                _notificationRepository.Insert(new Notifications
                {
                    CreatedAt = DateTime.Now,
                    Description = task.Name + " task's has been closed",
                    TasksId = task.Id,
                    Status = "New",
                    UserId = task.AssignToId
                });
                task.StatusId = status.Id;
            }
            else
            {
                return NotFound();
            }

            _notificationRepository.Save();
            _notificationRepository.Dispose();  

            _taskRepository.Update(task);
            _taskRepository.Save();
            _taskRepository.Dispose();
            return Ok();
        }

    }
}
