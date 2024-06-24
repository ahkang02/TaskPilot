using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Web.DTOs;

namespace TaskPilot.Web.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<ChartDTO> GetDueTasks()
        {
            var username = User.Identity!.Name;
            var currentUser = _unitOfWork.Users.Get(u => u.UserName == username);

            // Getting All The Task & Group Them Based On Respective Date
            var dueTasks = _unitOfWork.Tasks.GetAllInclude(u => u.AssignToId == currentUser.Id, null).GroupBy(x => x.DueDate).Select(x => new { Date = x.Key, DueTasksCount = x.Count() }).ToList();
            var createdTask = _unitOfWork.Tasks.GetAllInclude(u => u.AssignToId == currentUser.Id, null).GroupBy(x => x.Created.Date).Select(x => new { Date = x.Key, CreatedTasksCount = x.Count() }).ToList();
            var resolvedTask = _unitOfWork.Tasks.GetAllInclude(u => u.AssignToId == currentUser.Id, "Status").Where(x => x.Status!.Description == "Closed").GroupBy(x => x.Updated!.Value.Date).Select(x => new { Date = x.Key, ResolvedTasksCount = x.Count() }).ToList();

            // Combine All The Dates, Remove Duplicates
            var combinedDates = dueTasks.Select(x => x.Date.GetValueOrDefault()).Union(createdTask.Select(x => x.Date)).Union(resolvedTask.Select(x => x.Date)).Distinct().ToList();
            List<ChartDTO> data = new List<ChartDTO>();

            // Merge All Three List Into One
            data = combinedDates.Select(date => new ChartDTO
            {
                Date = date.Date.ToString("yyyy-MM-dd"),
                dueTasksCount = dueTasks.FirstOrDefault(x => x.Date == date)?.DueTasksCount ?? 0,
                createdTasksCount = createdTask.FirstOrDefault(x => x.Date == date)?.CreatedTasksCount ?? 0,
                resolvedTasksCount = resolvedTask.FirstOrDefault(x => x.Date == date)?.ResolvedTasksCount ?? 0
            }).ToList();

            return data;
        }

    }
}
