using System.ComponentModel.DataAnnotations;
using TaskPilot.Application.Common.Utility.CustomValidator;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.ViewModels
{
    public class TaskImportInfo
    {
        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        [DateLessThanToday(ErrorMessage = "Due date cannot be in the past")]
        public DateTime? DueDate { get; set; }

        [Required]
        public string? PriorityLevel { get; set; }

        public Guid? PriorityId { get; set; }

        public List<Priorities>? PriorityList {  get; set; }

       
        public string? Status { get; set; }

        public Guid? StatusId { get; set; }

        public List<Statuses>? StatusList { get; set; }

        [Required]
        public string? AssignToUser { get; set; }

        public string? UserId { get; set; }

        public List<ApplicationUser>? AssigeeList {  get; set; }
    }
}
