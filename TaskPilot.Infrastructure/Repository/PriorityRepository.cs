using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Data;

namespace TaskPilot.Infrastructure.Repository
{
    public class PriorityRepository : Repository<Priorities>, IPriorityRepository
    {
        public PriorityRepository(TaskContext context) : base(context)
        {

        }
    }
}
