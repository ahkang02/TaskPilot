using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Interface
{
    public interface IFeatureService
    {
        Features GetFeaturesById(Guid Id);

        IEnumerable<Features> GetAllFeatures();

        IEnumerable<string> GetAlLFeaturesSelectName();

    }
}
