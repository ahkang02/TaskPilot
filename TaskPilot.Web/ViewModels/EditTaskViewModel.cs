using System.ComponentModel.DataAnnotations;
using TaskPilot.Application.Common.Utility.CustomValidator;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.ViewModels
{
    public class EditTaskViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string TaskName { get; set; }

        [Required]
        [Display(Name = "Task Description")]
        public string TaskDescription { get; set; }

        [Required]
        [Display(Name = "Priority Indicator")]
        public Guid PriorityId { get; set; }

        public List<Priorities> PriorityList { get; set; }

        [Required]
        [Display(Name = "Task Status")]
        public Guid StatusId { get; set; }

        public List<Statuses> StatusList { get; set; }

        [Display(Name = "Due Date")]
        [DateLessThanToday(ErrorMessage = "Due date cannot be in the past")]
        public DateTime? DueDate { get; set; }

        [Required]
        [Display(Name = "Assign To")]
        public string AssignToId { get; set; }

        public List<ApplicationUser> AssigneeList { get; set; }

        public Nullable<Guid> DependencyId { get; set; }

        #region Recurring

        [Display(Name = "Recurring Task")]
        public bool IsRecurring { get; set; }

        public string RecurringType { get; set; }

        [Display(Name = "Recurring Start")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Recurring End")]
        public DateTime? EndDate { get; set; }

        #endregion
    }
}
