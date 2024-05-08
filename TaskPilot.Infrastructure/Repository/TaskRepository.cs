using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Data;

namespace TaskPilot.Infrastructure.Repository
{
    public class TaskRepository : Repository<Tasks>, ITaskRepository
    { 
        public TaskRepository(TaskContext context) : base(context) { 
        }

    }
}
