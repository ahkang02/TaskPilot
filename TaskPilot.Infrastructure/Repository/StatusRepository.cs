using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Data;

namespace TaskPilot.Infrastructure.Repository
{
    public class StatusRepository : Repository<Statuses>, IStatusRepository
    {
        public StatusRepository(TaskContext context) : base(context)
        {

        }
    }
}
