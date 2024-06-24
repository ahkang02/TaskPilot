using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Implementation
{
    public class FeatureService : IFeatureService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeatureService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Features> GetAllFeatures()
        {
            return _unitOfWork.Features.GetAll();
        }

        public IEnumerable<string> GetAlLFeaturesSelectName()
        {
            return _unitOfWork.Features.GetAll().Select(f => f.Name);
        }

        public Features GetFeaturesById(Guid Id)
        {
            return _unitOfWork.Features.Get(f => f.Id == Id);
        }
    }
}
