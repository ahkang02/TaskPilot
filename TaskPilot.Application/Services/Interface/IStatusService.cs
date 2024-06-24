using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Interface
{
    public interface IStatusService
    {
        IEnumerable<Statuses> GetAllStatuses();
        Statuses GetStatusById(Guid Id);

        Statuses GetStatusByName(string name);

        void CreateStatus(Statuses status);
        void UpdateStatus(Statuses status);
        void DeleteStatus(Statuses status);

        bool CheckIfStatusIsInUse(Statuses status);


    }
}
