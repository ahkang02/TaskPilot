using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Web.DTOs;

namespace TaskPilot.Web.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriorityController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PriorityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<PriorityDTO> GetPriorities()
        {
            var priorities = _unitOfWork.Priority.GetAll();
            List<PriorityDTO> priorityDTOs = new List<PriorityDTO>();
            foreach (var prior in priorities)
            {
                priorityDTOs.Add(new PriorityDTO
                {
                    Id = prior.Id,
                    CreatedAt = prior.CreatedAt,
                    Description = prior.Description,
                    UpdatedAt = prior.UpdatedAt,
                });
            }

            return priorityDTOs;
        }
    }
}
