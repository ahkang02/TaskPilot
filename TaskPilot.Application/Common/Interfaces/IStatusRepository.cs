using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Common.Interfaces
{
    public interface IStatusRepository : IRepository<Statuses>
    {
    }
}
