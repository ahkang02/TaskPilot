using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TaskManagementApp.DAL;
using TaskManagementApp.DTO;

namespace TaskManagementApp.API
{
    public class PriorityController : ApiController
    {
        private PrioritiesRepository _prioritiesRepository;

        public PriorityController()
        {
            _prioritiesRepository = new PrioritiesRepository(new TaskContext());
        }

        public IEnumerable<PriorityDTO> GetPriorities()
        {
            var priorities = _prioritiesRepository.GetAll();
            List<PriorityDTO>  priorityDTOs = new List<PriorityDTO>();
            foreach (var prior in priorities)
            {
                priorityDTOs.Add(new PriorityDTO
                {
                    CreatedAt = prior.CreatedAt,
                    Description = prior.Description,
                    UpdatedAt = prior.UpdatedAt,
                });
            }

            return priorityDTOs;
        }
    }
}
