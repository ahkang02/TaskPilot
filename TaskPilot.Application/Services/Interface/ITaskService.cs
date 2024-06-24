using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Interface
{
    public interface ITaskService
    {
        IEnumerable<Tasks> GetAllTaskWithInclude();

        IEnumerable<Tasks> GetNotClosedTaskSortByCreatedDateInDescFilterByUserId(string userId);

    }
}
