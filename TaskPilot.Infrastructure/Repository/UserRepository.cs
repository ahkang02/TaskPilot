using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Data;

namespace TaskPilot.Infrastructure.Repository
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        public UserRepository(TaskContext context) : base(context)
        {

        }
    }
}
