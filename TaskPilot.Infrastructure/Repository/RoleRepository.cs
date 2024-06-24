using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Data;

namespace TaskPilot.Infrastructure.Repository
{
    public class RoleRepository : Repository<ApplicationRole>, IRoleRepository
    {
        public RoleRepository(TaskContext context) : base(context) { }
    }
}
