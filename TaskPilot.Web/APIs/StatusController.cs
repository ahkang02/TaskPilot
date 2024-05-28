using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Web.DTOs;

namespace TaskPilot.Web.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;

        public StatusController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<StatusDTO> GetStatuses()
        {
            var status = _unitOfWork.Status.GetAll();
            List<StatusDTO> statusDTOs = new List<StatusDTO>();

            foreach (var stat in status)
            {
                statusDTOs.Add(new StatusDTO
                {
                    Id = stat.Id,
                    CreatedAt = stat.CreatedAt,
                    Description = stat.Description,
                    UpdatedAt = stat.UpdatedAt,
                });

            }
            return statusDTOs;
        }

    }
}
