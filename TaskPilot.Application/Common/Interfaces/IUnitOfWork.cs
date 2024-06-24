namespace TaskPilot.Application.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ITaskRepository Tasks { get; }

        IRoleRepository Roles { get; }

        IUserRepository Users { get; }

        IPermissionRepository Permissions { get; }

        IPriorityRepository Priority { get; }

        IFeatureRepository Features { get; }

        IStatusRepository Status { get; }

        INotificationRepository Notification { get; }

        void Save();
    }
}
