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
        private TaskRepository _taskRepository;


        public PriorityController()
        {
            _prioritiesRepository = new PrioritiesRepository(new TaskContext());
            _taskRepository = new TaskRepository(new TaskContext());
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

        [HttpDelete]
        public IHttpActionResult Delete(string id)
        {
            var priorityInDb = _prioritiesRepository.GetByName(id);
            if (priorityInDb != null)
            {
                if(_taskRepository.GetAll().Any(p => p.PriorityId ==  priorityInDb.Id))
                {
                    return BadRequest();
                }else
                {
                    _prioritiesRepository.Delete(priorityInDb);
                }
            }else
            {
                return NotFound();
            }
            _prioritiesRepository.Save();
            _prioritiesRepository.Dispose();
            return Ok();
        }


    }
}
