using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using Vonage.Users;

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
    }
}
