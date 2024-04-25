using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TaskManagementApp.DAL;
using TaskManagementApp.DTO;
using TaskManagementApp.Models;

namespace TaskManagementApp.API
{
    public class StatusController : ApiController
    {
        private StatusesRepository _statusesRepository;
        private TaskRepository _taskRepository;
        public StatusController() {
            _statusesRepository = new StatusesRepository(new TaskContext());
            _taskRepository = new TaskRepository(new TaskContext());    
        }

        public IEnumerable<StatusDTO> GetStatuses()
        {
            var status = _statusesRepository.GetAll();
            List<StatusDTO> statusDTOs = new List<StatusDTO>();
            foreach(var stat in status)
            {
                statusDTOs.Add(new StatusDTO
                {
                    CreatedAt = stat.CreatedAt,
                    Description = stat.Description,
                    UpdatedAt = stat.UpdatedAt,
                });

            }
            return statusDTOs;
        }

        public IHttpActionResult Delete(string Id)
        {
            var status = _statusesRepository.GetByName(Id);
            if (status != null)
            {
                if (_taskRepository.GetAll().Any(c => c.StatusId == status.Id))
                {
                    return BadRequest();
                }
                else
                {
                    _statusesRepository.Delete(status);
                }
            }
            else
            {
                return NotFound();
            }
            _statusesRepository.Save();
            _statusesRepository.Dispose();
            return Ok();    
        }

    }
}
