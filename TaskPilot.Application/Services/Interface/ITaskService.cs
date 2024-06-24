using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Interface
{
    public interface ITaskService
    {
        IEnumerable<Tasks> GetAllTaskWithInclude();

        IEnumerable<Tasks> GetNotClosedTaskSortByCreatedDateInDescFilterByUserId(string userId);

        bool IsUserHoldingTask(string userId);

        Tasks GetTasksWithId(Guid Id);

        void CreateTask(Tasks task);

        void UpdateTask(Tasks task);

        void DeleteTask(Tasks task);

        void AddRangeTasks(IEnumerable<Tasks> tasks);

        void DeleteRangeTasks(IEnumerable<Tasks> tasks);

        Tasks GetTasksByTaskName(string name);

        IEnumerable<Tasks> GetTasksFilterById(Guid Id);
    }
}
