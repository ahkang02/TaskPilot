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
