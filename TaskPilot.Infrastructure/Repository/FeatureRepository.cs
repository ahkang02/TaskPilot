using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Data;

namespace TaskPilot.Infrastructure.Repository
{
    public class FeatureRepository : Repository<Features>, IFeatureRepository
    {
        public FeatureRepository(TaskContext context) : base(context)
        {

        }
    }
}
