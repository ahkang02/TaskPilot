using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Web.DTOs;

namespace TaskPilot.Web.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<TaskDTO> GetTasks()
        {
            var task = _unitOfWork.Tasks.GetAllInclude(null, includeProperties:"Status,Priority,AssignTo");
            List<TaskDTO> tasks = new List<TaskDTO>();
            foreach (var t in task)
            {
                tasks.Add(new TaskDTO
                {
                    Id = t.Id,
                    TaskName = t.Name,
                    Priority = t.Priority!.Description,
                    Status = t.Status!.Description,
                    AssignTo = t.AssignTo!.UserName!,
                    dueDate = t.DueDate
                });
            }
            return tasks;
        }


    }
}
