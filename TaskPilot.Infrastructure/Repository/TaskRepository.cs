using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Data;

namespace TaskPilot.Infrastructure.Repository
{
    public class TaskRepository : Repository<Tasks>, ITaskRepository
    {
        public TaskRepository(TaskContext context) : base(context)
        {
        }

    }
}
