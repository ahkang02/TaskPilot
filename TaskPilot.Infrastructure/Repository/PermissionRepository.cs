using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Data;

namespace TaskPilot.Infrastructure.Repository
{
    public class PermissionRepository : Repository<Permission>, IPermissionRepository
    {
        public PermissionRepository(TaskContext context) : base(context)
        {

        }
    }
}
