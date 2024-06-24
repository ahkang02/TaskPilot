using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Infrastructure.Data;

namespace TaskPilot.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TaskContext _context;

        public UnitOfWork(TaskContext context)
        {
            _context = context;
            Tasks = new TaskRepository(_context);
            Roles = new RoleRepository(_context);
            Permissions = new PermissionRepository(_context);
            Priority = new PriorityRepository(_context);
            Features = new FeatureRepository(_context);
            Status = new StatusRepository(_context);
            Notification = new NotificationRepository(_context);
            Users = new UserRepository(_context);
        }

        public ITaskRepository Tasks { get; private set; }

        public IRoleRepository Roles { get; private set; }

        public IUserRepository Users { get; private set; }

        public IPermissionRepository Permissions { get; private set; }

        public IPriorityRepository Priority { get; private set; }

        public IFeatureRepository Features { get; private set; }

        public IStatusRepository Status { get; private set; }

        public INotificationRepository Notification { get; private set; }

        public void Dispose()
        {
            _context.Dispose();
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
