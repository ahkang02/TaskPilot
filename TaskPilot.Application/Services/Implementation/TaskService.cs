using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Implementation
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Tasks> GetNotClosedTaskSortByCreatedDateInDescFilterByUserId(string userId)
        {
            return _unitOfWork.Tasks.GetAllInclude(u => u.AssignToId == userId && u.Status!.Description != "Closed", "Status,Priority,AssignFrom,AssignTo").OrderByDescending(u => u.Created);
        }

        public IEnumerable<Tasks> GetAllTaskWithInclude()
        {
            return _unitOfWork.Tasks.GetAllInclude(null, "Status,Priority,AssignFrom,AssignTo");
        }

        public bool IsUserHoldingTask(string userId)
        {
            return _unitOfWork.Tasks.GetAll().Any(u => u.AssignToId == userId);
        }

        public Tasks GetTasksWithId(Guid Id)
        {
            return _unitOfWork.Tasks.GetAllInclude(t => t.Id == Id, "Status,Priority,AssignFrom,AssignTo").Single();
        }

        public void CreateTask(Tasks task)
        {
            _unitOfWork.Tasks.Add(task);
            _unitOfWork.Save();
        }

        public void UpdateTask(Tasks task)
        {
            _unitOfWork.Tasks.Update(task);
            _unitOfWork.Save();
        }

        public void DeleteTask(Tasks task)
        {
            _unitOfWork.Tasks.Remove(task);
            _unitOfWork.Save();
        }

        public void AddRangeTasks(IEnumerable<Tasks> tasks)
        {
            _unitOfWork.Tasks.AddRange(tasks);
            _unitOfWork.Save();
        }

        public void DeleteRangeTasks(IEnumerable<Tasks> tasks)
        {
            _unitOfWork.Features.RemoveRange(tasks);
            _unitOfWork.Save();
        }

        public Tasks GetTasksByTaskName(string name)
        {
            return _unitOfWork.Tasks.Get(t => t.Name == name);
        }

        public IEnumerable<Tasks> GetTasksFilterById(Guid Id)
        {
            return _unitOfWork.Tasks.Find(t => t.Id != Id);
        }
    }
}
