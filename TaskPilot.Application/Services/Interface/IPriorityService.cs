using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Interface
{
    public interface IPriorityService
    {
        IEnumerable<Priorities> GetAllPriority();

        Priorities GetPrioritiesById(Guid Id);

        Priorities GetPrioritiesByName(string name);

        void CreatePriority(Priorities priority);

        void UpdatePriority(Priorities priority);

        void DeletePriority(Priorities priority);

        bool CheckIfPriorityIsInUse(Priorities priority);
    }
}
